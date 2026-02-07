// For more information see https://aka.ms/fsharp-console-apps
open App

module W = WriteDomain

// TODO: parametrize
let bookFolder = "/home/fernando/books"
let connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../booktracker.db"

let createBook () =
  Workflow.createBook connectionString bookFolder |> Async.RunSynchronously

createBook () |> ignore
