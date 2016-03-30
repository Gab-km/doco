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

    let private (|Execute|ShowHelp|) argv =
        let len = Array.length argv
        if len = 0 || len >= 4 then ShowHelp
        else Execute

    let private showHelp() =
        printfn "Usage:"
        printfn "  Doco INPUT [OUTPUT [STYLESHEET]]"
        printfn ""
        printfn "Arguments:"
        printfn "  INPUT         An input file path."
        printfn "  OUTPUT        An output file path."
        printfn "                If none, 'output.html' will be created."
        printfn "  STYLESHEET    A CSS(Cascading Style Sheets) file path."
        printfn "                If none, no stylesheets will be included."
        printfn ""
        ()

    [<EntryPoint>]
    let main argv =
        match argv with
        | Execute  -> evalScript argv
        | ShowHelp -> showHelp(); 0
