module IntegrationTests.DonaldSpec

open System.Data.SQLite
open Donald
open System

open Expecto

let printDebug (message: string) =
  printfn "[DEBUG - %s]: %s" (DateTime.UtcNow.ToString "O") message

type Mark = {
  Start: string -> unit
  End: string -> unit
}

let createMark () =
  let timer = Diagnostics.Stopwatch.StartNew()

  let startMark =
    fun message ->
      printDebug (sprintf "starting: [%s]" message)
      timer.Restart()

  let endMark =
    fun message -> printDebug (sprintf "ending: [%s]. elapsed: %d ms" message timer.ElapsedMilliseconds)

  { Start = startMark; End = endMark }

let xp =
  testCase "xp"
  <| fun () ->
    let path = __SOURCE_DIRECTORY__ + "/../booktracker.db"
    let conn = new SQLiteConnection $"Data Source={path};Version=3"
    let sql = "select * from book"

    let mark = createMark()

    mark.Start "query"
    let titles = conn |> Db.newCommand sql |> Db.query (fun rd -> rd.ReadString "title")
    mark.End "query"

    mark.Start "query 2"
    let _ = conn |> Db.newCommand sql |> Db.query (fun rd -> rd.ReadString "title")
    mark.End "query 2"

    printfn "%A" titles

[<Tests>]
let donaldSpec = testList "donald" [ xp ]
