module App.Query

open System.Linq

val getBooks: dataContext: Context.ReadDataContext -> IQueryable<Context.Book>
val getReadingLogs: dataContext: Context.ReadDataContext -> IQueryable<Context.ReadingLog>
val getLastReadingLog: dataContext: Context.ReadDataContext -> Context.ReadingLog option
