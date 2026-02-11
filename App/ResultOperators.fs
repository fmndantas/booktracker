[<AutoOpen>]
module App.ResultOperators

type ResultBuilder() =
  member _.Bind(m, f) = Result.bind f m
  member _.Return v = Ok v
  member _.ReturnFrom v = v

let result = ResultBuilder()
