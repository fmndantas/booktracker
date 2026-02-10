module App.Workflow

open System

open Spectre.Console

let stringOption (evaluateAsNone: string -> bool) (s: string) : string ValueOption =
  if evaluateAsNone s then ValueNone else ValueSome s

let stringOptionIfEmpty = stringOption (fun s -> s.Length = 0)

let stringOptionIfValue v = stringOption (fun s -> s = v)

let createBook (dataContext: Context.DataContext) (bookFolder: string) : Async<unit> =
  async {
    AnsiConsole.MarkupLine "Type [green]book[/] data!"
    let title = AnsiConsole.Ask<string> "[bold]Title[/]?"
    let author = AnsiConsole.Prompt(TextPrompt<string>("[bold]Author[/]?").AllowEmpty())

    let mainTopic =
      AnsiConsole.Prompt(TextPrompt<string>("[bold]Main topic[/]?").AllowEmpty())

    let noFilepath = "Leave it blank"

    let files = IO.Directory.GetFiles bookFolder |> Array.insertAt 0 noFilepath

    let filepath =
      AnsiConsole.Prompt(SelectionPrompt<string>().Title("[bold]File path[/]").AddChoices(files).EnableSearch())

    let! result =
      Command.createBook
        dataContext
        title
        (stringOptionIfEmpty author)
        (stringOptionIfEmpty mainTopic)
        (stringOptionIfValue noFilepath filepath)
        DateTime.UtcNow

    match result with
    | Ok _ -> sprintf "[green] Book was saved successfully![/]" |> AnsiConsole.MarkupLine
    | Error es ->
      let boldRed s = sprintf "[bold red] - %s [/]" s

      AnsiConsole.MarkupLine(boldRed "Some errors ocurred")

      let errors =
        es
        |> List.map CommonTypes.appErrorToString
        |> List.map boldRed
        |> fun es -> String.Join('\n', es)

      AnsiConsole.MarkupLine errors
  }

let getBooks (dataContext: Context.ReadDataContext) : Async<unit> =
  async {
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
  }

let logReading (readDataContext: Context.ReadDataContext) (dataContext: Context.DataContext) : Async<unit> =
  async {
    let books =
      query {
        for book in readDataContext |> Query.getBooks do
          select (book.Id, book.Title)
      }
      |> Seq.toList

    let bookId =
      AnsiConsole.Prompt(
        SelectionPrompt<int64 * string>()
          .Title("[bold]What book?[/]")
          .UseConverter(snd)
          .AddChoices(books)
          .EnableSearch()
      )
      |> fst

    let initialPage = AnsiConsole.Ask<int> "[bold]Initial page[/]?"
    let finalPage = AnsiConsole.Ask<int> "[bold]Final page[/]?"

    let nextTopic =
      AnsiConsole.Prompt(TextPrompt<string>("[bold]Next topic[/]?").AllowEmpty())

    let! result =
      Command.logReading dataContext bookId initialPage finalPage (stringOptionIfEmpty nextTopic) DateTime.UtcNow

    match result with
    | Ok _ -> sprintf "[green] Reading log was saved![/]" |> AnsiConsole.MarkupLine
    | Error es ->
      let boldRed s = sprintf "[bold red] - %s [/]" s

      AnsiConsole.MarkupLine(boldRed "Some errors ocurred")

      let errors =
        es
        |> List.map CommonTypes.appErrorToString
        |> List.map boldRed
        |> fun es -> String.Join('\n', es)

      AnsiConsole.MarkupLine errors
  }

let getLastReadingLogsByBook (readDataContext: Context.ReadDataContext) : Async<unit> =
  async {
    let books =
      query {
        for book in readDataContext |> Query.getBooks do
          select (book.Id, book.Title)
      }
      |> Seq.toList

    let bookId =
      AnsiConsole.Prompt(
        SelectionPrompt<int64 * string>()
          .Title("[bold]What book?[/]")
          .UseConverter(snd)
          .AddChoices(books)
          .EnableSearch()
      )
      |> fst

    let readingLogs =
      query {
        for readingLog in readDataContext |> Query.getReadingLogs do
          where (readingLog.IdBook = bookId)
          sortByDescending readingLog.Modified
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
        b.Modified
      |]

      values |> table.AddRow |> ignore)

    AnsiConsole.Write table
  }
