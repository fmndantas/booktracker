module IntegrationTests.HookSpec

open Expecto
open Expecto.Flip.Expect

open Utils

module Sut = App.Hook

let ``it replaces parameters in hook command`` =
  testTheory "it replaces parameters in hook command" [
    1,
    "sioyek {{filepath}} --initial-page {{initial_page}} --final-page {{final_page}} [[--next-topic \"{{next_topic}}\"]]",
    "foo.pdf",
    1,
    2,
    Some "next topic",
    ("sioyek", "foo.pdf --initial-page 1 --final-page 2 --next-topic \"next topic\"")

    2,
    "foo [[--blah-blah {{next_topic}}]] bar",
    random5String (),
    randomInt1_10 (),
    randomInt1_10 (),
    None,
    ("foo", "bar")

    3,
    "foo --blah-blah {{next_topic}} bar",
    random5String (),
    randomInt1_10 (),
    randomInt1_10 (),
    None,
    ("foo", "--blah-blah bar")

    4, "", random5String (), randomInt1_10 (), randomInt1_10 (), None, ("", "")

    5, "foo {{filepath}}", "foo.pdf", randomInt1_10 (), randomInt1_10 (), random5String () |> Some, ("foo", "foo.pdf")

    6,
    "foo [[    --file    {{filepath}}  ]]",
    "foo.pdf",
    randomInt1_10 (),
    randomInt1_10 (),
    random5String () |> Some,
    ("foo", "--file foo.pdf")

    7,
    "foo     --file    {{filepath}}  ",
    "foo.pdf",
    randomInt1_10 (),
    randomInt1_10 (),
    random5String () |> Some,
    ("foo", "--file foo.pdf")
  ]
  <| fun (_, command, filepath, initialPage, finalPage, nextTopic, expectedCommand) ->
    let result =
      Sut.replacePlaceholders command filepath initialPage finalPage nextTopic

    result |> equal "result is wrong" expectedCommand

[<Tests>]
let querySpec = testList "hook" [ ``it replaces parameters in hook command`` ]
