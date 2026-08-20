// For more information see https://aka.ms/fsharp-console-apps
open System
open System.IO

open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"

let sqliteFilepath = Path.Join(__SOURCE_DIRECTORY__, "..", "test.db")

let migrationsFolder = Path.Join(__SOURCE_DIRECTORY__, "..", "migrations")

let conn = Context.getBooktrackerConnection sqliteFilepath

let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")

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

[<EntryPoint>]
let main argv =
  try
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
