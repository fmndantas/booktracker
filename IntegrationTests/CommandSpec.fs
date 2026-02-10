module IntegrationTests.CommandSpec

open System

open Expecto
open Expecto.Flip.Expect

open App
open App.SqliteExtensions

module Sut = Command

let ``it creates a book`` =
  testCaseAsync "it create a book"
  <| async {
    // arrange
    do! Utils.cleanDatabase ()
    let w, r = Utils.getTestDataContexts ()
    let newBook = Utils.createRandomBookEntity ()

    // act
    let! result =
      Sut.createBook w newBook.Title newBook.Author newBook.MainTopic newBook.Filepath newBook.Modified.FromSqlite

    // assert
    let savedBooks = Query.getBooks r |> Seq.toList

    savedBooks |> hasLength "no book was saved" 1

    result
    |> wantOk "result is not ok"
    |> fun savedBookId ->
        let head = savedBooks.Head
        newBook.Id <- savedBookId
        let actual = head.Id, head.Title, head.Author, head.Filepath, head.Modified
        let expected = newBook.Id, head.Title, head.Author, head.Filepath, head.Modified
        actual |> equal "wrong book" expected
  }

let ``it logs reading for a book`` =
  testCaseAsync "it logs reading for a book"
  <| async {
    // arrange
    do! Utils.cleanDatabase ()
    let w, r = Utils.getTestDataContexts ()
    let newBook = Utils.createRandomBookEntity ()

    let! bookResult =
      Sut.createBook w newBook.Title newBook.Author newBook.MainTopic newBook.Filepath newBook.Modified.FromSqlite

    let bookId =
      match bookResult with
      | Ok v -> v
      | Error _ -> failtest "book could not be saved"

    let newReadingLog = Utils.createRandomReadingLogEntity ()
    newReadingLog.IdBook <- bookId

    let now = DateTime.UtcNow

    // act
    let! result =
      Sut.logReading
        w
        newReadingLog.IdBook
        (int newReadingLog.InitialPage)
        (int newReadingLog.FinalPage)
        newReadingLog.NextTopic
        now

    // assert
    let readingLogs = Query.getReadingLogs r |> Seq.toList
    readingLogs |> hasLength "no book was saved" 1

    result
    |> wantOk "result should be ok"
    |> fun savedReadingLog ->
        let expected = readingLogs.Head.ColumnValues |> List.ofSeq |> List.sortBy fst
        newReadingLog.Id <- savedReadingLog
        newReadingLog.Read <- now.ToSqlite
        newReadingLog.Modified <- now.ToSqlite
        let actual = newReadingLog.ColumnValues |> List.ofSeq |> List.sortBy fst
        actual |> equal "objects are different" expected
  }

let ``it returns error if a log is created with a book that does not exists`` =
  testCaseAsync "it returns error if a log is created with a book that does not exists"
  <| async {
    do! Utils.cleanDatabase ()
    let w, _ = Utils.getTestDataContexts ()
    let newReadingLog = Utils.createRandomReadingLogEntity ()

    let! result =
      Sut.logReading
        w
        1000L
        (int newReadingLog.InitialPage)
        (int newReadingLog.FinalPage)
        newReadingLog.NextTopic
        DateTime.UtcNow

    result
    |> wantError "result should be an error"
    |> contains "does not have expected error" (CommonTypes.AppError.BusinessError "Log points to inexistent book")
  }

[<Tests>]
let commandSpec =
  testList "command" [
    ``it creates a book``
    ``it returns error if a log is created with a book that does not exists``
    ``it logs reading for a book``
  ]
