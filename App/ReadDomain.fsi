module App.ReadDomain

open System

type BookId
type ReadingLogId

type Book = { Id: BookId; Title: string }

type ReadingLog = {
  Id: ReadingLogId
  BookId: BookId
  InitialPage: int
  FinalPage: int
  Timestamp: DateTime
  NextTopic: string option
}

val createBook: BookId -> string -> Book
val createBookId: int64 -> BookId
val createReadingLogId: int64 -> ReadingLogId

val getBookIdValue: BookId -> int64
val getReadingLogIdValue: ReadingLogId -> int64
