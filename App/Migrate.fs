module App.Migrate

open System.IO
open Donald

let (|Int|_|) (value: string) =
  match System.Int32.TryParse value with
  | true, n -> Some n
  | _ -> None

let migrate conn printDebug migrationsFolder : unit =
  printDebug "starting migration"

  let userVersion =
    conn
    |> Db.newCommand "PRAGMA user_version"
    |> Db.query (fun rd -> rd.ReadInt32 "user_version")
    |> List.head

  sprintf "user_version is %d" userVersion |> printDebug

  let migrations =
    migrationsFolder
    |> Directory.GetFiles
    |> Array.choose (fun filepath ->
      let file = Path.GetFileName filepath
      let fragmentos = file.Split "_"

      if fragmentos.Length >= 2 then
        match fragmentos[0] with
        | Int n -> Some(n, filepath)
        | _ -> None
      else
        None)
    |> Array.filter (fst >> (<) userVersion)
    |> Array.sortBy fst

  for _, filepath in migrations do
    sprintf "applying migration %s" filepath |> printDebug

    let sql = File.ReadAllText filepath

    printDebug sql

    let tran = conn.TryBeginTransaction()
    tran |> Db.newCommandForTransaction sql |> Db.exec
    tran.TryCommit()

    (sprintf "PRAGMA user_version = %d" (userVersion + 1), conn)
    ||> Db.newCommand
    |> Db.exec

  ()
