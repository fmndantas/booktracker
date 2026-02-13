module App.Parser

open Argu

type Arguments =
  | [<AltCommandLine("-b")>] Get_Books
  | [<AltCommandLine("-gl")>] Get_Logs_By_Book
  | [<AltCommandLine("-cb")>] Create_Book
  | [<AltCommandLine("-l")>] Log_Reading
  | [<AltCommandLine("-c")>] Continue_Last_Reading
  | Debug

  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Get_Books -> "show your books"
      | Get_Logs_By_Book -> "show logs by book"
      | Create_Book -> "create a book"
      | Log_Reading -> "log reading for a book"
      | Continue_Last_Reading -> "continue last reading"      
      | Debug -> "run application in debug mode"
