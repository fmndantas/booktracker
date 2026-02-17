module App.Workflow

open System
open System.Diagnostics

open Spectre.Console

open CommonTypes
open SqliteExtensions

type BookDto = {
  Title: string
  Author: string ValueOption
  MainTopic: string ValueOption
  Filepath: string ValueOption
}

type CreateOrEditEntity<'T> =
  | Create
  | Edit of 'T

[<AutoOpen>]
module Helpers =
  let stringOption (evaluateAsNone: string -> bool) (s: string) : string ValueOption =
    if evaluateAsNone s then ValueNone else ValueSome s

  let stringOptionIfEmpty = stringOption (fun s -> s.Length = 0)

  let stringOptionIfValue v = stringOption (fun s -> s = v)

  let boldRed s = sprintf "[bold red]%s[/]" s

  let item s = sprintf "\u2022 %s" s

  let showDateTime (v: DateTime) = v.ToString "yyyy/MM/dd HH:mm:ss"

  let showErrors (es: CommonTypes.AppError list) =
    let errorItems =
      es
      |> List.map appErrorToString
      |> List.map (boldRed >> item)
      |> fun es -> String.Join('\n', es)

    AnsiConsole.MarkupLine(boldRed "Something went wrong:")
    AnsiConsole.MarkupLine errorItems

  let ask message (defaultValue: 'a option) =
    if defaultValue.IsSome then
      AnsiConsole.Ask(message, defaultValue.Value)
    else
      AnsiConsole.Ask message

  let selectBook (readDataContext: Context.ReadDataContext) : Result<BookId, AppError list> =
    let books =
      query {
        for book in readDataContext |> Query.getBooksOrderedByLastReadingLog do
          where (book.Id.IsSome && book.Title.IsSome)
          select (book.Id.Value, book.Title.Value)
      }
      |> Seq.toList

    if books.Length = 0 then
      Error[BusinessError "You don't have any books saved"]
    else
      AnsiConsole.Prompt(
        SelectionPrompt<int64 * string>()
          .Title("[bold]Select book[/]")
          .UseConverter(snd)
          .AddChoices(books)
          .EnableSearch()
      )
      |> (fst >> Ok)

  let selectHook (readDataContext: Context.ReadDataContext) : Result<HookId, AppError list> =
    let hooks =
      query {
        for hook in readDataContext.Main.Hook do
          select (hook.Id, hook.Name)
      }
      |> Seq.toList

    if hooks.Length = 0 then
      Error[BusinessError "You don't have any hooks saved"]
    else
      AnsiConsole.Prompt(
        SelectionPrompt<int64 * string>()
          .Title("[bold]Select hook[/]")
          .UseConverter(snd)
          .AddChoices(hooks)
          .EnableSearch()
      )
      |> (fst >> Ok)

  let askBookDetails (bookFolder: string) (entity: CreateOrEditEntity<Context.Book>) : BookDto =
    let title, author, mainTopic =
      match entity with
      | Create -> "", "", ""
      | Edit book ->
        book.Title, book.Author |> ValueOption.defaultValue "", book.MainTopic |> ValueOption.defaultValue ""

    let newTitle = ask "[bold]Title?[/]" (Some title)

    let newAuthor =
      AnsiConsole.Prompt(TextPrompt<string>("[bold]Author[/]?").AllowEmpty().DefaultValue author)

    let newMainTopic =
      AnsiConsole.Prompt(TextPrompt<string>("[bold]Main topic[/]?").AllowEmpty().DefaultValue mainTopic)

    let noFilepath = "Leave it blank"
    let files = [| noFilepath; yield! IO.Directory.GetFiles bookFolder |]

    let filepath =
      AnsiConsole.Prompt(SelectionPrompt<string>().Title("[bold]File path[/]").AddChoices(files).EnableSearch())

    {
      Title = newTitle
      Author = stringOptionIfEmpty newAuthor
      MainTopic = stringOptionIfEmpty newMainTopic
      Filepath = stringOptionIfValue noFilepath filepath
    }

let createOrEditBook
  (readDataContext: Context.ReadDataContext)
  (dataContext: Context.DataContext)
  (bookFolder: string)
  : unit =
  result {
    let action =
      AnsiConsole.Prompt(SelectionPrompt<string>().AddChoices([| "Create"; "Edit" |]).EnableSearch())

    let! createOrEditBook =
      match action with
      | "Create" -> Create |> Ok
      | "Edit" ->
        readDataContext
        |> selectBook
        |> Result.bind (Query.getBookById readDataContext)
        |> Result.map Edit
      | _ -> failwith "Unexpected case"

    AnsiConsole.MarkupLine "Enter [green]book[/] details!"

    let bookDetails = askBookDetails bookFolder createOrEditBook

    let createOrEdit =
      match createOrEditBook with
      | Create -> Command.createBook dataContext
      | Edit book -> Command.updateBook dataContext book.Id

    return! createOrEdit bookDetails.Title bookDetails.Author bookDetails.MainTopic bookDetails.Filepath DateTime.UtcNow
  }
  |> function
    | Ok _ -> sprintf "[green] Book was saved successfully![/]" |> AnsiConsole.MarkupLine
    | Error es -> showErrors es

let getBooks (dataContext: Context.ReadDataContext) : unit =
  let table = Table().AddColumns("Title", "Author", "Main topic", "Filepath")

  dataContext
  |> Query.getBooksOrderedByLastReadingLog
  |> Seq.toList
  |> List.iter (fun b ->
    let values = [|
      b.Title |> ValueOption.defaultValue "-"
      b.Author |> ValueOption.defaultValue "-"
      b.MainTopic |> ValueOption.defaultValue "-"
      b.Filepath |> ValueOption.defaultValue "-"
    |]

    values |> table.AddRow |> ignore)

  AnsiConsole.Write table

let logReading (readDataContext: Context.ReadDataContext) (dataContext: Context.DataContext) : unit =
  result {
    let! bookId = selectBook readDataContext

    let initialPage =
      (readDataContext, Some bookId)
      ||> Query.getLastReadingLogByBook
      |> Option.map _.FinalPage
      |> ask "[bold]Initial page[/]?"
      |> int

    let finalPage = AnsiConsole.Ask<int> "[bold]Final page[/]?"

    let nextTopic =
      AnsiConsole.Prompt(TextPrompt<string>("[bold]Next topic[/]?").AllowEmpty())

    let! result =
      Command.logReading dataContext bookId initialPage finalPage (stringOptionIfEmpty nextTopic) DateTime.UtcNow

    return result
  }
  |> function
    | Ok _ -> AnsiConsole.MarkupLine "[green]Reading log was saved successfully![/]"
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

    return table
  }
  |> function
    | Ok v -> AnsiConsole.Write v
    | Error es -> showErrors es

