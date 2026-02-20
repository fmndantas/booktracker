module App.Workflow

type Mark =
    { Start: string -> unit
      End: string -> unit }

val bookCrud: Context.BooktrackerConnection -> bookFolder: string -> Mark -> unit

val logReading: Context.BooktrackerConnection -> Mark -> unit

val getLastReadingLogsByBook: Context.BooktrackerConnection -> Mark -> unit

val continueLastReading: Context.BooktrackerConnection -> Mark -> unit
