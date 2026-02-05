module App.Context

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

let getReadContext (connectionSting: string) =
  SQL.GetReadOnlyDataContext connectionSting

let getWriteContext (connectionString: string) = SQL.GetDataContext connectionString
