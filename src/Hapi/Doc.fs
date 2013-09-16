module Doc
open System
open System.IO
open System.Reflection
open System
open ClariusLabs.NuDoc 

//Type containing the writers.
type DocArgs =
    {
        Write : string -> unit
        WriteLine : string -> unit
    }
    static member Console () = { Write = Console.Write; WriteLine = Console.WriteLine }

//Markdown doc writer (NuDoc visitor).
type private MarkdownWriter(write, writeLine) =
    inherit Visitor()

    let writeBlankLine() = writeLine ""
    let h1 text = writeLine ("# " + text)
    let h2 text = writeLine ("## " + text)
    let h3 text = writeLine ("### " + text)
    let para text = writeLine (text + Environment.NewLine)//Second new line for para.
    let inlineCode text = write "`"; write text; write "`"
    let indentedCodeLine (line:string) = 
        // Indent code with 4 spaces according to Markdown syntax.
        write "    "
        write line 
    let makeLink((cref:string)) =
        cref.Replace(":", "-").Replace("(", "-").Replace(")", "");

    override __.VisitMember(m) = 
        h3 m.Info.Name
        base.VisitMember(m)

    override __.VisitSummary(summary) =
        para (summary.ToText())

    override __.VisitRemarks(remarks) =
        base.VisitRemarks(remarks)

    override __.VisitExample(example) =
        base.VisitExample(example)

    override __.VisitClass(c) =
        h1 (c.Info.Name.ToString())
        base.VisitClass(c)

    override __.VisitC(c) =
        raise <| NotImplementedException()
        base.VisitC(c)

    override __.VisitCode(code) =
        code.Content.Split([| Environment.NewLine |], StringSplitOptions.None)
        |> Array.iter indentedCodeLine
        base.VisitCode(code)

    override __.VisitText(text) = base.VisitText(text)

    override __.VisitPara(para) = base.VisitPara(para)

    override __.VisitSee(see) =
        write (makeLink see.Cref)
        base.VisitSee(see)

    override __.VisitSeeAlso(seeAlso) =
        write (makeLink seeAlso.Cref)
        base.VisitSeeAlso(seeAlso)

//Active patterns for interesting types.
let (|Member|_|) (m:Element) = match m with | :? Member as m -> Some(m) | _ -> None 
let (|Class|_|) (x:Element) = match x with | :? Class as c -> Some(c) | _ -> None
let (|TypeDeclaration|_|) (x:Element) = match x with | :? TypeDeclaration as t -> Some(t) | _ -> None

///Document an assembly.
let docAssembly args path = 

    //Read from assembly so we get the reflection info.
    let ass = Assembly.LoadFrom(path)
    let members = DocReader.Read(ass)

    //Map all from id to member.
    let memberMap = 
        members.Elements
        |> Seq.choose (|Member|_|)
        |> Seq.map (fun m -> m.Id, m)
        |> Map.ofSeq

    //TODO take writer in args.
    let visitor = MarkdownWriter(args.Write, args.WriteLine)

    //Helper to make visit code cleaner.
    let visit (e:Element) = e.Accept(visitor) |> ignore

    //Document a type (struct, class, enum, interface).
    let docType (td:TypeDeclaration) = 

        //Group ALA MSDN by Constructors, Properties, Methods, Operators, Extension Methods...
        let memberGroups =
            let memberIdMap = MemberIdMap()
            memberIdMap.Add(td.Info :?> Type)//Always a Type for TD.
            memberIdMap.Ids
            |> Seq.choose memberMap.TryFind
            |> Seq.groupBy (fun m -> m.Info.MemberType)

        //Helper to print a group.
        let printMemberGroup (mt,members) =

            //Helper that makes member group headings.
            let memberTypeHeading memberType =
                match memberType with
                | MemberTypes.Property -> "Properties"
                | MemberTypes.NestedType-> "Nested Types"
                | mt -> mt.ToString() + "s"

            //TODO print the TypeInfo first without heading.
            if mt <> MemberTypes.TypeInfo then
                args.WriteLine (sprintf "## %s" <| memberTypeHeading mt)
            members |> Seq.iter visit

        //Print all the groups!
        memberGroups |> Seq.iter printMemberGroup
    
    //Document each type dec.
    members.Elements
    |> Seq.choose (|TypeDeclaration|_|)
    |> Seq.iter docType