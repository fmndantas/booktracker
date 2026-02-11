module App.CommonTypes

type BookId = int64
type ReadingLogId = int64
type HookId = int64
type HookCommand = string

[<RequireQualifiedAccess>]
type AppError =
  | GenericError of string
  | BusinessError of string

let appErrorToString appError =
  match appError with
  | AppError.GenericError e
  | AppError.BusinessError e -> e
