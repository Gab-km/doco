namespace Doconano.Test

open Doco
open Persimmon
open UseTestNameByReflection

module TagsTest =

    let ``tagToHtml`` =
        let parameterizedTest (target, expected) = test {
            let actual = Tags.tagToHtml target
            do! assertEquals expected actual
        }
        parameterize {
            case (Tags.script "alert();", "<script>alert();</script>\n")
            case (Tags.rawTag "<!--[if lt IE 9]>;", "<!--[if lt IE 9]>;\n")
            run parameterizedTest
        }
