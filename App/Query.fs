module App.Query

open FSharp.Data.Sql

[<Literal>]
let dummyDbConnectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../dummy.db"

type SQL =
    SQLite.SqlDataProvider<
        DatabaseVendor=Common.DatabaseProviderTypes.SQLITE,
        SQLiteLibrary=Common.SQLiteLibrary.MicrosoftDataSqlite,
        ConnectionString=dummyDbConnectionString,
        CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL
     >

let getContext (connectionSting: string) = SQL.GetDataContext connectionSting

let getBooks (connectionString: string) : int64 list =
    let ctx = getContext connectionString
    ctx.Main.Book |> List.ofSeq |> List.map (fun b -> b.Id)
