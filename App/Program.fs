// For more information see https://aka.ms/fsharp-console-apps
open System

open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

let debug (message: string) =
  // printfn "[DEBUG - %s]: %s" (DateTime.UtcNow.ToString "O") message
  ()

FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent
|> Event.add (fun e -> debug (sprintf "Executing SQL: %O" e))

let timer = Diagnostics.Stopwatch.StartNew()
let w = Context.getWriteContext connectionString
let r = Context.getReadContext connectionString
timer.Stop()

debug (sprintf "context.t = %d" timer.ElapsedMilliseconds)

timer.Restart()
let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")
timer.Stop()

debug (sprintf "parser.t = %d" timer.ElapsedMilliseconds)

[<EntryPoint>]
let main argv =
  timer.Restart()
  let result = parser.ParseCommandLine argv
  timer.Stop()

  debug (sprintf "parser.ParseCommandLine = %d" timer.ElapsedMilliseconds)

  timer.Restart()

  if result.Contains Parser.Arguments.Get_Books then
    timer.Restart()
    r |> Workflow.getBooks
    timer.Stop()
    printfn "workflow.t = %d" timer.ElapsedMilliseconds

  if result.Contains Parser.Arguments.Get_Logs_By_Book then
    r |> Workflow.getLastReadingLogsByBook

  if result.Contains Parser.Arguments.Create_Book then
    (w, bookFolder) ||> Workflow.createBook

  if result.Contains Parser.Arguments.Log_Reading then
    (r, w) ||> Workflow.logReading

  if result.Contains Parser.Arguments.Continue_Last_Reading then
    r |> Workflow.continueLastReading

  timer.Stop()

  debug (sprintf "workflow = %d" timer.ElapsedMilliseconds)

  0
