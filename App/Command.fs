module App.Command

module W = WriteDomain

open App.CommonTypes

let createBook (connectionString: string) (book: W.Book) : Async<Result<WriteDomain.BookId, AppError list>> =
  async {
    let context = Context.getWriteContext connectionString
    let contextBook = context.Main.Book.Create()
    contextBook.Title <- book.Title
    contextBook.Author <- book.Author |> ValueOption.ofOption
    contextBook.MainTopic <- book.MainTopic |> ValueOption.ofOption
    contextBook.Filepath <- book.Filepath |> ValueOption.ofOption
    do! context.SubmitUpdatesAsync() |> Async.AwaitTask
    return contextBook.Id |> W.createBookId |> Ok
  }
