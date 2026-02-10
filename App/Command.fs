module App.Command

open System

open SqliteExtensions

open App.CommonTypes

let createBook
  (dataContext: Context.DataContext)
  (title: string)
  (author: string ValueOption)
  (mainTopic: string ValueOption)
  (filepath: string ValueOption)
  (modified: DateTime)
  : Async<Result<BookId, AppError list>> =
  async {
    let contextBook: Context.Book = dataContext.Main.Book.Create()
    contextBook.Title <- title
    contextBook.Author <- author
    contextBook.MainTopic <- mainTopic
    contextBook.Filepath <- filepath
    contextBook.Modified <- modified.ToSqlite
    do! dataContext.SubmitUpdatesAsync() |> Async.AwaitTask
    return Ok contextBook.Id
  }

let logReading
  (dataContext: Context.DataContext)
  (bookId: BookId)
  (initialPage: int)
  (finalPage: int)
  (nextTopic: string ValueOption)
  (now: DateTime)
  : Async<Result<ReadingLogId, AppError list>> =
  async {
    let bookExists =
      query {
        for book in dataContext.Main.Book do
          exists (book.Id = bookId)
      }

    if bookExists then
      let contextReadingLog = dataContext.Main.ReadingLog.Create()
      contextReadingLog.IdBook <- bookId
      contextReadingLog.InitialPage <- initialPage
      contextReadingLog.FinalPage <- finalPage
      contextReadingLog.NextTopic <- nextTopic
      contextReadingLog.Read <- now.ToSqlite
      contextReadingLog.Modified <- now.ToSqlite
      do! dataContext.SubmitUpdatesAsync() |> Async.AwaitTask
      return Ok contextReadingLog.Id
    else
      return Error [ AppError.BusinessError "Log points to inexistent book" ]
  }
