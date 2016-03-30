# 使い方

## 使いはじめる前に

1. `Doco.exe` と `Doconano.dll` をパスの通ったところに格納します。(`AjaxMin.dll` や `FSharp.Compiler.Service.dll` も同じパスに格納してください)
2. `FSharp.Core.optdata` と `FSharp.Core.sigdata` を上記1.と同じパスに格納します。
3. `Doco.exe.config` の `appSettings` にある `FsiPath` の値をお使いの環境に合わせます。

## 使ってみる

F# スクリプトを作成し、 `Doconano.dll` への参照を通しておきます。

```fsharp
#r @"C:\your\path\to\Doconano.dll"
```

`Doco.Documents` と `Doco.Html` (必要に応じて `Doco.Tags` や `Doco.Tables` も)をオープンしておきます。

```fsharp
open Doco.Documents
open Doco.Tables
open Doco.Html
```

ドキュメントを書きます。

```fsharp
let makeDoc() =
    document() {
        do! h1 "テストだよ" ||| "title"
        do! p "Hello, world!"
        do! table2 ("ヘッダ1", "ヘッダ2") (
            [
                plain2 ("ここに", "名前を")
                (rawHtml "入れて", span "ください" >>> "font-color: #FF0000;")
            ])
        do! div <| rawHtml "スタイル指定(右寄せ)"
            >>> "text-align: right;"
    }
```

`docMain : output: string option -> css:string option -> 'a` というシグネチャの関数を定義して、ドキュメントを組み立てます。

```fsharp
let docMain (output: string option) (css: string option) =
    let head = makeHead css
    let body = makeBody <| makeDoc()  // 上で定義したドキュメントの本体
    makeHtml output head body
    0  // とりあえず、終了コードを返しておく
```

あとは `Doco.exe` を実行すると、

```text
$ Doco .\input.fsx
```

以下のような `output.html` が生成されます。

```html
<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <meta name="generator" content="Doconano">
  <title></title>
</head>
<body>

<h1 id="title">テストだよ</h1>
<p>Hello, world!</p>
<table>
<thead>
<tr><th>ヘッダ1</th><th>ヘッダ2</th></tr>
</thead>
<tr><td>ここに</td><td>名前を</td></tr>
<tr><td>入れて</td><td><span style="font-color: #FF0000;">ください</span></td></tr>
</table>
<div style='text-align: right;'>スタイル指定(右寄せ)</div></body>
</html>
```

`Doco.exe` には、出力ファイル名や食べさせるCSSファイルも指定できます。
