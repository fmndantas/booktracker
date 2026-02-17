module IntegrationTests.CommandSpec

open System

open Expecto
open Expecto.Flip.Expect

open App
open App.SqliteExtensions

module Sut = Command

let ``it creates a book`` =
  testCase "it creates a book"
  <| fun () ->
    // arrange
    let w, r = Utils.getTestDataContexts ()
    do Utils.cleanDatabase w

    let title, author, mainTopic, filepath, now =
      Utils.random5String (), Utils.random5String (), Utils.random5String (), Utils.random5String (), DateTime.UtcNow

    // act
    let result =
      Sut.createBook w title (ValueSome author) (ValueSome mainTopic) (ValueSome filepath) now

    // assert
    let savedBooks = Query.getBooks r |> Seq.toList

    savedBooks |> hasLength "no book was saved" 1

    result
    |> wantOk "result is not ok"
    |> fun savedBookId ->
        let head = savedBooks.Head
        let actual = head.Id, head.Title, head.Author, head.Filepath, head.Modified

        let expected =
          savedBookId, title, ValueSome author, ValueSome filepath, now.ToSqlite

        actual |> equal "wrong book" expected

let ``it updates a book`` =
  testCase "it updates a book"
  <| fun () ->
    let w, r = Utils.getTestDataContexts ()
    Utils.cleanDatabase w
    let createdBook = Utils.createRandomBook w

    let title, author, mainTopic, filepath, now =
      Utils.random5String (), Utils.random5String (), Utils.random5String (), Utils.random5String (), DateTime.UtcNow

    let result =
      Sut.updateBook w createdBook.Id title (ValueSome author) (ValueSome mainTopic) (ValueSome filepath) now

    let savedBooks = Query.getBooks r |> Seq.toList
    savedBooks |> hasLength "number of saved books should be 1" 1

    result
    |> wantOk "result is not ok"
    |> fun savedBookId ->
        let head = savedBooks.Head
        let actual = head.Id, head.Title, head.Author, head.Filepath, head.Modified

        let expected =
          savedBookId, title, ValueSome author, ValueSome filepath, now.ToSqlite

        actual |> equal "wrong book" expected

let ``it logs reading for a book`` =
  testCase "it logs reading for a book"
  <| fun () ->
    // arrange
    let w, r = Utils.getTestDataContexts ()
    Utils.cleanDatabase w
    let newBook = Utils.createRandomBook w

    let now = DateTime.UtcNow

    // act
    let! result =
      Sut.logReading
        w
        newBook.Id
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> ValueSome)
        now

    // assert
    let readingLogs = Query.getReadingLogs r |> Seq.toList
    readingLogs |> hasLength "no reading log was saved" 1

    result
    |> wantOk "result should be ok"
    |> fun savedReadingLog ->
        let expected = readingLogs.Head
        savedReadingLog |> equal "objects are different" expected.Id

let ``it returns error if a log is created with a book that does not exists`` =
  testCase "it returns error if a log is created with a book that does not exists"
  <| fun () ->
    let w, _ = Utils.getTestDataContexts ()
    Utils.cleanDatabase w

    let! result =
      Sut.logReading
        w
        1000L
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> ValueSome)
        DateTime.UtcNow

    result
    |> wantError "result should be an error"
    |> contains "does not have expected error" (CommonTypes.AppError.BusinessError "Log points to inexistent book")

[<Tests>]
let commandSpec =
  testList "command" [
    ``it creates a book``
    ``it updates a book``
    ``it returns error if a log is created with a book that does not exists``
    ``it logs reading for a book``
  ]
