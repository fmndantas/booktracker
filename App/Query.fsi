module App.Query

open System.Linq

open CommonTypes

val getBooks: dataContext: Context.ReadDataContext -> IQueryable<Context.Book>
val getReadingLogs: dataContext: Context.ReadDataContext -> IQueryable<Context.ReadingLog>
val getLastReadingLogByBook: dataContext: Context.ReadDataContext -> BookId option -> Context.ReadingLog option

val getHookCommandByReadingLog:
    dataContext: Context.ReadDataContext -> HookId -> ReadingLogId -> Result<string * string, AppError list>
