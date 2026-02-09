module App.Workflow

open System

val createBook: dataContext: Context.DataContext -> bookFolder: string -> Async<unit>
val getBooks: dataContext: Context.ReadDataContext -> Async<unit>
