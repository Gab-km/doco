namespace Doco

type ElementAttribute =
    {
        ID: string option
        Class: string option
        Style: string option
    }

module Attributes =
    let setId idName (attr: ElementAttribute) = {ID=Some(idName); Class=attr.Class; Style=attr.Style}
    let setClass className (attr: ElementAttribute) = {ID=attr.ID; Class=Some(className); Style=attr.Style}
    let setStyle style (attr: ElementAttribute) = {ID=attr.ID; Class=attr.Class; Style=Some(style)}
    
    let showId = function
    | {ID=Some(i); Class=_; Style=_} -> " id='" + i + "'"
    | _                              -> ""
    let showClass = function
    | {ID=_; Class=Some(c); Style=_} -> " class='" + c + "'"
    | _                              -> ""
    let showStyle = function
    | {ID=_; Class=_; Style=Some(s)} -> " style='" + s + "'"
    | _                              -> ""
    let showAll attr = showId attr + showClass attr + showStyle attr

    let defaultAttr() = {ID=None; Class=None; Style=None}

type Ordering =
    | Ordered
    | Unordered

type Hd1 = string
type Hd2 = string * string
type Hd3 = string * string * string
type Hd4 = string * string * string * string
type Hd5 = string * string * string * string * string

type Col1 = Document
and Col2 = Document * Document
and Col3 = Document * Document * Document
and Col4 = Document * Document * Document * Document
and Col5 = Document * Document * Document * Document * Document
and Table =
    | TabCol1 of Hd1 * (Col1 list)
    | TabCol2 of Hd2 * (Col2 list)
    | TabCol3 of Hd3 * (Col3 list)
    | TabCol4 of Hd4 * (Col4 list)
    | TabCol5 of Hd5 * (Col5 list)
and Document =
    | Paragraph of string * ElementAttribute
    | H1 of string * ElementAttribute
    | H2 of string * ElementAttribute
    | H3 of string * ElementAttribute
    | H4 of string * ElementAttribute
    | Table of Table
    | List of Ordering * (Document * (Document list)) list
    | Span of string * ElementAttribute
    | Div of Document * ElementAttribute
    | RawHtml of string
    | AggregDoc of Document list
    | PhysicalNewLine

type BuilderValue<'T> =
    | UnitValue of 'T
    | NotUnitValue of 'T

