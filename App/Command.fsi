module App.Command

val createBook: connectionString: string -> book: WriteDomain.Book -> Async<Result<WriteDomain.BookId, string list>>
