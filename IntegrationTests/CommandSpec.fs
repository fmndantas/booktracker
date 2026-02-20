module IntegrationTests.CommandSpec

open System

open Expecto
open Expecto.Flip.Expect

open Donald

open App

module Sut = Command

let ``it creates a book`` =
  "it creates a book",
  fun conn ->
    // arrange
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
  "it updates a book",
  fun conn ->
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

let ``it deletes a book`` =
  "it deletes a book",
  fun conn ->
    let createdBook = Utils.createRandomBook conn

    for _ in [ 1..10 ] do
      Command.logReading
        conn
        createdBook.Id
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> Some)
        DateTime.UtcNow
      |> ignore

    let count =
      fun conn ->
        conn
        |> Db.newCommand "select count(*) as cnt from reading_log where id_book = @id_book"
        |> Db.setParams [ "id_book", sqlInt64 createdBook.Id ]
        |> Db.querySingle (fun rd -> rd.ReadInt64 "cnt")
        |> Option.get

    count conn |> equal "wrong before count" 10

    let _ = Command.deleteBook conn createdBook.Id

    count conn |> equal "wrong after count" 0

let ``it logs reading for a book`` =
  "it logs reading for a book",
  fun conn ->
    // arrange
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
  "it returns error if a log is created with a book that does not exists",
  fun conn ->
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
    yield!
      testFixture Utils.testFixture [
        ``it creates a book``
        ``it updates a book``
        ``it deletes a book``
        ``it returns error if a log is created with a book that does not exists``
        ``it logs reading for a book``
      ]
  ]
