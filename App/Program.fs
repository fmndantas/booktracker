// For more information see https://aka.ms/fsharp-console-apps
open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

FSharp.Data.Sql.Common.QueryEvents.SqlQueryEvent
|> Event.add (printfn "Executing SQL: %O")

let writableDataContext = Context.getWriteContext connectionString
let readonlyDataContext = Context.getReadContext connectionString

let createBook () =
  Workflow.createBook writableDataContext bookFolder

let getBooks () = Workflow.getBooks readonlyDataContext

let logReading () =
  Workflow.logReading readonlyDataContext writableDataContext

let getReadingLogsByBook () =
  Workflow.getLastReadingLogsByBook readonlyDataContext
