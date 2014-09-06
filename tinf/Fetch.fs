module Fetch

type Request<'a> (t : 'a) =
    member __.fetch() = t

type FetchStatus<'a> =
    | NotFetched
    | FetchSuccess of 'a

type IBlockedRequest = 
    abstract fetch : unit -> unit
    abstract get_request : unit -> obj

type Result<'a> =
    | Done of 'a
    | Blocked of (IBlockedRequest seq) * Fetch<'a>
and Fetch<'a> = Lazy<Result<'a>>

let rec fmap f (lm : Fetch<'a>) = lazy(
    match lm.Force() with
    | Done a          -> Done (f a)
    | Blocked (br, c) -> Blocked (br, (fmap f c)))

let rec app (f : Fetch<'a -> 'b>) (x : Fetch<'a>) : Fetch<'b> = lazy(
    match (f.Force(), x.Force()) with
    | (Done f, Done x)                        -> Done (f x)
    | (Done f, Blocked (br, x))               -> Blocked (br, fmap f x)
    | (Blocked (br, f), Done x)               -> Blocked (br, app f (lazy (Done x)))
    | (Blocked (br1, f), Blocked (br2, x))    -> Blocked (Seq.concat [|br1; br2|], app f x))
   
let wrap (r : Request<'a>) (box : FetchStatus<'a> ref) : IBlockedRequest =
    { new IBlockedRequest with 
      member __.get_request () = r :> obj
      member __.fetch () = box := FetchSuccess (r.fetch()) }

let dataFetch (r : Request<'a>) : Fetch<'a> =
    let box : FetchStatus<'a> ref = ref NotFetched in
    let br : IBlockedRequest = wrap r box in
    let cont = lazy (match !box with FetchSuccess a -> Done a | _ -> failwith (sprintf "Unfetched %A" r))
    in lazy (Blocked (Seq.singleton br, cont))

let fetch (blockers : IBlockedRequest seq) : unit =
    let items = Array.ofSeq blockers in
    printf "Fetching %i blockers\n" items.Length
    items |> Array.iter (fun b -> b.fetch())
   
let rec runFetch (f : Fetch<'a>) : 'a =
    match f.Force() with
    | Done a -> a
    | Blocked (brs, cont) -> fetch brs; runFetch cont