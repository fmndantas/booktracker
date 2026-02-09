module App.Query

let getBooks (dataContext: Context.ReadDataContext) : Context.Book list = dataContext.Main.Book |> Seq.toList
