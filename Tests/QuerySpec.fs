module Tests.QuerySpec

open System

open Expecto
open Expecto.Flip.Expect

open Donald

open App

let ``it gets books`` =
  "it get books",
  fun (conn: Context.BooktrackerConnection) ->
    let! newBook = Utils.createRandomBook conn
    let result = Query.getBooks conn
    result |> hasLength "wrong result length" 1
    let book0 = result.Head
    book0.Title |> equal "wrong title" newBook.Title

let ``it gets the last reading log`` =
  "it gets the last reading log",
  fun (conn: Context.BooktrackerConnection) ->
    // arrange
    let tran = conn.TryBeginTransaction()
    let! book = Utils.createRandomBook conn
    let now = DateTime.UtcNow

    let createLogReading =
      Command.logReading tran book.Id (Utils.randomInt 1 100) (Utils.randomInt 1 100) None

    let! _ = createLogReading (now.AddDays -2)
    let! _ = createLogReading (now.AddDays -1)
    // This is the reading log that should be returned
    let! _ = createLogReading now

    tran.TryCommit()

    // act
    let result = Query.getLastReadingLogByBook conn None

    // assert
    result
    |> wantSome "result should be some"
    |> fun readingLog ->
        (readingLog.IdBook, readingLog.Read)
        |> equal "reading log is incorrect" (book.Id, now)

let ``it returns None when no last reading log exists`` =
  "it returns None when no last reading log exists",
  fun (conn: Context.BooktrackerConnection) ->
    let result = Query.getLastReadingLogByBook conn None
    result |> isNone "result should be None"

let ``it returns hook command filled with book data`` =
  "it returns hook command filled with book data",
  fun (conn: Context.BooktrackerConnection) ->
    // arrange
    let tran = conn.TryBeginTransaction()
    let! book = Utils.createRandomBook conn

    let! readingLogIdResult =
      Command.logReading
        tran
        book.Id
        (Utils.randomInt 1 100)
        (Utils.randomInt 1 100)
        (Utils.random5String () |> Some)
        DateTime.UtcNow

    let readingLogId =
      match readingLogIdResult with
      | Ok v -> v
      | Error _ -> failtest "readingLog should be ok"

    let readingLog =
      conn
      |> Db.newCommand "select * from reading_log"
      |> Db.querySingle Query.readingLogfromDataReader
      |> Option.get

    let hook =
      tran
      |> Db.newCommandForTransaction
        "
        insert into hook (name, command) values (@name, @command);

        select * from hook
        where hook.id = last_insert_rowid();
        "
      |> Db.setParams [
        "name", Utils.random5String () |> sqlString
        "command",
        sqlString
          "sioyek {{filepath}} --initial-page {{initial-page}} --final-page {{final-page}} --next-topic {{next-topic}}"
      ]
      |> Db.querySingle Query.hookFromDataReader
      |> Option.get

    tran.TryCommit()

    // act
    let result = Query.getHookCommandByReadingLog conn hook.Id readingLogId

    // assert
    result
    |> wantOk "result should be ok"
    |> equal
      "hook command is incorrect"
      ("sioyek",
       sprintf
         "%s --initial-page %d --final-page %d --next-topic %s"
         book.Filepath.Value
         readingLog.InitialPage
         readingLog.FinalPage
         readingLog.NextTopic.Value)

[<Tests>]
let querySpec =
  testList "query" [
    yield!
      testFixture Utils.memoryDbFixture [
        ``it gets books``
        ``it gets the last reading log``
        ``it returns None when no last reading log exists``
        ``it returns hook command filled with book data``
      ]
  ]
