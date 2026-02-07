module App.Workflow

open System

open Spectre.Console

module W = WriteDomain

let stringOption (evaluateAsNone: string -> bool) (s: string) : string option =
  if evaluateAsNone s then None else Some s

let stringOptionIfEmpty = stringOption (fun s -> s.Length = 0)

let stringOptionIfValue v = stringOption (fun s -> s = v)

let createBook (connectionString: string) (bookFolder: string) : Async<Result<W.BookId, string list>> =
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

    return! Command.createBook connectionString newBook
  }
