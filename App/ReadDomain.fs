module App.ReadDomain

open System

type BookId = BookId of int64
type ReadingLogId = ReadingLogId of int64

type Book = { Id: BookId; Title: string }

type ReadingLog = {
  Id: ReadingLogId
  BookId: BookId
  InitialPage: int
  FinalPage: int
  Timestamp: DateTime
  NextTopic: string option
}

let createBook (id: BookId) (title: string) : Book = { Id = id; Title = title }
