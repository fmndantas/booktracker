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
