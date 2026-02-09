module IntegrationTests.QuerySpec

open App

open Expecto
open Expecto.Flip.Expect

let ``it get books`` =
  testCaseAsync "it get books"
  <| async {
    // arrange
    do! Utils.cleanDatabase ()
    let _, r = Utils.getTestDataContexts ()
    let! newBook = Utils.createRandomBook ()

    // act
    let result = Query.getBooks r |> Seq.toList

    // assert
    result |> hasLength "wrong result length" 1
    let book0 = result[0]
    book0.Title |> equal "wrong result" newBook.Title
  }

[<Tests>]
let querySpec = testList "query" [ ``it get books`` ]
