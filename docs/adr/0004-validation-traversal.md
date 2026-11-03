# ADR 0004 — Validation traversal on the synchronous and asynchronous tracks

- **Status:** Accepted
- **Date:** 2026-10-28
- **Revised:** 2026-11-03
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md), [ADR 0002](0002-api-gap-remediation.md),
  [ADR 0003](0003-option-async-parity.md)

## Context

The library exposes two explicit result models: the short-circuiting track (`Result<T, TErr>`) and the accumulating
track (`Validation<T, TErr>`). Before 0.0.2, aggregation lived on `Result<T, IReadOnlyList<TErr>>` via
`Result.Validate`, `Result.SequenceValidation`, and related async helpers. That overload of `Result` mixed two semantics
in one type; 0.0.2 moves traversal and zip/sequence aggregation onto `Validation` while keeping the same behavioural
guarantees documented below.

## Decisions

### D30 — `Validation.Traverse` completes the synchronous validation trio

`Validation.Traverse<T, TOut, TErr>` maps every element with a synchronous mapper and returns
`Validation<IReadOnlyList<TOut>, TErr>`. It has the same contract as `Validation.Sequence`: the mapper is invoked for
every element, successes are kept in element order, and errors are collected in element order (never empty on the
failure track). Empty input yields `Success([])`.

**Consequence:** mapping and aggregating no longer need two steps, and the synchronous and asynchronous vocabularies
now line up (`Sequence`/`Traverse` on each track, with `Validation` for accumulation).

### D31 — Asynchronous validation traversal awaits every task

`Validation.SequenceAsync` and `Validation.TraverseAsync` accept the same inputs as `Result.SequenceAsync` /
`Result.TraverseAsync` but await all of them instead of short-circuiting, because an aggregating operation cannot
decide until every input has been inspected.

Waiting is delegated to `Task.WhenAll`, so every faulted task is observed even when an earlier element already
failed or faulted. When more than one task faults, the first fault wins; validation errors remain values on the
failure track and are never merged into a single exception.

`Validation.TraverseAsync` has both `Func<T, Task<Result<TOut, TErr>>>` and
`Func<T, CancellationToken, Task<Result<TOut, TErr>>>` mapper shapes, matching the Option and Result `TraverseAsync`
conventions.

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
| Introduce a dedicated `Validation<T, TErr>` type | **Accepted in 0.0.2** — separates accumulation from `Result<T, TErr>` and removes the `IReadOnlyList<TErr>` error channel. |
| Accept `IAsyncEnumerable<T>` | Bounded-concurrency and streaming semantics are a separate design decision; the existing helpers stay `IEnumerable<T>` based |

## Consequences

- Validation is now reachable from a bare sequence on both the synchronous and asynchronous tracks.
- Faults, cancellation and validation errors stay distinct.
- The public API baseline grows; the package version/documentation policy is unchanged by this decision.
