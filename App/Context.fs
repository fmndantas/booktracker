module App.Context

type BooktrackerConnection = System.Data.SQLite.SQLiteConnection

let getBooktrackerConnection sqliteFilepath =
  new BooktrackerConnection $"Data Source={sqliteFilepath};Version=3"
