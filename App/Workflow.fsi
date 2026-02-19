module App.Workflow

type Mark =
    { Start: string -> unit
      End: string -> unit }

val createOrEditBook: Context.BooktrackerConnection -> bookFolder: string -> Mark -> unit

val getBooks: Context.BooktrackerConnection -> Mark -> unit

val logReading: Context.BooktrackerConnection -> Mark -> unit

val getLastReadingLogsByBook: Context.BooktrackerConnection -> Mark -> unit

val continueLastReading: Context.BooktrackerConnection -> Mark -> unit
