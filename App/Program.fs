// For more information see https://aka.ms/fsharp-console-apps
open App

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

let writableDataContext = Context.getWriteContext connectionString
let readonlyDataContext = Context.getReadContext connectionString

let createBook () =
  Workflow.createBook writableDataContext bookFolder

let getBooks () = Workflow.getBooks readonlyDataContext

createBook () |> Async.RunSynchronously
getBooks () |> Async.RunSynchronously
