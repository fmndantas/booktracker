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
  let result = parser.ParseCommandLine argv

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

  0
