namespace Doconano.Test

open Doco
open Persimmon
open UseTestNameByReflection

module TablesTest =
    let ``map1`` = test {
        let actual = Tables.map1 Documents.p "test"
        let expected = Documents.p "test"
        do! assertEquals expected actual
    }

    let ``map2`` = test {
        let actual = Tables.map2 Documents.span ("test1", "test2")
        let expected = Documents.span "test1", Documents.span "test2"
        do! assertEquals expected actual
    }

    let ``map3`` = test {
        let actual = Tables.map3 Documents.h1 ("test1", "test2", "test3")
        let expected = Documents.h1 "test1", Documents.h1 "test2", Documents.h1 "test3"
        do! assertEquals expected actual
    }

    let ``map4`` = test {
        let f = Documents.div << Documents.p
        let actual = Tables.map4 f ("test1", "test2", "test3", "test4")
        let expected = f "test1", f "test2", f "test3", f "test4"
        do! assertEquals expected actual
    }

    let ``map5`` = test {
        let actual = Tables.map5 Documents.rawHtml ("test1", "test2", "test3", "test4", "test5")
        let expected = Documents.rawHtml "test1", Documents.rawHtml "test2", Documents.rawHtml "test3", Documents.rawHtml "test4", Documents.rawHtml "test5"
        do! assertEquals expected actual
    }
