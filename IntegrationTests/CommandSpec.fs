module IntegrationTests.CommandSpec

open System

open Expecto
open Expecto.Flip.Expect

open Donald

open App

module Sut = Command

let ``it creates a book`` =
  "it creates a book",
  fun (conn: Context.BooktrackerConnection) ->
    // arrange
    let tran = conn.TryBeginTransaction()

    let title, author, mainTopic, filepath, now =
      Utils.random5String (), Utils.random5String (), Utils.random5String (), Utils.random5String (), DateTime.UtcNow

    // act
    let result =
      Sut.createBook tran title (Some author) (Some mainTopic) (Some filepath) now

    tran.TryCommit()

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
  fun (conn: Context.BooktrackerConnection) ->
    let tran = conn.TryBeginTransaction()
    let createdBook = Utils.createRandomBook conn

    let title, author, mainTopic, filepath, now =
      Utils.random5String (), Utils.random5String (), Utils.random5String (), Utils.random5String (), DateTime.UtcNow

    let result =
      Sut.updateBook tran createdBook.Id title (Some author) (Some mainTopic) (Some filepath) now

    tran.TryCommit()

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
  fun (conn: Context.BooktrackerConnection) ->
    let tran1 = conn.TryBeginTransaction()
    let createdBook = Utils.createRandomBook conn

    for _ in [ 1..10 ] do
      Command.logReading
        tran1
        createdBook.Id
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> Some)
        DateTime.UtcNow
      |> ignore

    tran1.TryCommit()

    let count =
      fun conn ->
        conn
        |> Db.newCommand "select count(*) as cnt from reading_log where id_book = @id_book"
        |> Db.setParams [ "id_book", sqlInt64 createdBook.Id ]
        |> Db.querySingle (fun rd -> rd.ReadInt64 "cnt")
        |> Option.get

    count conn |> equal "wrong before count" 10

    let tran2 = conn.TryBeginTransaction()
    let _ = Command.deleteBook tran2 createdBook.Id
    tran2.TryCommit()

    count conn |> equal "wrong after count" 0

let ``it logs reading for a book`` =
  "it logs reading for a book",
  fun (conn: Context.BooktrackerConnection) ->
    // arrange
    let tran = conn.TryBeginTransaction()
    let newBook = Utils.createRandomBook conn

    let now = DateTime.UtcNow

    // act
    let! result =
      Sut.logReading
        tran
        newBook.Id
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> Some)
        now

    tran.TryCommit()

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
  fun (conn: Context.BooktrackerConnection) ->
    let tran = conn.TryBeginTransaction()

    let! result =
      Sut.logReading
        tran
        1000L
        (Utils.randomInt1_10 ())
        (Utils.randomInt1_10 ())
        (Utils.random5String () |> Some)
        DateTime.UtcNow

    tran.TryCommit()

    result
    |> wantError "result should be an error"
    |> contains
      "does not have expected error"
      (CommonTypes.AppError.BusinessError $"Book with id {1000} does not exists")

let ``it creates a hook`` =
  "it creates a hook",
  fun (conn: Context.BooktrackerConnection) ->
    let tran = conn.TryBeginTransaction()
    let! result = Sut.createHook tran (Utils.random5String ()) (Utils.random5String ())
    let uniqueHookId = conn |> Query.getHooks |> List.head |> _.Id

    result
    |> wantOk "result should be ok"
    |> equal "hook id is incorrect" uniqueHookId

let ``it updates a hook`` =
  "it updates a hook",
  fun (conn: Context.BooktrackerConnection) ->
    let tran1 = conn.TryBeginTransaction()
    Sut.createHook tran1 (Utils.random5String ()) (Utils.random5String ()) |> ignore
    tran1.TryCommit()

    let uniqueHookId = conn |> Query.getHooks |> List.head |> _.Id

    let updatedName, updatedCommand = Utils.random5String (), Utils.random5String ()

    let tran2 = conn.TryBeginTransaction()
    let! result = Sut.updateHook tran2 uniqueHookId updatedName updatedCommand
    tran2.Commit()

    result
    |> wantOk "result should be ok"
    |> fun hookId ->
        let (Ok updatedHook) = Query.getHookById conn hookId

        updatedHook
        |> equal "updated hook is wrong" {
          Id = hookId
          Name = updatedName
          Command = updatedCommand
        }

[<Tests>]
let commandSpec =
  testList "command" [
    yield!
      testFixture Utils.memoryDbFixture [
        ``it creates a book``
        ``it updates a book``
        ``it deletes a book``
        ``it returns error if a log is created with a book that does not exists``
        ``it logs reading for a book``
        ``it creates a hook``
        ``it updates a hook``
      ]
  ]
