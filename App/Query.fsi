module App.Query

open System.Linq

open CommonTypes

val getBooks: dataContext: Context.ReadDataContext -> IQueryable<Context.Book>

val getBookById: dataContext: Context.ReadDataContext -> BookId -> Result<Context.Book, AppError list>

val getReadingLogs: dataContext: Context.ReadDataContext -> IQueryable<Context.ReadingLog>

/// - Get the last reading log of a book with id `Some id`. 
/// - If the id parameter is None, get the last reading log among all books.
val getLastReadingLogByBook: dataContext: Context.ReadDataContext -> BookId option -> Context.ReadingLog option

val getHookCommandByReadingLog:
    dataContext: Context.ReadDataContext -> HookId -> ReadingLogId -> Result<string * string, AppError list>

val getBooksOrderedByLastReadingLog: dataContext: Context.ReadDataContext -> IQueryable<Context.BookByLastReadingLog>
