module App.ReadDomain

open System

type BookId = BookId of int64
type ReadingLogId = ReadingLogId of int64

type Book = {
  Id: BookId
  Title: string
  Author: string option
  MainTopic: string option
  Filepath: string option
  Modified: DateTime
}

type ReadingLog = {
  Id: ReadingLogId
  BookId: BookId
  InitialPage: int
  FinalPage: int
  Timestamp: DateTime
  NextTopic: string option
}

let createBook
  (id: BookId)
  (title: string)
  (author: string option)
  (mainTopic: string option)
  (filepath: string option)
  (modified: DateTime)
  : Book =
  {
    Id = id
    Title = title
    Author = author
    MainTopic = mainTopic
    Filepath = filepath
    Modified = modified
  }

let createBookId v = BookId v
let createReadingLogId v = ReadingLogId v

let getBookIdValue bookId =
  match bookId with
  | BookId v -> v

let getReadingLogIdValue readingLogId =
  match readingLogId with
  | ReadingLogId v -> v
