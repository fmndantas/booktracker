module IntegrationTests.CommandSpec

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

let ``it returns error if a log is created with a book that does not exists`` =
  testCaseAsync "it returns error if a log is created with a book that does not exists"
  <| async {
    do! Utils.cleanDatabase ()
    let w, _ = Utils.getTestDataContexts ()
    let newReadingLog: Context.ReadingLog = Utils.createRandomReadingLogEntity ()

    let! result =
      Sut.logReading
        w
        1000L
        (int newReadingLog.InitialPage)
        (int newReadingLog.FinalPage)
        newReadingLog.NextTopic
        newReadingLog.Modified.FromSqlite

    result
    |> wantError "result should be an error"
    |> contains "does not have expected error" (CommonTypes.AppError.BusinessError "Log points to inexistent book")
  }

[<Tests>]
let commandSpec =
  testList "command" [
    ``it creates a book``
    ``it returns error if a log is created with a book that does not exists``
  ]
