module IntegrationTests.QuerySpec

open App.ReadDomain

open Expecto
open Expecto.Flip.Expect

module ctx = App.Context
module sut = App.Query

let testDbConnectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../dummy.db"

let cleanDatabase () : Async<unit> =
  async {
    let ctx = ctx.getWriteContext testDbConnectionString
    ctx.Main.ReadingLog |> Seq.iter _.Delete()
    ctx.Main.Book |> Seq.iter _.Delete()
    return! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
  }

let createRandomBook () =
  async {
    let ctx = ctx.getWriteContext testDbConnectionString

    let newBook =
      (Utils.random5String (), Utils.random5String (), Utils.random5String ())
      |> ctx.Main.Book.``Create(author, genre, title)``

    do! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
    return newBook
  }

let ``it get books`` =
  testCaseAsync "it get books"
  <| async {
    // arrange
    do! cleanDatabase ()
    let! newBook = createRandomBook ()

    // act
    let result = sut.getBooks testDbConnectionString

    // assert
    result |> hasLength "Expected non-empty list of books" 1
    let book0 = result[0]
    book0.Title |> equal "Result is different than expected" newBook.Title
  }

[<Tests>]
let querySpec = testList "query" [ ``it get books`` ]
