module App.Workflow

open System

module R = ReadDomain
module W = WriteDomain

val createBook: connectionString: string -> bookFolder: string -> Async<Result<W.BookId, string list>>
