module App.Command

open System

open App.CommonTypes

type Book =
    { Title: string
      Author: string option
      MainTopic: string option
      Filepath: string option }

val createBook:
    Context.BooktrackerConnection ->
    title: string ->
    author: string option ->
    mainTopic: string option ->
    filepath: string option ->
    now: DateTime ->
        Result<BookId, AppError list>

val updateBook:
    Context.BooktrackerConnection ->
    BookId ->
    title: string ->
    author: string option ->
    mainTopic: string option ->
    filepath: string option ->
    now: DateTime ->
        Result<BookId, AppError list>

val deleteBook: Context.BooktrackerConnection -> BookId -> Result<unit, AppError list>

val logReading:
    Context.BooktrackerConnection ->
    BookId ->
    initialPage: int ->
    finalPage: int ->
    nextTopic: string option ->
    now: DateTime ->
        Result<ReadingLogId, AppError list>
