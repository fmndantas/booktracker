module IntegrationTests.QuerySpec

open App.ReadDomain

open Expecto
open Expecto.Flip.Expect

module Ctx = App.Context
module Sut = App.Query

let ``it get books`` =
  testCaseAsync "it get books"
  <| async {
    // arrange
    do! Utils.cleanDatabase Utils.testDbConnectionString
    let! newBook = Utils.createRandomBook Utils.testDbConnectionString

    // act
    let result = Sut.getBooks Utils.testDbConnectionString

    // assert
    result |> hasLength "wrong result length" 1
    let book0 = result[0]
    book0.Title |> equal "wrong result" newBook.Title
  }

[<Tests>]
let querySpec = testList "query" [ ``it get books`` ]
