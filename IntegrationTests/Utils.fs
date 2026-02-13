module IntegrationTests.Utils

open System

open App
open App.SqliteExtensions

[<Literal>]
let private testDbConnectionString =
  "DataSource=" + __SOURCE_DIRECTORY__ + "/../ddl/dummy.db"

let private getWriteDataContext () =
  Context.getWriteContext testDbConnectionString

let private getReadDataContext () =
  Context.getReadContext testDbConnectionString

let getTestDataContexts () =
  getWriteDataContext (), getReadDataContext ()

let randomString (size: int) : string =
  let letters = [ 'a' .. 'z' ]
  let generateNextIdx () = letters |> List.length |> Random().Next

  [ 0 .. (size - 1) ]
  |> List.map (fun _ -> letters[generateNextIdx ()])
  |> String.Concat

let random5String () = randomString 5

let randomInt a b = Random().Next(a, b)

let randomInt1_10 () = randomInt 1 10

let cleanDatabase (context: Context.DataContext) : unit =
  context.Main.ReadingLog |> Seq.iter _.Delete()
  context.Main.Book |> Seq.iter _.Delete()
  context.Main.Hook |> Seq.iter _.Delete()
  context.SubmitUpdates()

let createRandomBook (context: Context.DataContext) =
  let book = context.Main.Book.Create()
  book.Title <- random5String ()
  book.Author <- random5String () |> ValueSome
  book.MainTopic <- random5String () |> ValueSome
  book.Filepath <- random5String () |> ValueSome
  book.Modified <- DateTime.UtcNow.ToSqlite
  context.SubmitUpdates()
  book
