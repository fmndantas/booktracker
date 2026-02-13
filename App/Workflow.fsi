module App.Workflow

val createBook: dataContext: Context.DataContext -> bookFolder: string -> unit
val getBooks: dataContext: Context.ReadDataContext -> unit

val logReading: readDataContext: Context.ReadDataContext -> dataContext: Context.DataContext -> unit
val getLastReadingLogsByBook: readDataContext: Context.ReadDataContext -> unit
val continueLastReading: readDataContext: Context.ReadDataContext -> unit
