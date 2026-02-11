module App.Hook

open System
open System.Text.RegularExpressions

open App.CommonTypes

// TODO: validate if groups form pairs
// TODO: simplify this function
let replacePlaceholders
  (command: HookCommand)
  (filepath: string)
  (initialPage: int)
  (finalPage: int)
  (nextTopic: string option)
  : (string * string) =
  let replaceFragment (fragment: string) : string =
    fragment
      .Replace("{{initial_page}}", initialPage.ToString())
      .Replace("{{final_page}}", finalPage.ToString())
      .Replace("{{next_topic}}", nextTopic |> Option.defaultValue "")
      .Replace("{{filepath}}", filepath)

  let fragments =
    Regex.Split(command, @"(\[\[.*?\]\])")
    |> Array.choose (fun s ->
      let trim = s.Trim()

      match trim.Length > 0 with
      | true -> Some(trim.StartsWith "[[" && trim.EndsWith "]]", trim.Replace("[[", "").Replace("]]", ""))
      | _ -> None)

  let fragments' =
    fragments
    |> Array.choose (fun (isOptional, s) ->
      if isOptional && nextTopic.IsNone && s.Contains "{{next_topic}}" then
        None
      else
        Some s)

  let replacedFragments = Array.map replaceFragment fragments'

  let finalFragments =
    Regex.Replace(String.Join(" ", replacedFragments), "\s{2,}", " ").Split " "
    |> Array.filter (fun s -> s.Length > 0)

  if fragments.Length = 0 then
    "", ""
  elif finalFragments.Length = 1 then
    finalFragments[0], ""
  else
    finalFragments[0], String.Join(" ", Array.tail finalFragments)
