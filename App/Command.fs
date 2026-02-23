module App.Command

open System

open Donald

open App.CommonTypes

type Book = {
  Title: string
  Author: string option
  MainTopic: string option
  Filepath: string option
}

let createBook
  (conn: Context.BooktrackerConnection)
  (title: string)
  (author: string option)
  (mainTopic: string option)
  (filepath: string option)
  (now: DateTime)
  : Result<BookId, AppError list> =
  conn
  |> Db.newCommand
    "
    insert into book (title, author, main_topic, filepath, modified)
    values (@title, @author, @main_topic, @filepath, @now)
    returning id
    "
  |> Db.setParams [
    "title", sqlString title
    "author", sqlStringOrNull author
    "main_topic", sqlStringOrNull mainTopic
    "filepath", sqlStringOrNull filepath
    "now", sqlDateTime now
  ]
  |> Db.querySingle (fun rd -> rd.ReadInt64 "id")
  |> function
    | Some id -> Ok id
    | None -> Error [ DatabaseError "Book was not created" ]

let bookExists (conn: Context.BooktrackerConnection) (bookId: BookId) : bool =
  conn
  |> Db.newCommand "select id from book where id = @id"
  |> Db.setParams [ "id", sqlInt64 bookId ]
  |> Db.querySingle (fun rd -> rd.ReadInt64 "id")
  |> Option.isSome

let updateBook
  (conn: Context.BooktrackerConnection)
  (bookId: BookId)
  (title: string)
  (author: string option)
  (mainTopic: string option)
  (filepath: string option)
  (now: DateTime)
  : Result<BookId, AppError list> =
  if bookExists conn bookId |> not then
    Error [ BusinessError(sprintf "Book with id %d does not exists" bookId) ]
  else
    conn
    |> Db.newCommand
      "
      update book set 
      title = @title, author = @author, main_topic = @main_topic, filepath = @filepath, modified = @now
      where id = @id
      "
    |> Db.setParams [
      "id", sqlInt64 bookId
      "title", sqlString title
      "author", sqlStringOrNull author
      "main_topic", sqlStringOrNull mainTopic
      "filepath", sqlStringOrNull filepath
      "now", sqlDateTime now
    ]
    |> Db.exec

    Ok bookId

let deleteBook (conn: Context.BooktrackerConnection) (bookId: BookId) : Result<unit, AppError list> =
  conn
  |> Db.newCommand "delete from reading_log where id_book = @id_book; delete from book where id = @id_book;"
  |> Db.setParams [ "id_book", sqlInt64 bookId ]
  |> Db.exec
  |> Ok

let logReading
  (conn: Context.BooktrackerConnection)
  (bookId: BookId)
  (initialPage: int)
  (finalPage: int)
  (nextTopic: string option)
  (now: DateTime)
  : Result<ReadingLogId, AppError list> =
  if bookExists conn bookId |> not then
    Error [ BusinessError(sprintf "Book with id %d does not exists" bookId) ]
  else
    conn
    |> Db.newCommand
      "
      insert into reading_log
      (id_book, initial_page, final_page, read, modified, next_topic)
      values (@id_book, @initial_page, @final_page, @now, @now, @next_topic)
      returning id
      "
    |> Db.setParams [
      "id_book", sqlInt64 bookId
      "initial_page", sqlInt64 initialPage
      "final_page", sqlInt64 finalPage
      "next_topic", sqlStringOrNull nextTopic
      "now", sqlDateTime now
    ]
    |> Db.querySingle (fun rd -> rd.ReadInt64 "id")
    |> function
      | Some id -> Ok id
      | _ -> Error [ DatabaseError "Reading log was not created" ]

let deleteHook (tran: Context.BooktrackerTransaction) (hookId: HookId) : Result<unit, AppError list> =
  tran
  |> Db.newCommandForTransaction "delete from hook where id = @id"
  |> Db.setParams [ "id", sqlInt64 hookId ]
  |> Db.exec
  |> Ok
