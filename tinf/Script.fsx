#load "Fetch.fs"
#load "FetchRunner.fs"
open Fetch
open FetchRunner

let get (a : 'a) = new Request<'a>(a)

let inline ( ** ) (a : Fetch<'a>) (b : Fetch<'b>) : Fetch<'a * 'b> =
    let tuple a b = (a, b) in app (fmap tuple a) b

let _ = fetch {
            let! (nine, five)    = dataFetch (get 9) ** dataFetch(get 5.0f) in
            let! (eighteen, ten) = dataFetch (get (nine * 2)) ** dataFetch(get (five * 2.0f)) in
            let twentyeight = (float32 eighteen) + ten in
            return! dataFetch (get twentyeight)
        } |> runFetch |> printf "%f\n"