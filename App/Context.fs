module App.Context

type BooktrackerConnection = System.Data.SQLite.SQLiteConnection

type BooktrackerTransaction = System.Data.IDbTransaction

let getBooktrackerConnection (sqliteFilepath: string) =
  new BooktrackerConnection $"Data Source={sqliteFilepath};Version=3"
