module App.WriteDomain

type BookId = BookId of int64
type ReadingLogId = BookId of int64

type Book = { Title: string }
