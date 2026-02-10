module App.Parser

open Argu

type Arguments =
  | [<AltCommandLine("-b")>] Get_Books
  | [<AltCommandLine("-l")>] Get_Logs_By_Book 
  | Create_Book
  | Log_Reading

  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Get_Books -> "show your books"
      | Get_Logs_By_Book -> "show logs by book"      
      | Create_Book -> "create a book"
      | Log_Reading -> "log reading for a book"      
