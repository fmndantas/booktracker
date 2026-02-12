module App.Hook

open System
open System.Text.RegularExpressions

open App.CommonTypes

let replace (pattern: string) (replacement: string) (v: string) = v.Replace(pattern, replacement)
let replaceRegex (pattern: string) (replacement: string) (v: string) = Regex.Replace(v, pattern, replacement)
let trim (v: string) = v.Trim()
let isEmpty (v: string) = v.Length = 0
let join (separator: char) (v: string[]) = String.Join(separator, v)
let split (separator: char) (v: string) = v.Split separator

// TODO: validate if groups form pairs
let replacePlaceholders
  (command: HookCommand)
  (filepath: string)
  (initialPage: int)
  (finalPage: int)
  (nextTopic: string option)
  : (string * string) =
  let replaceFragment (fragment: string) : string =
    fragment
    |> replace "{{initial_page}}" (initialPage.ToString())
    |> replace "{{final_page}}" (finalPage.ToString())
    |> replace "{{next_topic}}" (nextTopic |> Option.defaultValue "")
    |> replace "{{filepath}}" filepath

  let fragments =
    Regex.Split(command, @"(\[\[.*?\]\])")
    |> Array.choose (fun s ->
      let trim = s.Trim()

      match trim.Length > 0 with
      | true -> Some(trim.StartsWith "[[" && trim.EndsWith "]]", trim)
      | _ -> None)

  let filteredFragments =
    fragments
    |> Array.choose (fun (isFragmentOptional, s) ->
      if isFragmentOptional && nextTopic.IsNone && s.Contains "{{next_topic}}" then
        None
      else
        Some s)

  let replacedFragments = Array.map replaceFragment filteredFragments

  let finalFragments =
    replacedFragments
    |> Array.map (replaceRegex "\s{2,}" " " >> replace "[[" "" >> replace "]]" "" >> trim)
    |> Array.filter (isEmpty >> not)
    |> join ' '
    |> split ' '

  finalFragments[0], finalFragments |> Array.tail |> join ' '
