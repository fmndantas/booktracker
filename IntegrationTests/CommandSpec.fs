module IntegrationTests.CommandSpec

open Expecto
open Expecto.Flip.Expect

// TODO: dedup with QuerySpec.fs
let testDbConnectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../dummy.db"

module ctx = App.Context
module sut = App.Command

let ``it create a book`` =
  testCaseAsync "it create a book"
  <| async {
    // arrange
    do! Utils.cleanDatabase testDbConnectionString

    // act
    let! result = sut.createBook testDbConnectionString { Title = Utils.random5String () }

    // App.Query.getBooks testDbConnectionString
    let savedBooks = App.Query.getBooks testDbConnectionString
    savedBooks |> hasLength "no book was saved" 1
    let book0Id = savedBooks.Head.Id
    result |> equal "wrong result" book0Id
  }

[<Tests>]
let commandSpec = testList "command" [ ``it create a book`` ]
