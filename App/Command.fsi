module App.Command

open System

open App.CommonTypes

type Book =
    { Title: string
      Author: string option
      MainTopic: string option
      Filepath: string option }

val createBook:
    Context.BooktrackerTransaction ->
    title: string ->
    author: string option ->
    mainTopic: string option ->
    filepath: string option ->
    now: DateTime ->
        Result<BookId, AppError list>

val updateBook:
    Context.BooktrackerTransaction ->
    BookId ->
    title: string ->
    author: string option ->
    mainTopic: string option ->
    filepath: string option ->
    now: DateTime ->
        Result<BookId, AppError list>

val deleteBook: Context.BooktrackerTransaction -> BookId -> Result<unit, AppError list>

val logReading:
    Context.BooktrackerTransaction ->
    BookId ->
    initialPage: int ->
    finalPage: int ->
    nextTopic: string option ->
    now: DateTime ->
        Result<ReadingLogId, AppError list>

val createHook: Context.BooktrackerTransaction -> name: string -> HookCommand -> Result<HookId, AppError list>

val updateHook: Context.BooktrackerTransaction -> HookId -> name: string -> HookCommand -> Result<HookId, AppError list>

val deleteHook: Context.BooktrackerTransaction -> HookId -> Result<unit, AppError list>
