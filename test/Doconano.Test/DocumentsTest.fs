namespace Doconano.Test

open Doco
open Persimmon
open UseTestNameByReflection

module DocumentsTest =

    module ``Given Paragraph`` =
        let ``When Documents.(|||) then returns new Paragraph whose attribute is replaced by given id`` = test {
            let p = Documents.p "test"
            let actual = Documents.(|||) p "myID"
            let expected = Paragraph ("test", {ID=Some("myID"); Class=None; Style=None})
            do! assertEquals expected actual
        }
        let ``When Documents.(<<<) then returns new Paragraph whose attribute is replaced by given class`` = test {
            let p = Documents.p "test"
            let actual = Documents.(<<<) p "myClass"
            let expected = Paragraph ("test", {ID=None; Class=Some("myClass"); Style=None})
            do! assertEquals expected actual
        }
        let ``When Documents.(>>>) then returns new Paragraph whose attribute is replaced by given style`` = test {
            let p = Documents.p "test"
            let actual = Documents.(>>>) p "myStyle"
            let expected = Paragraph ("test", {ID=None; Class=None; Style=Some("myStyle")})
            do! assertEquals expected actual
        }

    module ``Given Span`` =
        let ``When Documents.(|||) then returns new Span whose attribute is replaced by given id`` = test {
            let span = Documents.span "test"
            let actual = Documents.(|||) span "myID"
            let expected = Span ("test", {ID=Some("myID"); Class=None; Style=None})
            do! assertEquals expected actual
        }
        let ``When Documents.(<<<) then returns new Span whose attribute is replaced by given class`` = test {
            let span = Documents.span "test"
            let actual = Documents.(<<<) span "myClass"
            let expected = Span ("test", {ID=None; Class=Some("myClass"); Style=None})
            do! assertEquals expected actual
        }
        let ``When Documents.(>>>) then returns new Span whose attribute is replaced by given style`` = test {
            let span = Documents.span "test"
            let actual = Documents.(>>>) span "myStyle"
            let expected = Span ("test", {ID=None; Class=None; Style=Some("myStyle")})
            do! assertEquals expected actual
        }

    module ``Given Div`` =
        let ``When Documents.(|||) then returns new Div whose attribute is replaced by given id`` = test {
            let div = Documents.div <| Documents.rawHtml "test"
            let actual = Documents.(|||) div "myID"
            let expected = Div (RawHtml "test", {ID=Some("myID"); Class=None; Style=None})
            do! assertEquals expected actual
        }
        let ``When Documents.(<<<) then returns new Div whose attribute is replaced by given class`` = test {
            let div = Documents.div <| Documents.rawHtml "test"
            let actual = Documents.(<<<) div "myClass"
            let expected = Div (RawHtml "test", {ID=None; Class=Some("myClass"); Style=None})
            do! assertEquals expected actual
        }
        let ``When Documents.(>>>) then returns new Div whose attribute is replaced by given class`` = test {
            let div = Documents.div <| Documents.rawHtml "test"
            let actual = Documents.(>>>) div "myStyle"
            let expected = Div (RawHtml "test", {ID=None; Class=None; Style=Some("myStyle")})
            do! assertEquals expected actual
        }

    let ``docToHtml`` =
        let parameterizedTest (target, expected) = test {
            let actual = Documents.docToHtml target
            do! assertEquals expected actual
        }
        parameterize {
            case (Documents.p "test", "<p>test</p>")
            case (Documents.h1 "This is a Title", "<h1>This is a Title</h1>")
            case (Documents.h2 "This is another Title", "<h2>This is another Title</h2>")
            case (Documents.h3 "This is a Title, too", "<h3>This is a Title, too</h3>")
            case (Documents.h4 "That is a Title", "<h4>That is a Title</h4>")
            case (Documents.table1 "test" [Documents.rawHtml "yes"],
                    "<table>\n<thead>\n<tr><th>test</th></tr>\n</thead>\n<tr><td>yes</td></tr>\n</table>")
            case (Documents.table2 ("test1", "test2") [Documents.rawHtml "yes", Documents.p "no"],
                    "<table>\n<thead>\n<tr><th>test1</th><th>test2</th></tr>\n</thead>\n<tr><td>yes</td><td><p>no</p></td></tr>\n</table>")
            case (Documents.table3 ("test1", "test2", "test3") [Documents.rawHtml "yes", Documents.p "no", Documents.span "null"],
                    "<table>\n<thead>\n<tr><th>test1</th><th>test2</th><th>test3</th></tr>\n</thead>\n<tr><td>yes</td><td><p>no</p></td><td><span>null</span></td></tr>\n</table>")
            case (Documents.table4 ("test1", "test2", "test3", "test4") [Documents.rawHtml "yes", Documents.p "no", Documents.span "null", Documents.div <| Documents.p "NAN"],
                    "<table>\n<thead>\n<tr><th>test1</th><th>test2</th><th>test3</th><th>test4</th></tr>\n</thead>\n<tr><td>yes</td><td><p>no</p></td><td><span>null</span></td><td><div><p>NAN</p></div></td></tr>\n</table>")
            case (Documents.table5 ("test1", "test2", "test3", "test4", "test5") [Documents.rawHtml "yes", Documents.p "no", Documents.span "null", Documents.div <| Documents.p "NAN", Documents.rawHtml ""],
                    "<table>\n<thead>\n<tr><th>test1</th><th>test2</th><th>test3</th><th>test4</th><th>test5</th></tr>\n</thead>\n<tr><td>yes</td><td><p>no</p></td><td><span>null</span></td><td><div><p>NAN</p></div></td><td></td></tr>\n</table>")
            case (Documents.ol [Documents.rawHtml "ordered list", []], "<ol>\n<li>ordered list</li>\n</ol>")
            case (Documents.ul [Documents.rawHtml "unordered list", [Documents.p "p"]], "<ul>\n<li>unordered list<p>p</p></li>\n</ul>")
            case (Documents.span "span", "<span>span</span>")
            case (Documents.div <| Documents.p "p", "<div><p>p</p></div>")
            case (Documents.rawHtml "<a href='/hoge'>hoge</a>", "<a href='/hoge'>hoge</a>")
            run parameterizedTest
        }
