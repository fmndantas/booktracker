// For more information see https://aka.ms/fsharp-console-apps
open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

// FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent
// |> Event.add (printfn "Executing SQL: %O")

let writableDataContext = Context.getWriteContext connectionString
let readonlyDataContext = Context.getReadContext connectionString

let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")

[<EntryPoint>]
let main argv =
  let result = parser.ParseCommandLine argv

  if result.Contains Parser.Arguments.Get_Books then
    readonlyDataContext |> Workflow.getBooks |> Async.RunSynchronously

  if result.Contains Parser.Arguments.Get_Logs_By_Book then
    readonlyDataContext
    |> Workflow.getLastReadingLogsByBook
    |> Async.RunSynchronously

  0
