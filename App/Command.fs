module App.Command

module W = WriteDomain

let createBook (connectionString: string) (book: W.Book) : Async<WriteDomain.BookId> =
  async {
    let ctx = Context.getWriteContext connectionString
    let ctxBook = ctx.Main.Book.Create()
    ctxBook.Title <- book.Title
    do! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
    return W.BookId ctxBook.Id
  }
