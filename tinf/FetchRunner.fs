module FetchRunner
open Fetch

type FetchRunner() =
    member this.Return(a) : Fetch<'a> = lazy(Done a)
    member this.ReturnFrom(m) = m
    member this.Bind(m : Fetch<'a>, k : 'a -> Fetch<'b>) : Fetch<'b> =
        lazy(match m.Force() with
            | Done a          -> (k a).Force()
            | Blocked (br, c) -> Blocked (br, (this.Bind(c, k)))
        )

let fetch = new FetchRunner()

