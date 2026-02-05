module App.Command

val createBook: connectionString: string -> book: WriteDomain.Book -> Async<int64>
