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
    do! Utils.cleanDatabase ()
    let _, r = Utils.getTestDataContexts ()
    let! newBook = Utils.createRandomBook ()

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
    do! Utils.cleanDatabase ()
    let! book = Utils.createRandomBook ()
    let w, r = Utils.getTestDataContexts ()
    let now = DateTime.UtcNow

    let createBook =
      Command.logReading w book.Id (Utils.randomInt 1 100) (Utils.randomInt 1 100) ValueNone

    let! _ = createBook (now.AddDays -2)
    let! _ = createBook (now.AddDays -1)
    // This is the reading log that should be returned
    let! _ = createBook now

    // act
    let result = Query.getLastReadingLog r

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
    do! Utils.cleanDatabase ()
    let _, r = Utils.getTestDataContexts ()
    let result = Query.getLastReadingLog r
    result |> isNone "result should be None"
  }

[<Tests>]
let querySpec =
  testList "query" [
    ``it gets books``
    ``it gets the last reading log``
    ``it returns None when no last reading log exists``
  ]
