module App.Query

open ReadDomain

let getBooks (connectionString: string) : Book list =
  let ctx = Context.getReadContext connectionString
  ctx.Main.Book |> Seq.map (fun b -> createBook b.Id b.Title) |> List.ofSeq
