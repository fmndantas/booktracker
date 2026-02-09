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
        Async<Result<BookId, AppError list>>
