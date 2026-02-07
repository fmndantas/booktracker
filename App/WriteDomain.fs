module App.WriteDomain

type BookId = BookId of int64
type ReadingLogId = ReadingLogId of int64

type Book = {
  Title: string
  Author: string option
  MainTopic: string option
  Filepath: string option
}

let createBookId v = BookId v
let createReadingLogId v = ReadingLogId v

let createBook (title: string) (author: string option) (mainTopic: string option) (filepath: string option) : Book = {
  Title = title
  Author = author
  MainTopic = mainTopic
  Filepath = filepath
}

let getBookIdValue bookId =
  match bookId with
  | BookId v -> v

let getReadingLogIdValue readingLogId =
  match readingLogId with
  | ReadingLogId v -> v
