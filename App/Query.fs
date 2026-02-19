module App.Query

open System
open System.Data

open Donald

open CommonTypes

type Book = {
  Id: BookId
  Title: string
  Author: string option
  MainTopic: string option
  Filepath: string option
  Modified: DateTime
}

let bookFromDataReader (rd: IDataReader) : Book = {
  Id = rd.ReadInt64 "id"
  Title = rd.ReadString "title"
  Author = rd.ReadStringOption "author"
  MainTopic = rd.ReadStringOption "main_topic"
  Filepath = rd.ReadStringOption "filepath"
  Modified = rd.ReadDateTime "modified" |> _.ToUniversalTime()
}

type ReadingLog = {
  Id: ReadingLogId
  InitialPage: int
  FinalPage: int
  Read: DateTime
  NextTopic: string option
  IdBook: BookId
  Modified: DateTime
}

let readingLogfromDataReader (rd: IDataReader) : ReadingLog = {
  Id = rd.ReadInt64 "id"
  InitialPage = rd.ReadInt32 "initial_page"
  FinalPage = rd.ReadInt32 "final_page"
  Read = rd.ReadDateTime "read" |> _.ToUniversalTime()
  NextTopic = rd.ReadStringOption "next_topic"
  IdBook = rd.ReadInt64 "id_book"
  Modified = rd.ReadDateTime "modified" |> _.ToUniversalTime()
}

type Hook = {
  Id: HookId
  Name: string
  Command: string
}

let hookFromDataReader (rd: IDataReader) : Hook = {
  Id = rd.ReadInt64 "id"
  Name = rd.ReadString "name"
  Command = rd.ReadString "command"
}

let getBooks (conn: Context.BooktrackerConnection) : Book list =
  conn |> Db.newCommand "select * from book" |> Db.query bookFromDataReader

let getHooks (conn: Context.BooktrackerConnection) : Hook list =
  conn |> Db.newCommand "select * from hook" |> Db.query hookFromDataReader

let getBookById (conn: Context.BooktrackerConnection) (bookId: BookId) : Result<Book, AppError list> =
  conn
  |> Db.newCommand "select * from book where id = @id"
  |> Db.setParams [ "id", sqlInt64 bookId ]
  |> Db.querySingle bookFromDataReader
  |> function
    | Some v -> Ok v
    | _ -> Error [ DatabaseError(sprintf "Book with id %d does not exists" bookId) ]

let getReadingLogs (conn: Context.BooktrackerConnection) (bookId: BookId option) : ReadingLog list =
  let filterByBook =
    bookId
    |> Option.map (fun _ -> "where id_book = @id_book")
    |> Option.defaultValue String.Empty

  conn
  |> Db.newCommand $"select * from reading_log {filterByBook} order by datetime(read) desc"
  |> Db.setParams [ "id_book", sqlInt64OrNull bookId ]
  |> Db.query readingLogfromDataReader

let getLastReadingLogByBook (conn: Context.BooktrackerConnection) (bookId: BookId option) : ReadingLog option =
  conn
  |> Db.newCommand (
    (bookId
     |> Option.map (fun _ -> "select * from reading_log where id_book = @id")
     |> Option.defaultValue "select * from reading_log")
    + " order by datetime(read) desc"
  )
  |> Db.setParams [ "id", sqlInt64OrNull bookId ]
  |> Db.querySingle readingLogfromDataReader

let getHookCommandByReadingLog
  (conn: Context.BooktrackerConnection)
  (hookId: HookId)
  (readingLogId: ReadingLogId)
  : Result<string * string, AppError list> =
  let hook =
    conn
    |> Db.newCommand "select * from hook where id = @hook_id"
    |> Db.setParams [ "hook_id", sqlInt64 hookId ]
    |> Db.querySingle hookFromDataReader

  let logReadingWithFilepathOption =
    conn
    |> Db.newCommand
      "
    select a.filepath, b.initial_page, b.final_page, b.next_topic 
    from book a 
    join reading_log b on a.id = b.id_book 
    where b.id = @reading_log_id
    "
    |> Db.setParams [ "reading_log_id", sqlInt64 readingLogId ]
    |> Db.querySingle (fun rd -> {|
      Filepath = rd.ReadStringOption "filepath"
      InitialPage = rd.ReadInt64 "initial_page"
      FinalPage = rd.ReadInt64 "final_page"
      NextTopic = rd.ReadStringOption "next_topic"
    |})

  let errors = [
    if hook.IsNone then
      BusinessError $"Hook with id {hookId} was not found"

    if logReadingWithFilepathOption.IsNone then
      BusinessError $"Reading log with id {readingLogId} was not found"

    if logReadingWithFilepathOption |> Option.exists (fun v -> v.Filepath.IsNone) then
      BusinessError "Book pointed by reading log does not have filepath defined"
  ]

  if errors.Length = 0 then
    match
      hook, logReadingWithFilepathOption, logReadingWithFilepathOption |> (Option.map _.Filepath >> Option.flatten)
    with
    | Some h, Some r, Some filepath ->
      Hook.replacePlaceholders h.Command filepath (int r.InitialPage) (int r.FinalPage) r.NextTopic
    | _ -> failwith "Not possible"
    |> Ok
  else
    Error errors

let getBooksOrderedByLastReadingLog (conn: Context.BooktrackerConnection) =
  conn
  |> Db.newCommand "select * from book_by_last_reading_log"
  |> Db.query bookFromDataReader
