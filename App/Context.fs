module App.Context

open FSharp.Data.Sql

[<Literal>]
let dummyDbConnectionString =
  "DataSource=" + __SOURCE_DIRECTORY__ + "/../ddl/dummy.db"

type SQL =
  SQLite.SqlDataProvider<
    DatabaseVendor=Common.DatabaseProviderTypes.SQLITE,
    SQLiteLibrary=Common.SQLiteLibrary.MicrosoftDataSqlite,
    ConnectionString=dummyDbConnectionString,
    CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL,
    UseOptionTypes=Common.NullableColumnType.VALUE_OPTION
   >

type DataContext = SQL.dataContext
type ReadDataContext = SQL.readDataContext

type Book = DataContext.``main.bookEntity``

let getReadContext (connectionSting: string) : ReadDataContext =
  SQL.GetReadOnlyDataContext connectionSting

let getWriteContext (connectionString: string) : DataContext = SQL.GetDataContext connectionString
