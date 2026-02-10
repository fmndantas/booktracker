module App.Parser

open Argu

type Arguments =
  | [<AltCommandLine("-b")>] Get_Books
  | [<AltCommandLine("-l")>] Get_Logs_By_Book 

  interface IArgParserTemplate with
    member s.Usage =
      match s with
      | Get_Books -> "show your books"
      | Get_Logs_By_Book -> "show logs by book"
