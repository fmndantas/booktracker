module App.Command

open System

open App.CommonTypes

val createBook:
    dataContext: Context.DataContext ->
    title: string ->
    author: string ValueOption ->
    mainTopic: string ValueOption ->
    filepath: string ValueOption ->
    modified: DateTime ->
        Result<BookId, AppError list>

val logReading:
    dataContext: Context.DataContext ->
    bookId: BookId ->
    initialPage: int ->
    finalPage: int ->
    nextTopic: string ValueOption ->
    now: DateTime ->
        Result<ReadingLogId, AppError list>
