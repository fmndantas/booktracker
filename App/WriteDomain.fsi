module App.WriteDomain

open System

type BookId
type ReadingLogId

type Book =
    { Title: string
      Author: string option
      MainTopic: string option
      Filepath: string option
      Modified: DateTime }

val createBookId: int64 -> BookId
val createReadingLogId: int64 -> ReadingLogId

val createBook:
    title: string ->
    author: string option ->
    mainTopic: string option ->
    filepath: string option ->
    modified: DateTime ->
        Book

val getBookIdValue: BookId -> int64
val getReadingLogIdValue: ReadingLogId -> int64
