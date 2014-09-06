F# Fetch<'a> implementation
=======================

First attempt at the types and a computation expression to implement the Fetch monad from the ICFP 2014 talk and paper "There is no fork" [1].

It doesn't yet cover parameterising over the Request type which would be required for this to be at all useful. It also don't have the exception handling from the paper.

Right now, given ```**``` as an infix merge (```F<a> -> F<b> -> F<a * b>```) the following code will make three rounds of requests (with ```fetch```) of 2, 2 and one request.

```
let _ = fetch {
            let! (nine, five)    = dataFetch (get 9) ** dataFetch(get 5.0f) in
            let! (eighteen, ten) = dataFetch (get (nine * 2)) ** dataFetch(get (five * 2.0f)) in
            let twentyeight = (float32 eighteen) + ten in
            return! dataFetch (get twentyeight)
        } |> runFetch |> printf "%f\n"
```

Next steps:
  * Parameterising over ```Request<'a>``` and ```fetch```
  * Demo batching/caching fetcher
  * Error Handling
  * Work out nicer syntax for merging/handling non-dependent requests

1: Marlow, Simon, et al. "There is no fork: an abstraction for efficient, concurrent, and concise data access." Proceedings of the 19th ACM SIGPLAN international conference on Functional programming. ACM, 2014.
