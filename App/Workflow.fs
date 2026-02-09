module App.Workflow

open System

open Spectre.Console

module W = WriteDomain

let stringOption (evaluateAsNone: string -> bool) (s: string) : string option =
  if evaluateAsNone s then None else Some s

let stringOptionIfEmpty = stringOption (fun s -> s.Length = 0)

let stringOptionIfValue v = stringOption (fun s -> s = v)

let createBook (connectionString: string) (bookFolder: string) : Async<unit> =
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

    let newBook =
      W.createBook
        title
        (stringOptionIfEmpty author)
        (stringOptionIfEmpty mainTopic)
        (stringOptionIfValue noFilepath filepath)
        DateTime.UtcNow

    let! result = Command.createBook connectionString newBook

    match result with
    | Ok _ -> sprintf "[green] Book was saved successfully![/]" |> AnsiConsole.MarkupLine
    | Error es ->
      let boldRed s = sprintf "[bold red] %s [/]" s

      AnsiConsole.MarkupLine(boldRed "Some errors ocurred")

      let errors =
        es
        |> List.map CommonTypes.appErrorToString
        |> List.map boldRed
        |> fun es -> String.Join('\n', es)

      AnsiConsole.MarkupLine errors
  }

let getBooks (connectionString: string) : Async<unit> =
  async {
    let books = Query.getBooks connectionString

    let table = Table().AddColumns("Title", "Author", "Main topic", "Filepath")

    books
    |> List.iter (fun b ->
      let values = [|
        b.Title
        b.Author |> Option.defaultValue "_"
        b.MainTopic |> Option.defaultValue "-"
        b.Filepath |> Option.defaultValue "-"
      |]

      values |> table.AddRow |> ignore)

    AnsiConsole.Write table
  }
