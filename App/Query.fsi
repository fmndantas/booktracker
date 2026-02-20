module App.Query

open System
open System.Data

open CommonTypes

type Book =
    { Id: BookId
      Title: string
      Author: string option
      MainTopic: string option
      Filepath: string option
      Modified: DateTime }

type ReadingLog =
    { Id: ReadingLogId
      InitialPage: int
      FinalPage: int
      Read: DateTime
      NextTopic: string option
      IdBook: BookId
      Modified: DateTime }

type Hook =
    { Id: HookId
      Name: string
      Command: string }

val bookFromDataReader: IDataReader -> Book
val readingLogfromDataReader: IDataReader -> ReadingLog
val hookFromDataReader: IDataReader -> Hook

val getBooks: Context.BooktrackerConnection -> Book list

val getHooks: Context.BooktrackerConnection -> Hook list

val getBookById: Context.BooktrackerConnection -> BookId -> Result<Book, AppError list>

val getReadingLogs: Context.BooktrackerConnection -> BookId option -> ReadingLog list

/// - Get the last reading log of a book with id `Some id`.
/// - If the id parameter is None, get the last reading log among all books.
val getLastReadingLogByBook: Context.BooktrackerConnection -> BookId option -> ReadingLog option

val getHookCommandByReadingLog:
    Context.BooktrackerConnection -> HookId -> ReadingLogId -> Result<string * string, AppError list>

val getBooksOrderedByLastReadingLog: Context.BooktrackerConnection -> Book list
