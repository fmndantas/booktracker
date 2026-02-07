module App.Command

open App.CommonTypes

val createBook: connectionString: string -> book: WriteDomain.Book -> Async<Result<WriteDomain.BookId, AppError list>>
