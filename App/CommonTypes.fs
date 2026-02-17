module App.CommonTypes

type BookId = int64
type ReadingLogId = int64
type HookId = int64
type HookCommand = string

type AppError =
  | GenericError of string
  | BusinessError of string
  | HookError of string
  | DatabaseError of string

let appErrorToString appError =
  match appError with
  | GenericError e
  | BusinessError e
  | HookError e
  | DatabaseError e -> e
