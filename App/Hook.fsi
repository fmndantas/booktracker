module App.Hook

open App.CommonTypes

val replacePlaceholders:
    command: HookCommand ->
    filepath: string ->
    initialPage: int ->
    finalPage: int ->
    nextTopic: string option ->
        (string * string)