let spawnProcess (command: string * string) : Result<unit, AppError list> =
  try
    let processInfo = ProcessStartInfo(fst command, snd command)
    processInfo.UseShellExecute <- false
    processInfo.CreateNoWindow <- true
    processInfo.RedirectStandardOutput <- true
    processInfo.RedirectStandardError <- true
    processInfo.RedirectStandardInput <- false
    let p = Process.Start processInfo
    p.OutputDataReceived.Add ignore
    p.ErrorDataReceived.Add ignore
    p.EnableRaisingEvents <- true
    p.Exited.Add(fun _ -> p.Dispose())

    if p.Start() then
      p.BeginOutputReadLine()
      p.BeginErrorReadLine()
      Ok()
    else
      Error [ HookError "Process could not be started" ]
  with ex ->
    Error [ HookError ex.Message ]

let continueLastReading (readDataContext: Context.ReadDataContext) : unit =
  result {
    let! hookId = selectHook readDataContext
    let! bookId = selectBook readDataContext

    let! readingLog =
      match Query.getLastReadingLogByBook readDataContext (Some bookId) with
      | Some readingLog -> readingLog |> Ok
      | None -> Error[BusinessError "No reading logs found for this book"]

    let! command = Query.getHookCommandByReadingLog readDataContext hookId readingLog.Id
    let! _ = spawnProcess command

    return readingLog.NextTopic
  }
  |> function
    | Ok v ->
      v
      |> ValueOption.iter (sprintf "Next topic: [green]\"%s\"[/]" >> AnsiConsole.MarkupLine)
    | Error es -> showErrors es
