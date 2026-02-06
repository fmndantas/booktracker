module App.WriteDomain

type BookId = BookId of int64
type ReadingLogId = ReadingLogId of int64

type Book = { Title: string }

let createBookId v = BookId v 
let createReadingLogId v = ReadingLogId v

let getBookIdValue bookId = match bookId with BookId v -> v
let getReadingLogIdValue readingLogId = match readingLogId with ReadingLogId v -> v
