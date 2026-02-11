// For more information see https://aka.ms/fsharp-console-apps
open Argu

open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

// FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent
// |> Event.add (printfn "Executing SQL: %O")

let w = Context.getWriteContext connectionString
let r = Context.getReadContext connectionString

let parser = ArgumentParser.Create<Parser.Arguments>(programName = "booktracker")

[<EntryPoint>]
let main argv =
  // let result = parser.ParseCommandLine argv
  //
  // if result.Contains Parser.Arguments.Get_Books then
  //   r |> Workflow.getBooks |> Async.RunSynchronously
  //
  // if result.Contains Parser.Arguments.Get_Logs_By_Book then
  //   r |> Workflow.getLastReadingLogsByBook |> Async.RunSynchronously
  //
  // if result.Contains Parser.Arguments.Create_Book then
  //   (w, bookFolder) ||> Workflow.createBook |> Async.RunSynchronously
  //
  // if result.Contains Parser.Arguments.Log_Reading then
  //   (r, w) ||> Workflow.logReading |> Async.RunSynchronously

  Workflow.continueLastReading r |> Async.RunSynchronously

  0
