module App.Command

module W = WriteDomain

let createBook (connectionString: string) (_: W.Book) : Async<int64> =
  async {
    let ctx = Context.getWriteContext connectionString
    let book = ctx.Main.Book.Create()
    book.Title <- "lkdfjlakjflaskdfj"
    do! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
    return book.Id
  }
