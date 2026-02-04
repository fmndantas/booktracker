module QuerySpec

open Expecto
open Expecto.Flip.Expect

module sut = App.Query

let testDbConnectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../dummy.db"

let cleanDatabase () : Async<unit> =
    async {
        let ctx = sut.getContext testDbConnectionString
        ctx.Main.ReadingLog |> Seq.iter _.Delete()
        ctx.Main.Book |> Seq.iter _.Delete()
        return! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
    }

let createRandomBook () : Async<unit> =
    async {
        let ctx = sut.getContext testDbConnectionString

        let _ =
            ("book author", "book genre", "book title")
            |> ctx.Main.Book.``Create(author, genre, title)``

        return! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
    }

let ``it get books`` =
    testCaseAsync "sut should get books"
    <| async {
        do! cleanDatabase ()
        do! createRandomBook ()

        sut.getBooks testDbConnectionString
        |> hasLength "Expected non-empty list of books" 1
    }

[<Tests>]
let querySpec = testList "query" [ ``it get books`` ]
