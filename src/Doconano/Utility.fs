namespace Doco

module Utility =
    open System.Text
    open Microsoft.Ajax.Utilities

    let concat strings =
        StringBuilder()
        |> fun sb -> List.fold (fun (s1: StringBuilder) (s2: string) -> s1.Append(s2)) sb strings
        |> fun sb -> sb.ToString()

    let internal minifyJS (sr: System.IO.StreamReader) =
        let minifier = Minifier()
        minifier.WarningLevel <- 3
        let settings = CodeSettings()
        settings.LocalRenaming <- LocalRenaming.CrunchAll
        settings.OutputMode <- OutputMode.SingleLine
        settings.CollapseToLiteral <- true
        minifier.MinifyJavaScript(sr.ReadToEnd(), settings)

    let internal minifyCSS (sr: System.IO.StreamReader) =
        let minifier = Minifier()
        minifier.WarningLevel <- 3
        let settings = CssSettings()
        settings.CssType <- CssType.DeclarationList
        settings.OutputMode <- OutputMode.SingleLine
        minifier.MinifyStyleSheet(sr.ReadToEnd(), settings)
