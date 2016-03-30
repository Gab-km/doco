namespace Doco

type Head(cssFile: string option) =
    let mutable additionalTags: AdditionalTag list = []
    member private self.MakeStyle(embedded) =
        match cssFile with
        | Some file ->
            // TODO: what to do if there is not 'file'
            use sr = new System.IO.StreamReader(file, System.Text.Encoding.UTF8)
            let css = Utility.minifyCSS sr
            if embedded then
                let encoded = System.Uri.EscapeDataString css
                "  <link href=\"data:text/css:charset=utf-8," + encoded + "\" rel=\"stylesheet\">\n"
            else
                "  <style type='text/css'>" + css + "</style>\n"
        | None -> ""
    member self.ToHtml() =
        "<head>\n" +
        "  <meta charset=\"utf-8\">\n" +
        "  <meta name=\"generator\" content=\"Doconano\">\n" +
        "  <title></title>\n" +
        self.MakeStyle(false) +
        (Tags.toHtmlAllTags <| List.rev additionalTags) +
        "</head>\n"
    member self.InsertTag(tag) =
        additionalTags <- tag::additionalTags
                
type Body(docs) =
    let mutable additionalTags: AdditionalTag list = []
    member self.ToHtml() =
        "<body>\n" +
        (Tags.toHtmlAllTags <| List.rev additionalTags) +
        Documents.toHtmlAllDocs docs +
        "</body>\n"
    member self.InsertTag(tag) =
        additionalTags <- tag::additionalTags

module Html =

    let makeBody docs = Body(docs)

    let makeHead (cssFile: string option) = Head(cssFile)

    let makeHtml output (head: Head) (body: Body) =
        let hd = head.ToHtml()
        let bd = body.ToHtml()
        let fileName =
            match output with
            | Some file -> file
            | None      -> "output.html"
        use sw = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8)
        sw.Write("<!DOCTYPE html>\n<html>\n" + hd + bd + "</html>")
