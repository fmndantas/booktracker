module IntegrationTests.QuerySpec

open App.ReadDomain

open Expecto
open Expecto.Flip.Expect

module ctx = App.Context
module sut = App.Query

let testDbConnectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../dummy.db"

let ``it get books`` =
  testCaseAsync "it get books"
  <| async {
    // arrange
    do! Utils.cleanDatabase testDbConnectionString
    let! newBook = Utils.createRandomBook testDbConnectionString

    // act
    let result = sut.getBooks testDbConnectionString

    // assert
    result |> hasLength "wrong result length" 1
    let book0 = result[0]
    book0.Title |> equal "wrong result" newBook.Title
  }

[<Tests>]
let querySpec = testList "query" [ ``it get books`` ]
