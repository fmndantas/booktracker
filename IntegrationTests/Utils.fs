module IntegrationTests.Utils

open System

open Donald

open App

// TODO: Move to memory.
// Reference: https://github.com/pimbrouwers/Donald/blob/master/test/Donald.Tests/Tests.fs
let getTestBooktrackerConnection () =
  let path = __SOURCE_DIRECTORY__ + "/../ddl/dummy.db"
  new Context.BooktrackerConnection $"Data Source={path};Version=3"

let randomString (size: int) : string =
  let letters = [ 'a' .. 'z' ]
  let generateNextIdx () = letters |> List.length |> Random().Next

  [ 0 .. (size - 1) ]
  |> List.map (fun _ -> letters[generateNextIdx ()])
  |> String.Concat

let random5String () = randomString 5

let randomInt a b = Random().Next(a, b)

let randomInt1_10 () = randomInt 1 10

let cleanDatabase (conn: Context.BooktrackerConnection) =
  let sql =
    "
    delete from reading_log where id >= 0;  
    delete from book where id >= 0;  
  "

  conn |> Db.newCommand sql |> Db.exec

let createRandomBook (conn: Context.BooktrackerConnection) : Query.Book =
  let book =
    {
      Title = random5String ()
      Author = random5String () |> Some
      MainTopic = random5String () |> Some
      Filepath = random5String () |> Some
    }
    : Command.Book

  conn
  |> Db.newCommand
    "
    insert into book (title, author, main_topic, filepath, modified) 
    values (@title, @author, @main_topic, @filepath, @now);

    select * from book
    where book.id = last_insert_rowid();
    "
  |> Db.setParams [
    "title", SqlType.String book.Title
    "author", sqlStringOrNull book.Author
    "main_topic", sqlStringOrNull book.MainTopic
    "filepath", sqlStringOrNull book.Filepath
    "now", sqlDateTime DateTime.UtcNow
  ]
  |> Db.querySingle Query.bookFromDataReader
  |> function
    | Some v -> v
    | None -> failwith "Random book creation failed"
