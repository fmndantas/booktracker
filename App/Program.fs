// For more information see https://aka.ms/fsharp-console-apps
open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

// FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent
// |> Event.add (printfn "Executing SQL: %O")

let wdc = Context.getWriteContext connectionString
let rdc = Context.getReadContext connectionString

let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")

[<EntryPoint>]
let main argv =
  let result = parser.ParseCommandLine argv

  if result.Contains Parser.Arguments.Get_Books then
    rdc |> Workflow.getBooks |> Async.RunSynchronously

  if result.Contains Parser.Arguments.Get_Logs_By_Book then
    rdc
    |> Workflow.getLastReadingLogsByBook
    |> Async.RunSynchronously

  if result.Contains Parser.Arguments.Create_Book then
    (wdc,bookFolder)
    ||> Workflow.createBook
    |> Async.RunSynchronously

  if result.Contains Parser.Arguments.Log_Reading then
    (rdc, wdc)
    ||> Workflow.logReading
    |> Async.RunSynchronously

  0
