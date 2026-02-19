module IntegrationTests.CommandSpec

open System

open Expecto
open Expecto.Flip.Expect

open App

module Sut = Command

let ``it creates a book`` =
  testCase "it creates a book"
  <| fun () ->
    // arrange
    let conn = Utils.getTestBooktrackerConnection ()
    Utils.cleanDatabase conn

    let title, author, mainTopic, filepath, now =
      Utils.random5String (), Utils.random5String (), Utils.random5String (), Utils.random5String (), DateTime.UtcNow

    // act
    let result =
      Sut.createBook conn title (Some author) (Some mainTopic) (Some filepath) now

    // assert
    let savedBooks = Query.getBooks conn

    savedBooks |> hasLength "no book was saved" 1

    result
    |> wantOk "result is not ok"
    |> fun savedBookId ->
        let head = savedBooks.Head

        let actual = head.Id, head.Title, head.Author, head.Filepath, head.Modified

        let expected = savedBookId, title, Some author, Some filepath, now

        actual |> equal "wrong book" expected

let ``it updates a book`` =
  testCase "it updates a book"
  <| fun () ->
    let conn = Utils.getTestBooktrackerConnection ()
    Utils.cleanDatabase conn
    let createdBook = Utils.createRandomBook conn

    let title, author, mainTopic, filepath, now =
      Utils.random5String (), Utils.random5String (), Utils.random5String (), Utils.random5String (), DateTime.UtcNow

    let result =
      Sut.updateBook conn createdBook.Id title (Some author) (Some mainTopic) (Some filepath) now

    let savedBooks = Query.getBooks conn
    savedBooks |> hasLength "number of saved books should be 1" 1

    result
    |> wantOk "result is not ok"
    |> fun savedBookId ->
        let head = savedBooks.Head
        let actual = head.Id, head.Title, head.Author, head.Filepath, head.Modified
        let expected = savedBookId, title, Some author, Some filepath, now
        actual |> equal "wrong book" expected

let ``it logs reading for a book`` =
  testCase "it logs reading for a book"
  <| fun () ->
    // arrange
    let conn = Utils.getTestBooktrackerConnection ()
    Utils.cleanDatabase conn
    let newBook = Utils.createRandomBook conn

    let now = DateTime.UtcNow

    // act
    let! result =
      Sut.logReading
        conn
        newBook.Id
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> Some)
        now

    // assert
    let readingLogs = Query.getReadingLogs conn None
    readingLogs |> hasLength "no reading log was saved" 1

    result
    |> wantOk "result should be ok"
    |> fun savedReadingLog ->
        let expected = readingLogs.Head
        savedReadingLog |> equal "objects are different" expected.Id

let ``it returns error if a log is created with a book that does not exists`` =
  testCase "it returns error if a log is created with a book that does not exists"
  <| fun () ->
    let conn = Utils.getTestBooktrackerConnection ()
    Utils.cleanDatabase conn

    let! result =
      Sut.logReading
        conn
        1000L
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> Some)
        DateTime.UtcNow

    result
    |> wantError "result should be an error"
    |> contains
      "does not have expected error"
      (CommonTypes.AppError.BusinessError $"Book with id {1000} does not exists")

[<Tests>]
let commandSpec =
  testList "command" [
    ``it creates a book``
    ``it updates a book``
    ``it returns error if a log is created with a book that does not exists``
    ``it logs reading for a book``
  ]
