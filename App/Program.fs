// For more information see https://aka.ms/fsharp-console-apps
open System
open System.IO

open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"

let sqliteFilepath =
  let applicationData =
    Environment.GetFolderPath Environment.SpecialFolder.LocalApplicationData

  let booktrackerFolder = Path.Join(applicationData, "booktracker")
  Directory.CreateDirectory booktrackerFolder |> ignore
  Path.Join(booktrackerFolder, "booktracker.db")

let migrationsFolder = Path.Join(__SOURCE_DIRECTORY__, "..", "migrations")

let conn = Context.getBooktrackerConnection sqliteFilepath

let mutable isDebug = false

let printDebug (message: string) =
  if isDebug then
    printfn "[DEBUG - %s]: %s" (DateTime.UtcNow.ToString "O") message

conn.Open()

conn.Trace
|> Event.add (fun e -> printDebug (sprintf "Executing SQL: %s" e.Statement))

let createMark () =
  let timer = Diagnostics.Stopwatch.StartNew()

  let startMark =
    fun message ->
      printDebug (sprintf "starting: [%s]" message)
      timer.Restart()

  let endMark =
    fun message -> printDebug (sprintf "ending: [%s]. elapsed: %d ms" message timer.ElapsedMilliseconds)

  ({ Start = startMark; End = endMark }: Workflow.Mark)

module CLI =
  let main argv =
    try
      let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")

      let result = parser.ParseCommandLine argv

      isDebug <- result.Contains Parser.Debug

      let externalMark = createMark ()
      let innerMark = createMark ()

      externalMark.Start "migration"

      Migrate.migrate conn printDebug migrationsFolder

      externalMark.End "migration"

      externalMark.Start "workflow"

      if result.Contains Parser.Arguments.Book_Crud then
        Workflow.bookCrud conn bookFolder innerMark

      if result.Contains Parser.Arguments.Hook_Crud then
        Workflow.hookCrud conn innerMark

      if result.Contains Parser.Arguments.Get_Logs_By_Book then
        Workflow.getLastReadingLogsByBook conn innerMark

      if result.Contains Parser.Arguments.Log_Reading then
        Workflow.logReading conn innerMark

      if result.Contains Parser.Arguments.Continue_Last_Reading then
        Workflow.continueLastReading conn innerMark

      externalMark.End "workflow"

      conn.Close()
      0
    with :? ArguParseException as e ->
      printf "%s" e.Message
      conn.Close()
      1

module TUI =
  open System.Collections.Generic
  open Terminal.Gui.App
  open Terminal.Gui.ViewBase
  open Terminal.Gui.Views

  let table (columns: string list) (rows: string list list) =
    let columnDefs =
      columns
      |> List.mapi (fun i c -> c, Func<string list, obj>(fun row -> box row.[i]))
      |> dict
      |> Dictionary<_, _>

    let tableView =
      new TableView(Width = Dim.Fill(), Height = Dim.Fill(), Table = EnumerableTableSource(rows, columnDefs))

    columns
    |> List.iteri (fun i _ ->
      let style = tableView.Style.GetOrCreateColumnStyle i
      style.MinWidth <- 16
      style.MaxWidth <- 32)

    tableView

  let main argv =
    use app = Application.Create()
    app.Init() |> ignore
    use window = new Window(Title = "Booktracker")

    let booksView =
      new FrameView(Title = "Books", Width = Dim.Fill(), Height = Dim.Fill())

    let booksTable =
      table [ "Title"; "Progress"; "Last topic" ] [
        [ "Dune"; "42%"; "Chapter 12" ]
        [ "Neuromancer"; "87%"; "Chateau Rouge" ]
        [ "Foundation"; "10%"; "Part I" ]
      ]

    booksView.Add booksTable |> ignore

    let hooksView =
      new FrameView(Title = "Hooks", Width = Dim.Fill(), Height = Dim.Fill())

    booksView.Visible <- true
    hooksView.Visible <- false

    window.Add(booksView, hooksView)

    let showBooksView () =
      booksView.Visible <- true
      hooksView.Visible <- false

    let showHooksView () =
      booksView.Visible <- false
      hooksView.Visible <- true

    let manageBooksItem =
      new MenuItem("_Manage books", "", Action(fun () -> showBooksView ()))

    let manageHooksItem =
      new MenuItem("_Manage hooks", "", Action(fun () -> showHooksView ()))

    let booksMenu = new MenuBarItem("_Books", [ manageBooksItem :> View ])
    let hooksMenu = new MenuBarItem("_Hooks", [ manageHooksItem :> View ])

    let menu = new MenuBar([ booksMenu; hooksMenu ])

    window.Add menu |> ignore

    app.Run window |> ignore

    0

[<EntryPoint>]
let main argv = TUI.main argv
