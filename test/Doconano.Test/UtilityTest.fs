namespace Doconano.Test

open Doco
open Persimmon
open UseTestNameByReflection

module UtilityTest =
    let ``concat should return a string joined a list of string`` =
        let parameterizeTest (xs, expected) = test {
            let actual = Utility.concat xs
            do! assertEquals expected actual
        }
        parameterize {
            case ([], "")
            case (["Hello"; ","; " "; "world"; "!"], "Hello, world!")
            run parameterizeTest
        }
