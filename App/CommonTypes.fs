module App.CommonTypes

type BookId = int64

[<RequireQualifiedAccess>]
type AppError = GenericError of string

let appErrorToString appError =
  match appError with
  | AppError.GenericError e -> e
