module App.Workflow

open System
open System.Diagnostics

open Spectre.Console

open CommonTypes
open SqliteExtensions
open SpectreWrapper

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
  let string2Option (evaluateAsNone: string -> bool) (s: string) : string ValueOption =
    if evaluateAsNone s then ValueNone else ValueSome s

  let stringIsNoneIfEmpty = string2Option (fun s -> s.Length = 0)

  let stringIsNoneIfHasValue v = string2Option (fun s -> s = v)

  let showDateTime (v: DateTime) = v.ToString "yyyy/MM/dd HH:mm:ss"

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

  let askBookDetails (bookFolder: string) (v: CreateOrEditEntity<Context.Book>) : BookDto =
    let title, author, mainTopic =
      match v with
      | Create -> None, None, None
      | Edit book -> Some book.Title, book.Author |> Option.ofValueOption, book.MainTopic |> Option.ofValueOption

    let newTitle = ask "[bold]Title?[/]" title

    let newAuthor =
      AnsiConsole.Prompt(aTextPrompt "[bold]Author[/]?" |> allowEmpty |> defaultValueOption author)

    let newMainTopic =
      AnsiConsole.Prompt(aTextPrompt "[bold]Main topic[/]?" |> allowEmpty |> defaultValueOption mainTopic)

    let noFilepath = "Leave it blank"
    let files = [| noFilepath; yield! IO.Directory.GetFiles bookFolder |]

    let filepath =
      AnsiConsole.Prompt(aSelectionPrompt "[bold]File path[/]" |> addChoices files |> enableSearch)

    {
      Title = newTitle
      Author = stringIsNoneIfEmpty newAuthor
      MainTopic = stringIsNoneIfEmpty newMainTopic
      Filepath = stringIsNoneIfHasValue noFilepath filepath
    }

let createOrEditBook
  (readDataContext: Context.ReadDataContext)
  (dataContext: Context.DataContext)
  (bookFolder: string)
  : unit =
  result {
    let action =
      AnsiConsole.Prompt(aSelectionPrompt' () |> addChoices [| "Create"; "Edit" |] |> enableSearch)

    let! createOrEditBook =
      match action with
      | "Create" -> Create |> Ok
      | "Edit" ->
        result {
          let! bookId = selectBook readDataContext
          let! book = Query.getBookById readDataContext bookId
          return Edit book
        }
      | _ -> failwith "Unexpected action"

    AnsiConsole.MarkupLine "Enter [green]book[/] details!"

    let bookDetails = askBookDetails bookFolder createOrEditBook

    let createOrEditFn =
      match createOrEditBook with
      | Create -> Command.createBook dataContext
      | Edit book -> Command.updateBook dataContext book.Id

    return!
      createOrEditFn bookDetails.Title bookDetails.Author bookDetails.MainTopic bookDetails.Filepath DateTime.UtcNow
  }
  |> function
    | Ok _ -> sprintf "[green] Book was saved successfully![/]" |> AnsiConsole.MarkupLine
    | Error es -> showErrors es

let getBooks (dataContext: Context.ReadDataContext) : unit =
  let books = Query.getBooksOrderedByLastReadingLog dataContext |> Seq.toArray

  aTable ()
  |> addColumns [| "Title"; "Author"; "Main topic"; "Filepath" |]
  |> addRows (
    books
    |> Array.map (fun b -> [|
      b.Title |> ValueOption.defaultValue "-"
      b.Author |> ValueOption.defaultValue "-"
      b.MainTopic |> ValueOption.defaultValue "-"
      b.Filepath |> ValueOption.defaultValue "-"
    |])
  )
  |> AnsiConsole.Write

let logReading (readDataContext: Context.ReadDataContext) (dataContext: Context.DataContext) : unit =
  result {
    let! bookId = selectBook readDataContext

    let initialPage =
      (readDataContext, Some bookId)
      ||> Query.getLastReadingLogByBook
      |> Option.map (_.FinalPage >> int)
      |> ask "[bold]Initial page[/]?"

    let finalPage = ask' "[bold]Final page[/]?"

    let nextTopic = AnsiConsole.Prompt(aTextPrompt "[bold]Next topic[/]?" |> allowEmpty)

    let! result =
      Command.logReading dataContext bookId initialPage finalPage (stringIsNoneIfEmpty nextTopic) DateTime.UtcNow

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
      |> Seq.toArray

    return
      aTable ()
      |> addColumns [| "Initial page"; "Final page"; "Next topic"; "When" |]
      |> addRows (
        readingLogs
        |> Array.map (fun b -> [|
          b.InitialPage.ToString()
          b.FinalPage.ToString()
          b.NextTopic |> ValueOption.defaultValue "-"
          b.Modified.FromSqlite |> showDateTime
        |])
      )
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
