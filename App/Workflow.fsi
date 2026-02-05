module App.Workflow

open System

module R = ReadDomain
module W = WriteDomain

val createBook: string -> Result<W.BookId, string list>
val createReadingLog: W.BookId -> Result<W.ReadingLogId, string list>

val getBooks: unit -> R.Book list
val getReadingLogs: bookId: R.BookId option -> since: DateTime option -> until: DateTime option -> R.ReadingLog list
