namespace Doconano.Test

open Doco
open Persimmon
open UseTestNameByReflection

module AttributesTest =

    let ``Attributes.setId should return new ElementAttribute whose id is replaced by given id`` = test {
        let actual = Attributes.defaultAttr() |> Attributes.setId "myID"
        let expected = {ID=Some("myID"); Class=None; Style=None}
        do! assertEquals expected actual
    }
    let ``Attributes.setClass should return new ElementAttribute whose id is replaced by given class`` = test {
        let actual = Attributes.defaultAttr() |> Attributes.setClass "myClass"
        let expected = {ID=None; Class=Some("myClass"); Style=None}
        do! assertEquals expected actual
    }
    let ``Attributes.setStyle should return new ElementAttribute whose id is replaced by given style`` = test {
        let actual = Attributes.defaultAttr() |> Attributes.setStyle "myStyle"
        let expected = {ID=None; Class=None; Style=Some("myStyle")}
        do! assertEquals expected actual
    }
    let ``Attributes.showId should return string which represents an id attribute of the given ElementAttribute`` = test {
        let attr = Attributes.defaultAttr() |>  Attributes.setId "myID"
        let actual = Attributes.showId attr
        do! assertEquals " id='myID'" actual
    }
    let ``Attributes.showClass should return string which represents a class attribute of the given ElementAttribute`` = test {
        let attr = Attributes.defaultAttr() |> Attributes.setClass "myClass"
        let actual = Attributes.showClass attr
        do! assertEquals " class='myClass'" actual
    }
    let ``Attributes.showStyle should return string which represents a style attribute of the given ElementAttribute`` = test {
        let attr = Attributes.defaultAttr() |> Attributes.setStyle "myStyle"
        let actual = Attributes.showStyle attr
        do! assertEquals " style='myStyle'" actual
    }
    let ``Attributes.showAll should return a string of all attributes in the given ElementAttribute`` = test {
        let attr = Attributes.defaultAttr()
        do! assertEquals "" <| Attributes.showAll attr
        let attr = attr |>  Attributes.setId "myID"
        do! assertEquals " id='myID'" <| Attributes.showAll attr
        let attr = attr |> Attributes.setClass "myClass"
        do! assertEquals " id='myID' class='myClass'" <| Attributes.showAll attr
        let attr = attr |> Attributes.setStyle "myStyle"
        do! assertEquals " id='myID' class='myClass' style='myStyle'" <| Attributes.showAll attr
    }