type DocumentBuilder() =
    let mutable env: Document list = []
    member self.Source(x: unit) = UnitValue (x)
    member self.Source(x: _) = NotUnitValue(x)
    member self.Bind(x: BuilderValue<'T>, f) =
        match x with
        | UnitValue(u) -> f ()
        | NotUnitValue(v) ->
            env <- (unbox<Document> v)::PhysicalNewLine::env
            f ()
    member self.Return(x: unit) = RawHtml("")
    member self.Return(x: _) = RawHtml(x)
    member self.Run(x) = List.rev env

module Documents =

    let p text = Paragraph(text, Attributes.defaultAttr())
    let h1 text = H1(text, Attributes.defaultAttr())
    let h2 text = H2(text, Attributes.defaultAttr())
    let h3 text = H3(text, Attributes.defaultAttr())
    let h4 text = H4(text, Attributes.defaultAttr())
    let table1 hd rows = Table <| TabCol1(hd, rows)
    let table2 hd rows = Table <| TabCol2(hd, rows)
    let table3 hd rows = Table <| TabCol3(hd, rows)
    let table4 hd rows = Table <| TabCol4(hd, rows)
    let table5 hd rows = Table <| TabCol5(hd, rows)
    let ol (contents: (Document * (Document list)) list) = List(Ordered, contents)
    let ul (contents: (Document * (Document list)) list) = List(Unordered, contents)
    let span text = Span(text, Attributes.defaultAttr())
    let div doc = Div(doc, Attributes.defaultAttr())
    let rawHtml html = RawHtml html

    let inline (|||) doc idName =
        match doc with
        | Paragraph (text, attr) -> Paragraph(text, Attributes.setId idName attr)
        | Span (text, attr)      -> Span(text, Attributes.setId idName attr)
        | Div (doc, attr)        -> Div(doc, Attributes.setId idName attr)
        | other                  -> other

    let inline (<<<) doc className =
        match doc with
        | Paragraph (text, attr) -> Paragraph(text, Attributes.setClass className attr)
        | Span (text, attr)      -> Span(text, Attributes.setClass className attr)
        | Div (doc, attr)        -> Div(doc, Attributes.setClass className attr)
        | other                  -> other

    let inline (>>>) doc style =
        match doc with
        | Paragraph (text, attr) -> Paragraph(text, Attributes.setStyle style attr)
        | Span (text, attr)      -> Span(text, Attributes.setStyle style attr)
        | Div (doc, attr)        -> Div(doc, Attributes.setStyle style attr)
        | other                  -> other

    let inline (+++) (d1: Document) (d2: Document) =
        match d1 with
        | AggregDoc(docs) -> AggregDoc(d2::docs)
        | _               -> AggregDoc(d2::[d1])

    let rec docToHtml = function
    | Paragraph(t, attr) -> "<p" + Attributes.showAll attr + ">" + t + "</p>"
    | H1(t, attr) -> "<h1" + Attributes.showAll attr + ">" + t + "</h1>"
    | H2(t, attr) -> "<h2" + Attributes.showAll attr + ">" + t + "</h2>"
    | H3(t, attr) -> "<h3" + Attributes.showAll attr + ">" + t + "</h3>"
    | H4(t, attr) -> "<h4" + Attributes.showAll attr + ">" + t + "</h4>"
    | Table(table) ->
        let makeRow make elm rows = (List.fold (fun s1 s2 -> s1 + s2) "<tr>" <| List.map (fun row -> "<" + elm + ">" + make row + "</" + elm + ">") rows) + "</tr>\n"
        let makeHeader = makeRow id "th"
        let makeData = makeRow docToHtml "td"
        let makeTable headers data = "<table>\n<thead>\n" + headers + "</thead>\n" + data + "</table>"
        match table with
        | TabCol1(hd, rows) -> makeTable (makeHeader [hd]) (makeData rows)
        | TabCol2((hd1, hd2), rows) -> makeTable (makeHeader [hd1; hd2]) <| Utility.concat (List.map (fun (s1, s2) -> makeData [s1; s2]) rows)
        | TabCol3((hd1, hd2, hd3), rows) -> makeTable (makeHeader [hd1; hd2; hd3]) <| Utility.concat (List.map (fun (s1, s2, s3) -> makeData [s1; s2; s3]) rows)
        | TabCol4((hd1, hd2, hd3, hd4), rows) -> makeTable (makeHeader [hd1; hd2; hd3; hd4]) <| Utility.concat (List.map (fun (s1, s2, s3, s4) -> makeData [s1; s2; s3; s4]) rows)
        | TabCol5((hd1, hd2, hd3, hd4, hd5), rows) -> makeTable (makeHeader [hd1; hd2; hd3; hd4; hd5]) <| Utility.concat (List.map (fun (s1, s2, s3, s4, s5) -> makeData [s1; s2; s3; s4; s5]) rows)
    | List(ordering, contents) ->
        let childToHtml = List.map (fun (d: Document) -> docToHtml d)
        let line l1 l2 = l1 + "<li>" + (docToHtml <| fst l2) + (Utility.concat <| childToHtml (snd l2)) + "</li>\n"
        let b, e = match ordering with
                    | Ordered   -> "<ol>", "</ol>"
                    | Unordered -> "<ul>", "</ul>"
        let lists = List.fold line (b + "\n") contents
        lists + e
    | Span(t, attr) -> "<span" + Attributes.showAll attr + ">" + t + "</span>"
    | Div(d, attr)  -> "<div" + Attributes.showAll attr + ">" + docToHtml d + "</div>"
    | RawHtml(html) -> html
    | AggregDoc(docs) -> docs |> List.map docToHtml |> List.rev |> Utility.concat
    | PhysicalNewLine -> "\n"
    
    let toHtmlAllDocs = List.map docToHtml >> Utility.concat

    let document something = DocumentBuilder()

module Tables =
    let map1 (f: string -> Document) (col1: Hd1): Col1 = f col1
    let map2 (f: string -> Document) (col2: Hd2): Col2 =
        let x, y = col2
        (f x, f y)
    let map3 (f: string -> Document) (col3: Hd3): Col3 =
        let x, y, z = col3
        (f x, f y, f z)
    let map4 (f: string -> Document) (col4: Hd4): Col4 =
        let x, y, z, w = col4
        (f x, f y, f z, f w)
    let map5 (f: string -> Document) (col5: Hd5): Col5 =
        let x, y, z, w, v = col5
        (f x, f y, f z, f w, f v)
    let plain1 = map1 Documents.rawHtml
    let plain2 = map2 Documents.rawHtml
    let plain3 = map3 Documents.rawHtml
    let plain4 = map4 Documents.rawHtml
    let plain5 = map5 Documents.rawHtml

type AdditionalTag =
    | Script of string
    | RawTag of string
    | AggregTag of AdditionalTag list

module Tags =

    let script file = Script file
    let rawTag tag = RawTag tag

    let inline (@++) (d1: AdditionalTag) (d2: AdditionalTag) =
        match d1 with
        | AggregTag(docs) -> AggregTag(d2::docs)
        | _               -> AggregTag(d2::[d1])

    let rec tagToHtml = function
    | Script (jsfile) ->
        let getScript jsfile =
            if System.IO.File.Exists(jsfile) then
                use sr = new System.IO.StreamReader(jsfile, System.Text.Encoding.UTF8)
                Utility.minifyJS sr
            else
                jsfile
        let script = getScript jsfile
        "<script>" + script + "</script>\n"
    | RawTag (tag) -> tag + "\n"
    | AggregTag(tags) -> tags |> List.map tagToHtml |> List.rev |> Utility.concat

    let toHtmlAllTags = List.map tagToHtml >> Utility.concat
