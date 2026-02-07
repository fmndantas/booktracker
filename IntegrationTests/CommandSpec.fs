module IntegrationTests.CommandSpec

open Expecto
open Expecto.Flip.Expect

module Sut = App.Command
module R = App.ReadDomain
module W = App.WriteDomain

let ``it creates a book`` =
  testCaseAsync "it create a book"
  <| async {
    // arrange
    do! Utils.cleanDatabase Utils.TestDbConnectionString

    // act
    let newBook =
      W.createBook
        (Utils.random5String ())
        (Utils.random5String () |> Some)
        (Utils.random5String () |> Some)
        (Utils.random5String () |> Some)

    let! result = Sut.createBook Utils.TestDbConnectionString newBook

    // assert
    let savedBooks = App.Query.getBooks Utils.TestDbConnectionString

    savedBooks |> hasLength "no book was saved" 1

    result
    |> wantOk "result is not ok"
    |> fun savedBookId ->
        let expectedBook =
          R.createBook
            (savedBookId |> W.getBookIdValue |> R.createBookId)
            newBook.Title
            newBook.Author
            newBook.MainTopic
            newBook.Filepath
        let head = savedBooks.Head
        head |> equal "wrong book" expectedBook
  }

[<Tests>]
let commandSpec = testList "command" [ ``it creates a book`` ]
