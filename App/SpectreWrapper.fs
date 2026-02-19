module App.SpectreWrapper

open System

open Spectre.Console

open App.CommonTypes

let ask message (defaultValueOption: 'a option) =
  if defaultValueOption.IsSome then
    AnsiConsole.Ask(message, defaultValueOption.Value)
  else
    AnsiConsole.Ask message

let ask' message = ask message None

let boldRed s = sprintf "[bold red]%s[/]" s

let item s = sprintf "\u2022 %s" s

let showErrors (es: AppError list) =
  let errorItems =
    es
    |> List.map appErrorToString
    |> List.map (boldRed >> item)
    |> fun es -> String.Join('\n', es)

  AnsiConsole.MarkupLine(boldRed "Something went wrong:")
  AnsiConsole.MarkupLine errorItems

[<AutoOpen>]
module TextPromptBuilder =
  let aTextPrompt message = TextPrompt message
  let allowEmpty (v: TextPrompt<'T>) = v.AllowEmpty()

  /// Set default value if it is some. Otherwise, just return the text prompt
  let defaultValueOption (defaultValue: 'T option) (v: TextPrompt<'T>) =
    defaultValue |> Option.map v.DefaultValue |> Option.defaultValue v

[<AutoOpen>]
module SelectPromptBuilder =
  let aSelectionPrompt message = SelectionPrompt().Title message
  let aSelectionPrompt' () = SelectionPrompt()
  let addChoices choices (v: SelectionPrompt<'T>) = v.AddChoices choices
  let enableSearch (v: SelectionPrompt<'T>) = v.EnableSearch()

[<AutoOpen>]
module TableBuilder =
  let aTable () = Table()
  let addColumns (columns: string[]) (v: Table) = v.AddColumns columns

  let addRows (rows: string[][]) (v: Table) =
    rows |> Array.iter (fun values -> v.AddRow values |> ignore)
    v
