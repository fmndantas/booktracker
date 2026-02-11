module IntegrationTests.QuerySpec

open System

open Expecto
open Expecto.Flip.Expect

open App
open App.SqliteExtensions

let ``it gets books`` =
  testCaseAsync "it get books"
  <| async {
    // arrange
    let w, r = Utils.getTestDataContexts ()
    do! Utils.cleanDatabase w
    let! newBook = Utils.createRandomBook w

    // act
    let result = Query.getBooks r |> Seq.toList

    // assert
    result |> hasLength "wrong result length" 1
    let book0 = result[0]
    book0.Title |> equal "wrong result" newBook.Title
  }

let ``it gets the last reading log`` =
  testCaseAsync "it gets the last reading log"
  <| async {
    // arrange
    let w, r = Utils.getTestDataContexts ()
    do! Utils.cleanDatabase w
    let! book = Utils.createRandomBook w
    let now = DateTime.UtcNow

    let createLogReading =
      Command.logReading w book.Id (Utils.randomInt 1 100) (Utils.randomInt 1 100) ValueNone

    let! _ = createLogReading (now.AddDays -2)
    let! _ = createLogReading (now.AddDays -1)
    // This is the reading log that should be returned
    let! _ = createLogReading now

    // act
    let result = Query.getLastReadingLogByBook r None

    // assert
    result
    |> wantSome "result should be some"
    |> fun lastReadingLog ->
        (lastReadingLog.IdBook, lastReadingLog.Read.FromSqlite)
        |> equal "reading log is incorrect" (book.Id, now)
  }

let ``it returns None when no last reading log exists`` =
  testCaseAsync "it returns None when no last reading log exists"
  <| async {
    let w, r = Utils.getTestDataContexts ()
    do! Utils.cleanDatabase w
    let result = Query.getLastReadingLogByBook r None
    result |> isNone "result should be None"
  }

let ``it returns hook command filled with book data`` =
  testCaseAsync "it returns hook command filled with book data"
  <| async {
    // arrange
    let w, r = Utils.getTestDataContexts ()
    do! Utils.cleanDatabase w

    let! book = Utils.createRandomBook w

    let! readingLogIdResult =
      Command.logReading
        w
        book.Id
        (Utils.randomInt 1 100)
        (Utils.randomInt 1 100)
        (Utils.random5String () |> ValueSome)
        DateTime.UtcNow

    let readingLogId =
      match readingLogIdResult with
      | Ok v -> v
      | Error _ -> failtest "readingLog should be ok"

    let readingLog =
      query {
        for log in r.Main.ReadingLog do
          head
      }

    let hook =
      w.Main.Hook.``Create(command, name)`` (
        "sioyek {{filepath}} --initial-page {{initial_page}} --final-page {{final_page}} --next-topic {{next_topic}}",
        Utils.random5String ()
      )

    do! w.SubmitUpdatesAsync() |> Async.AwaitTask

    // act
    let result = Query.getHookCommandByReadingLog r hook.Id readingLogId

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
  }

[<Tests>]
let querySpec =
  testList "query" [
    ``it gets books``
    ``it gets the last reading log``
    ``it returns None when no last reading log exists``
    ``it returns hook command filled with book data``
  ]
