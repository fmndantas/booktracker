module App.Workflow

val createBook: dataContext: Context.DataContext -> bookFolder: string -> Async<unit>
val getBooks: dataContext: Context.ReadDataContext -> Async<unit>

val logReading: readDataContext: Context.ReadDataContext -> dataContext: Context.DataContext -> Async<unit>
val getLastReadingLogsByBook: readDataContext: Context.ReadDataContext -> Async<unit>
