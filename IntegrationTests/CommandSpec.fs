module IntegrationTests.CommandSpec

open Expecto
open Expecto.Flip.Expect

module Sut = App.Command
module R = App.ReadDomain
module W = App.WriteDomain

let ``it create a book`` =
  testCaseAsync "it create a book"
  <| async {
    // arrange
    do! Utils.cleanDatabase Utils.TestDbConnectionString

    // act
    let bookToSave = { Title = Utils.random5String () }: W.Book
    let! result = Sut.createBook Utils.TestDbConnectionString bookToSave

    // assert
    let savedBooks = App.Query.getBooks Utils.TestDbConnectionString

    savedBooks |> hasLength "no book was saved" 1

    savedBooks.Head
    |> equal "wrong book" {
      Id = result |> W.getBookIdValue |> R.createBookId
      Title = bookToSave.Title
    }
  }

[<Tests>]
let commandSpec = testList "command" [ ``it create a book`` ]
