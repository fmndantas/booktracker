// For more information see https://aka.ms/fsharp-console-apps
open System

open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"

[<Obsolete>]
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

let sqliteFilepath = __SOURCE_DIRECTORY__ + "/../booktracker.db"

let conn = Context.getBooktrackerConnection sqliteFilepath

let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")

let mutable isDebug = false

let printDebug (message: string) =
  if isDebug then
    printfn "[DEBUG - %s]: %s" (DateTime.UtcNow.ToString "O") message

// FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent
// |> Event.add (fun e -> printDebug (sprintf "Executing SQL: %O" e))

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

    externalMark.Start "workflow"

    if result.Contains Parser.Arguments.Get_Books then
      Workflow.getBooks conn innerMark

    if result.Contains Parser.Arguments.Get_Logs_By_Book then
      Workflow.getLastReadingLogsByBook conn innerMark

    if result.Contains Parser.Arguments.Create_Book then
      Workflow.createOrEditBook conn bookFolder innerMark

    if result.Contains Parser.Arguments.Log_Reading then
      Workflow.logReading conn innerMark

    if result.Contains Parser.Arguments.Continue_Last_Reading then
      Workflow.continueLastReading conn innerMark

    externalMark.End "workflow"

    0
  with :? ArguParseException as e ->
    printf "%s" e.Message
    1
