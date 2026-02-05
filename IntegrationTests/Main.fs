namespace IntegrationTests

open Expecto

module Main = 
    [<EntryPoint>]
    let main argv =
        runTestsInAssemblyWithCLIArgs [] argv
