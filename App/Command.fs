module App.Command

module W = WriteDomain

let createBook (connectionString: string) (bookToSave: W.Book) : Async<int64> =
  async {
    let ctx = Context.getWriteContext connectionString
    let book = ctx.Main.Book.Create()
    book.Title <- bookToSave.Title
    do! ctx.SubmitUpdatesAsync() |> Async.AwaitTask
    return book.Id
  }
