module App.Context

open FSharp.Data.Sql

[<Literal>]
let dummyDbConnectionString = "DataSource=" + __SOURCE_DIRECTORY__ + "/../ddl/dummy.db"

type SQL =
  SQLite.SqlDataProvider<
    DatabaseVendor=Common.DatabaseProviderTypes.SQLITE,
    SQLiteLibrary=Common.SQLiteLibrary.MicrosoftDataSqlite,
    ConnectionString=dummyDbConnectionString,
    CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL,
    UseOptionTypes=Common.NullableColumnType.VALUE_OPTION
   >

let getReadContext (connectionSting: string) =
  SQL.GetReadOnlyDataContext connectionSting

let getWriteContext (connectionString: string) = SQL.GetDataContext(connectionString)//.``Design Time Commands``.ClearDatabaseSchemaCache.
