module App.Workflow

open System

type WorkflowError = Value of string

let createBook (_: string) : Result<WriteDomain.BookId, string list> = Error []

let createReadingLog (_: WriteDomain.BookId) : Result<WriteDomain.ReadingLogId, string list> = Error []

let getBooks () : ReadDomain.Book list = []

let getReadingLogs
  (bookId: ReadDomain.BookId option)
  (since: DateTime option)
  (until: DateTime option)
  : ReadDomain.ReadingLog list =
  []
