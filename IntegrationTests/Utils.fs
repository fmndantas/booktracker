module IntegrationTests.Utils

open System

module ctx = App.Context

[<Literal>]
// TODO: rename to camelCase
let TestDbConnectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../dummy.db"

let randomString (size: int) : string =
  let letters = [ 'a' .. 'z' ]
  let generateNextIdx () = letters |> List.length |> Random().Next

  [ 0 .. (size - 1) ]
  |> List.map (fun _ -> letters[generateNextIdx ()])
  |> String.Concat

let random5String () = randomString 5

let cleanDatabase connectionString : Async<unit> =
  async {
    let ctx = ctx.getWriteContext connectionString
    ctx.Main.ReadingLog |> Seq.iter _.Delete()
    ctx.Main.Book |> Seq.iter _.Delete()
    return! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
  }

let createRandomBook connectionString =
  async {
    let ctx = ctx.getWriteContext connectionString

    let newBook = random5String () |> ctx.Main.Book.``Create(title)``

    do! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
    return newBook
  }
