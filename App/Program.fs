// For more information see https://aka.ms/fsharp-console-apps
open System

open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

let w = Context.getWriteContext connectionString
let r = Context.getReadContext connectionString

let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")

let mutable isDebug = false

let debug (message: string) =
  if isDebug then
    printfn "[DEBUG - %s]: %s" (DateTime.UtcNow.ToString "O") message

FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent
|> Event.add (fun e -> debug (sprintf "Executing SQL: %O" e))

[<EntryPoint>]
let main argv =
  try
    let result = parser.ParseCommandLine argv

    isDebug <- result.Contains Parser.Debug

    let timer = Diagnostics.Stopwatch.StartNew()

    if result.Contains Parser.Arguments.Get_Books then
      r |> Workflow.getBooks

    if result.Contains Parser.Arguments.Get_Logs_By_Book then
      r |> Workflow.getLastReadingLogsByBook

    if result.Contains Parser.Arguments.Create_Book then
      (w, bookFolder) ||> Workflow.createBook

    if result.Contains Parser.Arguments.Log_Reading then
      (r, w) ||> Workflow.logReading

    if result.Contains Parser.Arguments.Continue_Last_Reading then
      r |> Workflow.continueLastReading

    timer.Stop()

    debug (sprintf "workflow ran in %d ms" timer.ElapsedMilliseconds)
    0
  with :? ArguParseException as e ->
    printf "%s" e.Message
    1
