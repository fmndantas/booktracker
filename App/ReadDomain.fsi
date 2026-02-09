module App.ReadDomain

open System

type BookId
type ReadingLogId

type Book =
    { Id: BookId
      Title: string
      Author: string option
      MainTopic: string option
      Filepath: string option
      Modified: DateTime }

type ReadingLog =
    { Id: ReadingLogId
      BookId: BookId
      InitialPage: int
      FinalPage: int
      Timestamp: DateTime
      NextTopic: string option }

val createBook:
    id: BookId ->
    title: string ->
    author: string option ->
    mainTopic: string option ->
    filepath: string option ->
    modified: DateTime ->
        Book

val createBookId: int64 -> BookId
val createReadingLogId: int64 -> ReadingLogId

val getBookIdValue: BookId -> int64
val getReadingLogIdValue: ReadingLogId -> int64
