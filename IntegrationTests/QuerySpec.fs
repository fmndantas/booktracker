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
    do! Utils.cleanDatabase Utils.TestDbConnectionString
    let! newBook = Utils.createRandomBook Utils.TestDbConnectionString

    // act
    let result = Sut.getBooks Utils.TestDbConnectionString

    // assert
    result |> hasLength "wrong result length" 1
    let book0 = result[0]
    book0.Title |> equal "wrong result" newBook.Title
  }

[<Tests>]
let querySpec = testList "query" [ ``it get books`` ]
