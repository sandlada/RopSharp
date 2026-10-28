# ADR 0003 — Option asynchronous parity

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md), [ADR 0002](0002-api-gap-remediation.md)

## Context

The 1.1 surface exposed asynchronous aggregation and LINQ for `Result<TOk,TErr>`
(`Rop.CombineAsync`/`ValidateAsync`/`ZipAsync`/`PartitionAsync`/`SequenceAsync`/`TraverseAsync` and
`Select`/`SelectMany` on `Task<Result<,>>`), but `Option<T>` only offered async combinators on
`Task<Option<T>>`. A synchronous `Option<T>` had to be wrapped with `AsTask()` before an asynchronous
mapper, binder, tee or filter could be used, and there was no `Opt.SequenceAsync`, `Opt.TraverseAsync`,
`Opt.CombineAsync`, `Opt.ZipAsync` or `Task<Option<T>>` query support.

That asymmetry contradicted the documented "fully symmetric" surface and forced callers to choose between
`Result` and `Option` based on async ergonomics instead of domain meaning. This ADR records the additive
decisions that close the gap without changing existing semantics.

## Decisions

### D25 — Synchronous options accept asynchronous callbacks

`OptionAsyncExtensions` adds `Option<T>`-source overloads for:
`MapSomeAsync`, `BindSomeAsync` (cancellation-aware), `BindNoneAsync`, `TeeSomeAsync`, `TeeNoneAsync` and
`FilterAsync`. The synchronous counterpart remains the direct method (`MapSome`, `BindSome`, `BindNone`,
`TeeSome`, `TeeNone`, `Filter`); the async names are reserved for callbacks that actually return a task.

**Consequence:** no `AsTask()` ceremony is needed before async work on a present value, and overload
resolution stays unambiguous because the callback arity distinguishes sync, async and cancellation-aware
shapes.

### D26 — Asynchronous option aggregation mirrors the synchronous model

`Opt.CombineAsync` and `Opt.ZipAsync` (2 and 3 inputs) await every supplied task before deciding, so faults
from every input are observed even when an earlier option is `None`. `Opt.SequenceAsync` and
`Opt.TraverseAsync` await in enumeration order and stop at the first `None`; they do not await tasks after
the short-circuit, matching `Rop.SequenceAsync`/`TraverseAsync`. Empty input yields `Some([])`.

`Opt.TraverseAsync` has both `Func<T, Task<Option<TOut>>>` and `Func<T, CancellationToken,
Task<Option<TOut>>>` mapper shapes.

**Consequence:** callers pick concurrency deliberately (eager tasks + `CombineAsync`/`ZipAsync`, or
sequential `SequenceAsync`/`TraverseAsync`) instead of relying on hidden fan-out.

### D27 — Async LINQ covers `Task<Option<T>>`

`OptionLinqExtensions` adds `Select`, `SelectMany` (sync and async binder) and `Where` for
`Task<Option<T>>`. Query syntax now spans awaited options just as it already does for awaited results.

**Consequence:** query syntax has no cancellation-token slot; cancellation remains an outer concern, as
documented for the existing async `Result` LINQ surface.

### D28 — Cancellation semantics are unchanged

The token is optional and last. The active track checks it before invoking a callback; the pass-through
track returns the original value without observing the token. Cancellation throws
`OperationCanceledException` and is never converted to `None`. Callbacks that return a null task fault with
`InvalidOperationException` naming the callback role.

**Consequence:** `Option` and `Result` share one cancellation model; a caller can distinguish "missing" from
"cancelled" by value versus task state.

### D29 — The addition is additive and baseline-checked

All new members are appended to `PublicAPI.Unshipped.txt`; no existing signature changes. The suite covers
the new sync-source overloads, aggregation, cancellation and LINQ paths. `RS0026` is suppressed on
`OptionAsyncExtensions` and `Opt` with a justification because the classes intentionally expose ct-last
overloads that differ by receiver or delegate arity.

**Consequence:** the release is source- and binary-compatible; future overloads still need an explicit
baseline entry and analyzer review.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Require `AsTask()` for every sync-option async callback | Leaks an implementation detail and keeps the surface asymmetric |
| Add synchronous-callback `...Async` overloads for a sync source | Duplicates the existing sync method with no async work; violates one-behaviour-one-name |
| Make `SequenceAsync`/`TraverseAsync` concurrent by default | Hides fan-out and makes failure ordering non-deterministic |
| Use `Task.WhenAll` for every async aggregation | `SequenceAsync`/`TraverseAsync` must report the first `None` in source order and avoid starting later work after short-circuiting |
| Add a token parameter to query syntax | C# query syntax has no token slot; callers compose with the fluent API when cancellation is required |

## Consequences

- `Option` now has the same async vocabulary as `Result`, including cancellation-aware callbacks.
- Async aggregation remains explicit about sequential versus eager awaiting.
- The public API baseline grows; the package version/documentation policy is unchanged by this decision.
