module App.Query

module R = ReadDomain

let getBooks (connectionString: string) : R.Book list =
  let ctx = Context.getReadContext connectionString

  ctx.Main.Book
  |> Seq.map (fun b ->
    R.createBook
      (R.createBookId b.Id)
      b.Title
      (b.Author |> Option.ofValueOption)
      (b.MainTopic |> Option.ofValueOption)
      (b.Filepath |> Option.ofValueOption))
  |> List.ofSeq
