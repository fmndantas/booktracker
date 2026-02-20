module App.Workflow

open System
open System.Diagnostics

open Spectre.Console

open CommonTypes
open SpectreWrapper

type Mark = {
  Start: string -> unit
  End: string -> unit
}

type BookDto = {
  Title: string
  Author: string option
  MainTopic: string option
  Filepath: string option
}

type CrudResult =
  | Create
  | Edit
  | List
  | Delete of DeleteResult

and DeleteResult =
  | Confirmed
  | Declined

[<AutoOpen>]
module Helpers =
  let string2Option (evaluateAsNone: string -> bool) (s: string) : string option =
    if evaluateAsNone s then None else Some s

  let stringIsNoneIfEmpty = string2Option (fun s -> s.Length = 0)

  let stringIsNoneIfHasValue v = string2Option (fun s -> s = v)

  let showDateTime (v: DateTime) = v.ToString "yyyy/MM/dd HH:mm:ss"

  let selectBook (conn: Context.BooktrackerConnection) : Result<BookId, AppError list> =
    let options =
      conn
      |> Query.getBooksOrderedByLastReadingLog
      |> List.map (fun x -> x.Id, x.Title)

    if options.Length = 0 then
      Error[BusinessError "You don't have any books saved"]
    else
      AnsiConsole.Prompt(
        SelectionPrompt<BookId * string>()
          .Title("[bold]Select book[/]")
          .UseConverter(snd)
          .AddChoices(options)
          .EnableSearch()
      )
      |> (fst >> Ok)

  let selectHook (conn: Context.BooktrackerConnection) : Result<HookId, AppError list> =
    let options = Query.getHooks conn

    if options.Length = 0 then
      Error[BusinessError "You don't have any hooks saved"]
    else
      AnsiConsole.Prompt(
        SelectionPrompt<HookId * string>()
          .Title("[bold]Select hook[/]")
          .UseConverter(snd)
          .AddChoices(options |> List.map (fun h -> h.Id, h.Name))
          .EnableSearch()
      )
      |> (fst >> Ok)

  let askBookDetails (bookFolder: string) (PlaceholderBook: BookDto option) : BookDto =
    let title, author, mainTopic =
      match PlaceholderBook with
      | None -> None, None, None
      | Some book -> Some book.Title, book.Author, book.MainTopic

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

let getBooks (conn: Context.BooktrackerConnection) (mark: Mark) : unit =
  mark.Start "get books"
  let books = Query.getBooksOrderedByLastReadingLog conn
  mark.End "get books"

  mark.Start "render table"

  aTable ()
  |> addColumns [| "Title"; "Author"; "Main topic"; "Filepath" |]
  |> addRows (
    books
    |> List.map (fun b -> [|
      b.Title
      b.Author |> Option.defaultValue "-"
      b.MainTopic |> Option.defaultValue "-"
      b.Filepath |> Option.defaultValue "-"
    |])
  )
  |> AnsiConsole.Write

  mark.End "render table"

let bookCrud (conn: Context.BooktrackerConnection) (bookFolder: string) (mark: Mark) : unit =
  result {
    let action =
      AnsiConsole.Prompt(
        aSelectionPrompt' ()
        |> addChoices [| "List"; "Create"; "Edit"; "Delete" |]
        |> enableSearch
      )

    return!
      match action with
      | "List" ->
        getBooks conn mark
        List |> Ok
      | "Create"
      | "Edit" ->
        result {
          let isCreate = action = "Create"

          let! placeholderBook =
            if isCreate then
              Ok None
            else
              result {
                let! bookId = selectBook conn
                let! book = Query.getBookById conn bookId
                return book |> Some
              }

          let bookDetails =
            askBookDetails
              bookFolder
              (placeholderBook
               |> Option.map (fun b -> {
                 Title = b.Title
                 Author = b.Author
                 MainTopic = b.MainTopic
                 Filepath = b.Filepath
               }))

          let createOrEditFn =
            if isCreate then
              Command.createBook conn
            else
              Command.updateBook conn placeholderBook.Value.Id

          let! _ =
            createOrEditFn
              bookDetails.Title
              bookDetails.Author
              bookDetails.MainTopic
              bookDetails.Filepath
              DateTime.UtcNow

          return if isCreate then Create else Edit
        }
      | "Delete" ->
        result {
          let! bookId = selectBook conn
          let! book = Query.getBookById conn bookId

          let confirm =
            AnsiConsole.Confirm(
              $"[bold yellow]Warning![/] Are you sure you want to delete [yellow]\"{book.Title}\"[/]? This will delete reading logs too!",
              false
            )

          if confirm then
            do! Command.deleteBook conn bookId
            return Delete Confirmed
          else
            return Delete Declined
        }
      | _ -> failwith "TODO"
  }
  |> function
    | Ok(Create | Edit) -> AnsiConsole.MarkupLine "[green]Book was saved successfully![/]"
    | Ok(Delete Confirmed) -> AnsiConsole.MarkupLine "[green]Book was deleted successfully![/]"
    | Ok _ -> ()
    | Error es -> showErrors es

let logReading (conn: Context.BooktrackerConnection) (mark: Mark) : unit =
  result {
    let! bookId = selectBook conn

    let initialPage =
      (conn, Some bookId)
      ||> Query.getLastReadingLogByBook
      |> Option.map (_.FinalPage >> int)
      |> ask "[bold]Initial page[/]?"

    let finalPage = ask' "[bold]Final page[/]?"
    let nextTopic = AnsiConsole.Prompt(aTextPrompt "[bold]Next topic[/]?" |> allowEmpty)

    return! Command.logReading conn bookId initialPage finalPage (stringIsNoneIfEmpty nextTopic) DateTime.UtcNow
  }
  |> function
    | Ok _ -> AnsiConsole.MarkupLine "[green]Reading log was saved successfully![/]"
    | Error es -> showErrors es

let getLastReadingLogsByBook (conn: Context.BooktrackerConnection) (mark: Mark) : unit =
  result {
    let! bookId = selectBook conn

    mark.Start "get reading logs"
    let readingLogs = Query.getReadingLogs conn (Some bookId)
    mark.End "get reading logs"

    mark.Start "table preparing"

    let table =
      aTable ()
      |> addColumns [| "Initial page"; "Final page"; "Next topic"; "When" |]
      |> addRows (
        readingLogs
        |> List.map (fun b -> [|
          b.InitialPage.ToString()
          b.FinalPage.ToString()
          b.NextTopic |> Option.defaultValue "-"
          b.Modified |> showDateTime
        |])
      )

    mark.End "table preparing"
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

let continueLastReading (conn: Context.BooktrackerConnection) (mark: Mark) : unit =
  result {
    let! hookId = selectHook conn
    let! bookId = selectBook conn

    let! readingLog =
      match Query.getLastReadingLogByBook conn (Some bookId) with
      | Some readingLog -> Ok readingLog
      | None -> Error[BusinessError "No reading logs found for this book"]

    mark.Start "get filled hook"
    let! command = Query.getHookCommandByReadingLog conn hookId readingLog.Id
    mark.End "get filled hook"

    let! _ = spawnProcess command

    return readingLog.NextTopic
  }
  |> function
    | Ok v ->
      v
      |> Option.iter (sprintf "Next topic: [green]\"%s\"[/]" >> AnsiConsole.MarkupLine)
    | Error es -> showErrors es
