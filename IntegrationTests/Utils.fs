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

let cleanDatabase () : Async<unit> =
  async {
    let context = Context.getWriteContext testDbConnectionString
    context.Main.ReadingLog |> Seq.iter _.Delete()
    context.Main.Book |> Seq.iter _.Delete()
    return! context.SubmitUpdatesAsync() |> Async.AwaitTask
  }

let createRandomBook () =
  async {
    let context = getWriteDataContext ()

    let now = DateTime.UtcNow.ToSqlite

    let newBook =
      (now, random5String ()) |> context.Main.Book.``Create(modified, title)``

    do! context.SubmitUpdatesAsync() |> Async.AwaitTask
    return newBook
  }

let createRandomBookEntity () : Context.Book =
  let context = getWriteDataContext ()
  let book = context.Main.Book.Create()
  book.Title <- random5String ()
  book.Author <- random5String () |> ValueSome
  book.MainTopic <- random5String () |> ValueSome
  book.Filepath <- random5String () |> ValueSome
  book.Modified <- DateTime.UtcNow.ToSqlite
  book

let createRandomReadingLogEntity () : Context.ReadingLog =
  let context = getWriteDataContext ()
  let readingLog = context.Main.ReadingLog.Create()
  readingLog.IdBook <- randomInt 1 1000
  readingLog.InitialPage <- randomInt 1 100
  readingLog.FinalPage <- randomInt 1 100
  readingLog.NextTopic <- random5String () |> ValueSome
  readingLog.Modified <- DateTime.UtcNow.ToSqlite
  readingLog
