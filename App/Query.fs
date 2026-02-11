module App.Query

open System.Linq

let getBooks (dataContext: Context.ReadDataContext) : IQueryable<Context.Book> = dataContext.Main.Book

let getReadingLogs (dataContext: Context.ReadDataContext) : IQueryable<Context.ReadingLog> = dataContext.Main.ReadingLog

let getLastReadingLog (dataContext: Context.ReadDataContext) : Context.ReadingLog option =
  let lastLogReading =
    query {
      for log in getReadingLogs dataContext do
        sortByDescending log.Read
        headOrDefault
    }

  if lastLogReading = null then None else Some lastLogReading
