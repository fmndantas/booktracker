module App.Workflow

open System
open System.Diagnostics

open Spectre.Console

open CommonTypes
open SqliteExtensions

let stringOption (evaluateAsNone: string -> bool) (s: string) : string ValueOption =
  if evaluateAsNone s then ValueNone else ValueSome s

let stringOptionIfEmpty = stringOption (fun s -> s.Length = 0)

let stringOptionIfValue v = stringOption (fun s -> s = v)

let boldRed s = sprintf "[bold red]%s[/]" s

let item s = sprintf "\u2022 %s" s

let showDateTime (v: DateTime) = 
  v.ToString "yyyy/MM/dd HH:mm:ss"

let showErrors (es: CommonTypes.AppError list) =
  let errorItems =
    es
    |> List.map appErrorToString
    |> List.map (boldRed >> item)
    |> fun es -> String.Join('\n', es)

  AnsiConsole.MarkupLine(boldRed "Some errors ocurred:")
  AnsiConsole.MarkupLine errorItems

let selectBook (readDataContext: Context.ReadDataContext) : Result<BookId, AppError list> =
  let books =
    query {
      for book in readDataContext |> Query.getBooks do
        select (book.Id, book.Title)
    }
    |> Seq.toList

  if books.Length = 0 then
    Error[BusinessError "You don't have any book saved"]
  else
    AnsiConsole.Prompt(
      SelectionPrompt<int64 * string>().Title("[bold]Select book[/]").UseConverter(snd).AddChoices(books).EnableSearch()
    )
    |> fst
    |> Ok

let selectHook (readDataContext: Context.ReadDataContext) : Result<HookId, AppError list> =
  let hooks =
    query {
      for hook in readDataContext.Main.Hook do
        select (hook.Id, hook.Name)
    }
    |> Seq.toList

  if hooks.Length = 0 then
    Error[BusinessError "You don't have any hook saved"]
  else
    AnsiConsole.Prompt(
      SelectionPrompt<int64 * string>().Title("[bold]Select hook[/]").UseConverter(snd).AddChoices(hooks).EnableSearch()
    )
    |> fst
    |> Ok

let createBook (dataContext: Context.DataContext) (bookFolder: string) : unit =
  AnsiConsole.MarkupLine "Type [green]book[/] data!"
  let title = AnsiConsole.Ask<string> "[bold]Title[/]?"
  let author = AnsiConsole.Prompt(TextPrompt<string>("[bold]Author[/]?").AllowEmpty())

  let mainTopic =
    AnsiConsole.Prompt(TextPrompt<string>("[bold]Main topic[/]?").AllowEmpty())

  let noFilepath = "Leave it blank"

  let files = IO.Directory.GetFiles bookFolder |> Array.insertAt 0 noFilepath

  let filepath =
    AnsiConsole.Prompt(SelectionPrompt<string>().Title("[bold]File path[/]").AddChoices(files).EnableSearch())

  let result =
    Command.createBook
      dataContext
      title
      (stringOptionIfEmpty author)
      (stringOptionIfEmpty mainTopic)
      (stringOptionIfValue noFilepath filepath)
      DateTime.UtcNow

  match result with
  | Ok _ -> sprintf "[green] Book was saved successfully![/]" |> AnsiConsole.MarkupLine
  | Error es -> showErrors es

let getBooks (dataContext: Context.ReadDataContext) : unit =
  let table = Table().AddColumns("Title", "Author", "Main topic", "Filepath")

  dataContext
  |> Query.getBooks
  |> Seq.toList
  |> List.iter (fun b ->
    let values = [|
      b.Title
      b.Author |> ValueOption.defaultValue "-"
      b.MainTopic |> ValueOption.defaultValue "-"
      b.Filepath |> ValueOption.defaultValue "-"
    |]

    values |> table.AddRow |> ignore)

  AnsiConsole.Write table

let logReading (readDataContext: Context.ReadDataContext) (dataContext: Context.DataContext) : unit =
  result {
    let! bookId = selectBook readDataContext

    let initialPage = AnsiConsole.Ask<int> "[bold]Initial page[/]?"
    let finalPage = AnsiConsole.Ask<int> "[bold]Final page[/]?"

    let nextTopic =
      AnsiConsole.Prompt(TextPrompt<string>("[bold]Next topic[/]?").AllowEmpty())

    let! result =
      Command.logReading dataContext bookId initialPage finalPage (stringOptionIfEmpty nextTopic) DateTime.UtcNow

    return result
  }
  |> function
    | Ok _ -> ()
    | Error es -> showErrors es

let getLastReadingLogsByBook (readDataContext: Context.ReadDataContext) : unit =
  result {
    let! bookId = selectBook readDataContext

    let readingLogs =
      query {
        for readingLog in readDataContext |> Query.getReadingLogs do
          where (readingLog.IdBook = bookId)
          sortByDescending readingLog.Read
          select readingLog
      }
      |> Seq.toList

    let table = Table().AddColumns("Initial page", "Final page", "Next topic", "When")

    readingLogs
    |> List.iter (fun b ->
      let values = [|
        b.InitialPage.ToString()
        b.FinalPage.ToString()
        b.NextTopic |> ValueOption.defaultValue "-"
        b.Modified.FromSqlite |> showDateTime
      |]

      values |> table.AddRow |> ignore)

    AnsiConsole.Write table
  }
  |> function
    | Ok _ -> ()
    | Error es -> showErrors es


let spawnProcess (command: string * string) : Result<unit, AppError list> =
  result {
    let processInfo = ProcessStartInfo(fst command, snd command)
    processInfo.UseShellExecute <- false
    processInfo.RedirectStandardOutput <- false
    processInfo.RedirectStandardError <- false
    processInfo.RedirectStandardInput <- false
    let _ = Process.Start processInfo
    return ()
  }

let continueLastReading (readDataContext: Context.ReadDataContext) : unit =
  result {
    let! hookId = selectHook readDataContext
    let! bookId = selectBook readDataContext

    let! readingLogId =
      match Query.getLastReadingLogByBook readDataContext (Some bookId) with
      | Some readingLog -> readingLog.Id |> Ok
      | None -> Error[BusinessError "Book does not have log entries yet"]

    let! command = Query.getHookCommandByReadingLog readDataContext hookId readingLogId
    let! _ = spawnProcess command
    return ()
  }
  |> function
    | Error es -> showErrors es
    | _ -> ()
