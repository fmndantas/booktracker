module App.Parser

open Argu

type Arguments =
  | [<AltCommandLine("-b")>] Book_Crud
  | [<AltCommandLine("-h")>] Hook_Crud
  | [<AltCommandLine("-gl")>] Get_Logs_By_Book
  | [<AltCommandLine("-l")>] Log_Reading
  | [<AltCommandLine("-c")>] Continue_Last_Reading
  | Debug

  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Book_Crud -> "access book crud"
      | Hook_Crud -> "access hook crud"
      | Get_Logs_By_Book -> "show logs by book"
      | Log_Reading -> "log reading for a book"
      | Continue_Last_Reading -> "continue last reading"
      | Debug -> "run application in debug mode"
