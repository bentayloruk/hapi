open Doc 
open System
open System.IO

[<EntryPoint>]
let main argv = 

    printfn "Hapi"

    //Our input.
    let assemblies = [
        @"..\..\..\Hapi.Example\bin\Debug\Hapi.Example.dll"
        ]
 
    let docArgs = DocArgs.Console()

    //Do the work.
    assemblies |> Seq.iter (Doc.docAssembly docArgs)

    //Sit around.
    printfn "Hit any key to exit."
    Console.ReadLine() |> ignore

    0


(*
    use file =
        let fileName = """c:\output.md"""
        File.Delete(fileName)
        File.CreateText(fileName)

    let docArgs = {
            Write = (fun text -> file.Write(text));
            WriteLine = (fun line -> file.Write(line + Environment.NewLine))
        }
*)
