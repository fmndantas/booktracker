module App.WriteDomain

type BookId
type ReadingLogId

type Book = { Title: string }

val createBookId: int64 -> BookId
val createReadingLogId: int64 -> ReadingLogId

val getBookIdValue: BookId -> int64
val getReadingLogIdValue: ReadingLogId -> int64
