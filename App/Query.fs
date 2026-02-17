module App.Query

open System.Linq

open CommonTypes

let getBooks (dataContext: Context.ReadDataContext) : IQueryable<Context.Book> = dataContext.Main.Book

let getReadingLogs (dataContext: Context.ReadDataContext) : IQueryable<Context.ReadingLog> = dataContext.Main.ReadingLog

let getLastReadingLogByBook (dataContext: Context.ReadDataContext) (bookId: BookId option) : Context.ReadingLog option =
  let bookIds = bookId |> Option.map Array.singleton |> Option.defaultValue [||]

  let lastLogReading =
    query {
      for log in getReadingLogs dataContext do
        where (bookIds.Length = 0 || bookIds.Contains log.IdBook)
        sortByDescending log.Read
        headOrDefault
    }

  if lastLogReading = null then None else Some lastLogReading

let getHookCommandByReadingLog
  (dataContext: Context.ReadDataContext)
  (hookId: HookId)
  (readingLogId: ReadingLogId)
  : Result<string * string, AppError list> =
  let hook =
    query {
      for h in dataContext.Main.Hook do
        where (h.Id = hookId)
    }
    |> Seq.tryHead

  let readingLogFilepath =
    query {
      for r in dataContext.Main.ReadingLog do
        join book in dataContext.Main.Book on (r.IdBook = book.Id)
        where (r.Id = readingLogId)
        select (r, book.Filepath)
    }
    |> Seq.tryHead

  let readingLogOptional, filepathOptional =
    match readingLogFilepath with
    | Some(r, ValueSome f) -> Some r, Some f
    | Some(r, _) -> Some r, None
    | _ -> None, None

  let errors = [
    if hook.IsNone then
      BusinessError $"Hook with id {hookId} was not found"

    if readingLogOptional.IsNone then
      BusinessError $"Reading log with id {readingLogId} was not found"

    if filepathOptional.IsNone then
      BusinessError $"Book pointed by log does not have a filepath"
  ]

  if errors.Length = 0 then
    match hook, readingLogOptional, filepathOptional with
    | Some h, Some r, Some filepath ->
      Hook.replacePlaceholders
        h.Command
        filepath
        (int r.InitialPage)
        (int r.FinalPage)
        (r.NextTopic |> Option.ofValueOption)
    | _ -> failwith "Not possible"
    |> Ok
  else
    Error errors

let getBooksOrderedByLastReadingLog (dataContext: Context.ReadDataContext) =
  query {
    for v in dataContext.Main.BookByLastReadingLog do
      select v
  }
