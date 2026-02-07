module App.CommonTypes

type AppError = GenericError of string

let appErrorToString appError =
  match appError with
  | GenericError e -> e
