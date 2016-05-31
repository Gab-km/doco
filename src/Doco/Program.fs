namespace Doco

open System.Text

type Argument =
    {
        Css: string option
        Output: string option
    }
    override self.ToString() =
        StringBuilder()
        |> fun sb -> sb.Append("(")
        |> fun sb -> match self.Output with
                        | Some output -> sb.Append("Some(\"" + output + "\")")
                        | None        -> sb.Append("None")
        |> fun sb -> sb.Append(") (")
        |> fun sb -> match self.Css with
                        | Some css -> sb.Append("Some(\"" + css + "\")")
                        | None     -> sb.Append("None")
        |> fun sb -> sb.Append(")")
        |> fun sb -> sb.ToString()

module Program =
    open System
    open System.IO
    open Microsoft.FSharp.Compiler
    open Microsoft.FSharp.Compiler.Interactive.Shell

    let private argumentFrom (argv: string []) =
        let inputPath = argv.[0]    // mandatory
        let argument = {            // optional
            Output =
                if argv.Length > 1 && not <| String.IsNullOrEmpty(argv.[1]) then Some(argv.[1])
                else None
            Css =
                if argv.Length > 2 && not <| String.IsNullOrEmpty(argv.[2]) then Some(argv.[2])
                else None
        }
        inputPath, argument

    let private makeTempDocFile (argv: string []) =
        let inputPath, arg = argumentFrom argv
        use sr = new StreamReader(inputPath)
        let script = sr.ReadToEnd() + "\n" +
                        "docMain " +
                        arg.ToString()
        let tempfile = Path.GetTempPath() +
                        Guid.NewGuid().ToString() +
                        ".fsx"
        use sw = new StreamWriter(tempfile)
        sw.WriteLine(script)
        tempfile

    let private getAppSettingsOrDefault (key: string) defaultValue =
        let value = System.Configuration.ConfigurationManager.AppSettings.Item(key)
        if String.IsNullOrEmpty(value) then defaultValue
        else value

    let private makeFsiSession outStream errStream =
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
        let fsiPath = getAppSettingsOrDefault "FsiPath" "Fsi.exe"
        let args = [|fsiPath|]
        let allArgs = Array.append args [|"--noninteractive"; "--noframework"|]
        use inStream = new StringReader("")
        FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

    let private evalResult (sbOut: StringBuilder) (sbErr: StringBuilder) result warnings =
        Array.iter
            (fun (warn: FSharpErrorInfo) ->
                printfn "警告 %s 場所 %d,%d" warn.Message warn.StartLineAlternate warn.StartColumn)
            warnings
        match result with
        | Choice1Of2(_) ->
            0
        | Choice2Of2(_) ->
            Console.Error.WriteLine(sbErr.ToString())
            1

    let private evalScript (argv: string []) =
        let sbOut = new StringBuilder()
        let sbErr = new StringBuilder()
        use outStream = new StringWriter(sbOut)
        use errStream = new StringWriter(sbErr)
        let fsiSession = makeFsiSession outStream errStream
        let tempfile = makeTempDocFile argv
        try
            let result, warnings = fsiSession.EvalScriptNonThrowing tempfile
            evalResult sbOut sbErr result warnings
        finally
            File.Delete tempfile

    let private initialize (dirName: string) =
        let sep = Path.DirectorySeparatorChar
        // create a new directory
        let newDirPath = sprintf "%s%c%s" Environment.CurrentDirectory sep dirName
        let dirInfo = Directory.CreateDirectory newDirPath
        // create a sample stylesheet
        let newStylePath = sprintf "%s%c%s" newDirPath sep "style.css"
        use fs1 = File.Create newStylePath
        use sw1 = new StreamWriter(fs1)
        sw1.Write(Templates.style)
        // create a sample document script
        let newDocumentPath = sprintf "%s%c%s" newDirPath sep "document.fsx"
        use fs2 = File.Create newDocumentPath
        use sw2 = new StreamWriter(fs2)
        sw2.Write(Templates.script)

    let private (|Initialize|Execute|ShowHelp|) argv =
        let len = Array.length argv
        if len = 0 then ShowHelp
        else
            match argv.[0], len with
            | "init", 2                       -> Initialize
            | "make", n when 2 <= n && n <= 4 -> Execute
            | _                               -> ShowHelp

    let private showHelp() =
        printfn "Usage:"
        printfn "  Doco COMMAND [...]"
        printfn ""
        printfn "Command:"
        printfn "  init DIRNAME"
        printfn "       DIRNAME       A root directory name to initialize."
        printfn "  make INPUT [OUTPUT [STYLESHEET]]"
        printfn "       INPUT         An input file path."
        printfn "       OUTPUT        An output file path."
        printfn "                     If none, 'output.html' will be created."
        printfn "       STYLESHEET    A CSS(Cascading Style Sheets) file path."
        printfn "                     If none, no stylesheets will be included."
        printfn ""
        ()

    [<EntryPoint>]
    let main argv =
        match argv with
        | Initialize -> initialize argv.[1]; 0
        | Execute    -> evalScript <| Array.skip 1 argv
        | ShowHelp   -> showHelp(); 0
