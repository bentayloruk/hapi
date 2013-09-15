module Doc
open System
open System.Reflection
open System
open ClariusLabs.NuDoc 

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

    member __.NormalizeLink((cref:string)) =
        cref.Replace(":", "-").Replace("(", "-").Replace(")", "");

    override __.VisitMember(m) = 
        writeLine ("### " + m.Info.Name)
        base.VisitMember(m)

    override __.VisitSummary(summary) =
        writeLine (summary.ToText())
        writeBlankLine()
        base.VisitSummary(summary)

    override __.VisitRemarks(remarks) =
        writeBlankLine()
        writeLine "## Remarks"
        base.VisitRemarks(remarks)

    override __.VisitExample(example) =
        writeBlankLine()
        writeLine "### Example"
        base.VisitExample(example)

    override __.VisitClass(c) =
        writeLine <| sprintf "# %s" (c.Info.Name.ToString())

    override __.VisitC(code) =
        // Wrap inline code in ` according to Markdown syntax.
        writeLine " `"
        write code.Content
        write "` "
        base.VisitC(code)

    override __.VisitCode(code) =
        writeBlankLine()
        writeBlankLine()

        let writeCodeLine (line:string) = 
            // Indent code with 4 spaces according to Markdown syntax.
            write "    "
            writeLine line 

        code.Content.Split([| Environment.NewLine |], StringSplitOptions.None)
        |> Array.iter writeCodeLine

        base.VisitCode(code)

    override __.VisitText(text) =
        //Commented out as duping summary.
        //write(text.Content)
        base.VisitText(text)

    override __.VisitPara(para) =
        writeBlankLine()
        writeBlankLine()
        base.VisitPara(para)
        writeBlankLine()
        writeBlankLine()

    override __.VisitSee(see) =
        let cref = __.NormalizeLink(see.Cref)
        Console.Write(" [{0}]({1}) ", cref.Substring(2), cref)

    override __.VisitSeeAlso(seeAlso) =
        let cref = __.NormalizeLink(seeAlso.Cref)
        writeLine (sprintf "[%s](%s)" (cref.Substring(2)) cref)

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
    let writer = MarkdownWriter(args.Write, args.WriteLine)

    //Helper to make visit code cleaner.
    let visit (e:Element) = e.Accept(writer) |> ignore

    //Document a type (struct, class, enum, interface).
    let docType (td:TypeDeclaration) = 

        //Get the ids in this type declaration.
        let tdIds = 
            let memberIdMap = MemberIdMap()
            //Info on TypeDeclaration is a Type.
            memberIdMap.Add(td.Info :?> Type)
            memberIdMap.Ids

        //Constructors, Properties, Methods, Operators, Extension Methods...
        //Write the type dec members (if any).
        let memberTypeGroups =
            tdIds
            |> Seq.choose memberMap.TryFind
            |> Seq.groupBy (fun m -> m.Info.MemberType)

        //Note: Code below here is me printing something for now.  Should not be here. 

        let memberTypeHeading =
            function
            | MemberTypes.Property -> "Properties"
            | MemberTypes.NestedType-> "Nested Types"
            | mt -> mt.ToString() + "s"

        let printMemberGroup (mt,members) =
            //TODO print the TypeInfo first without heading.
            if mt <> MemberTypes.TypeInfo then
                args.WriteLine (sprintf "## %s" <| memberTypeHeading mt)
            members |> Seq.iter visit

        memberTypeGroups |> Seq.iter printMemberGroup
    
    //Document each type dec.
    members.Elements
    |> Seq.choose (|TypeDeclaration|_|)
    |> Seq.iter docType