# ADR 0004 — Validation traversal on the synchronous and asynchronous tracks

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md), [ADR 0002](0002-api-gap-remediation.md),
  [ADR 0003](0003-option-async-parity.md)

## Context

The library exposes two explicit result models: the short-circuiting track (`Result<T, TErr>`) and the
aggregating track (`Result<T, IReadOnlyList<TErr>>`). The aggregating surface was reachable through
`Rop.Validate`, `Rop.SequenceValidation` and `Rop.ValidateAsync`, but the collection helpers were incomplete:

- there was no `Rop.TraverseValidation`, so callers that map a source sequence had to build the sequence of
  results first, and
- there was no `Rop.SequenceValidationAsync` or `Rop.TraverseValidationAsync`, so asynchronous validation had to
  be written as `await Task.WhenAll(...)` plus a manual fold.

Both gaps pushed callers back to hand-written aggregation, which is exactly where the "collect every error"
guarantee tends to be lost. This ADR records the additive decisions that close them.

## Decisions

### D30 — `Rop.TraverseValidation` completes the synchronous validation trio

`Rop.TraverseValidation<T, TOut, TErr>` maps every element with a synchronous mapper and returns
`Result<IReadOnlyList<TOut>, IReadOnlyList<TErr>>`. It has the same contract as `Rop.SequenceValidation`: the
mapper is invoked for every element, successes are kept in element order, and errors are collected in element
order (never empty on the failure track). Empty input yields `Ok([])`.

**Consequence:** mapping and aggregating no longer need two steps, and the synchronous and asynchronous
vocabularies now line up (`Sequence`/`Traverse`, `SequenceValidation`/`TraverseValidation`).

### D31 — Asynchronous validation traversal awaits every task

`Rop.SequenceValidationAsync` and `Rop.TraverseValidationAsync` accept the same inputs as
`Rop.SequenceAsync`/`Rop.TraverseAsync` but await all of them instead of short-circuiting, because an
aggregating operation cannot decide until every input has been inspected.

Waiting is delegated to `Task.WhenAll`, so every faulted task is observed even when an earlier element already
failed or faulted. When more than one task faults, the first fault wins; validation errors remain values on the
failure track and are never merged into a single exception.

`Rop.TraverseValidationAsync` has both `Func<T, Task<Result<TOut, TErr>>>` and
`Func<T, CancellationToken, Task<Result<TOut, TErr>>>` mapper shapes, matching the Option and Result
`TraverseAsync` conventions.

**Consequence:** callers choose between `SequenceAsync`/`TraverseAsync` (first error wins, later work skipped) and
the validation variants (every error collected) without writing their own fan-out.

### D32 — Faults are not domain errors

A task that throws surfaces as an exception. A task that completes with `Err(...)` contributes an element to the
aggregated error list. `Task.WhenAll` semantics are preserved deliberately: fault handling belongs to the caller,
not to the validation track.

**Consequence:** the boundary between "a bug crashed the pipeline" and "the domain rejected this input" stays
visible, exactly as it does for the synchronous helpers.

### D33 — Cancellation semantics are unchanged

The token is optional and last. The token is checked before the sequence starts, before each mapper invocation,
and immediately before awaiting the tasks; cancellation throws `OperationCanceledException` and is never converted
into an error. A mapper that returns a null task faults with `InvalidOperationException` naming the callback.

**Consequence:** validation traversal follows the same rules as every other asynchronous combinator, so a caller
can distinguish "cancelled" from "invalid" by task state versus value.

### D34 — The addition is additive and baseline-checked

All new members are appended to `PublicAPI.Unshipped.txt`; no existing signature changes and no overloads become
ambiguous (the mapper delegates differ by arity, and `SequenceValidationAsync` has a single shape).

**Consequence:** the release is source- and binary-compatible.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Reuse `SequenceAsync`/`TraverseAsync` and accumulate errors at the call site | Forces hand-written `Task.WhenAll` plus folding, which is the behaviour being added |
| Short-circuit validation traversal on the first error | Defeats the aggregating contract; callers can use `SequenceAsync` for that |
| Await tasks sequentially and stop at the first fault | Later faults would go unobserved; `Task.WhenAll` already observes them |
| Introduce a dedicated `Validation<T, TErr>` type | Duplicates the existing `Result<T, IReadOnlyList<TErr>>` track and adds a fourth core concept |
| Accept `IAsyncEnumerable<T>` | Bounded-concurrency and streaming semantics are a separate design decision; the existing helpers stay `IEnumerable<T>` based |

## Consequences

- Validation is now reachable from a bare sequence on both the synchronous and asynchronous tracks.
- Faults, cancellation and validation errors stay distinct.
- The public API baseline grows; the package version/documentation policy is unchanged by this decision.
