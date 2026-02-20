module IntegrationTests.DonaldSpec

open System

open Donald

open Expecto
open Expecto.Flip.Expect

open App

let countBooks conn =
  conn
  |> Db.newCommand "select count(*) as cnt from book"
  |> Db.querySingle (fun rd -> rd.ReadInt64 "cnt")
  |> Option.get

let ``test rollback`` =
  "test rollback",
  fun (conn: Context.BooktrackerConnection) ->
    let tran = conn.TryBeginTransaction()

    tran
    |> Db.newCommandForTransaction "insert into book (title, modified) values (@title, @now)"
    |> Db.execMany (
      [
        "title", Utils.random5String () |> sqlString
        "now", DateTime.UtcNow |> sqlDateTime
      ]
      |> List.replicate 10
    )

    // This test passes if this line is discommented
    // This happens because rollback undoes book insertions
    // tran.TryRollback()

    conn |> countBooks |> equal "wrong count" 0

let ``test commit`` =
  "test commit",
  fun (conn1: Context.BooktrackerConnection, conn2: Context.BooktrackerConnection) ->
    let tran = conn1.TryBeginTransaction()

    tran
    |> Db.newCommandForTransaction "insert into book (title, modified) values (@title, @now)"
    |> Db.execMany (
      [
        "title", Utils.random5String () |> sqlString
        "now", DateTime.UtcNow |> sqlDateTime
      ]
      |> List.replicate 10
    )

    conn1 |> countBooks |> equal "wrong conn1 count before" 10
    // Connection 2 does not see connection 1 insertions until they are commited
    conn2 |> countBooks |> equal "wrong conn2 count before" 0

    tran.TryCommit()

    conn1 |> countBooks |> equal "wrong conn1 count after" 10
    conn2 |> countBooks |> equal "wrong conn2 count after" 10

[<PTests>]
let DonaldSpec =
  testList "donald" [
    yield! testFixture Utils.memoryDbFixture [ ``test rollback`` ]
    yield! testFixture Utils.twoConnectionsFixture [ ``test commit`` ]
  ]
