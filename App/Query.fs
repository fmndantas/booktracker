module App.Query

open System.Linq

let getBooks (dataContext: Context.ReadDataContext) : IQueryable<Context.Book> = dataContext.Main.Book
let getReadingLogs (dataContext: Context.ReadDataContext) : IQueryable<Context.ReadingLog> = dataContext.Main.ReadingLog
