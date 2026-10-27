# ADR 0002 — API gap remediation for 1.1

- **Status:** Accepted
- **Date:** 2026-10-27
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md)

## Context

A comparison against ErrorOr, CSharpFunctionalExtensions, Danom, Ardalis.Result, FluentResults and
LanguageExt identified capabilities that those libraries expose and RopSharp did not. Some were already
rejected deliberately by ADR 0001 (implicit conversions, aliases, operators, a `Data` bag, reflection-based
serialization, analyzers). Others were genuine gaps: collection helpers for options, conditional construction,
action terminals, asynchronous aggregation, string parsing, asynchronous LINQ and a general two-branch union.

This ADR records a decision for every audited gap so that no item is silently dropped. Implemented items are
covered by TUnit tests and the public API baseline; retained rejections carry a concrete rationale.

## Decisions

### D15 — Option collection helpers are part of the optional boundary

`OptionEnumerableExtensions` adds `TryFirst`, `TryLast`, `TryFind`, `Choose`, `Execute` and `ExecuteNoValue`.
They do not introduce a second collection abstraction: probes return `Option<T>`, `Choose` materializes an
`IReadOnlyList<T>` exactly once, and the side-effect helpers run on exactly one track.

**Consequence:** callers no longer need `FirstOrDefault`/`null` or manual `foreach` filtering at the optional
boundary.

### D16 — Conditional construction is explicit; `Result.Of` remains rejected

`Rop.SuccessIf` and `Rop.FailureIf` (value and lazy-factory overloads) cover conditional construction.
`Result.Of` is not added: `Rop.Ok` already constructs a pure success value and `Rop.Try`/`Rop.TryAsync` are the
only exception boundaries. An `Of` alias would duplicate both and violate D6.

**Consequence:** the construction vocabulary stays small: `Ok`, `Err`, `Try`, `TryAsync`, `FromNullable`,
`SuccessIf`, `FailureIf`.

### D17 — Action terminals and rejection rules are first-class

`ResultTerminalExtensions` adds `Switch`, `SwitchAsync`, `MatchFirst` and `SwitchFirst`.
`ResultEnsureExtensions` adds `FailIf` (predicate true means failure) and `ThenEnsure` (run a result-returning
check and keep the original success value). `Ensure` keeps its existing predicate-false-means-failure meaning.

**Consequence:** callers can log/exit on both tracks without projecting a value, and rejection rules read in the
natural direction. `FailIf` is not an alias: its predicate polarity is the opposite of `Ensure`.

### D18 — Asynchronous aggregation mirrors the synchronous model

`Rop.CombineAsync`, `ValidateAsync`, `ZipAsync` and `PartitionAsync` await every supplied task, check cancellation
before waiting (so already-completed tasks cannot bypass a cancelled token), and then apply the same short-circuit
or aggregating semantics as their synchronous counterparts. The merger is never invoked when an input failed; the
first error in argument order wins for `CombineAsync`/`ZipAsync`.

**Consequence:** asynchronous pipelines no longer need hand-written `Task.WhenAll` plus per-result branching.

### D19 — Asynchronous LINQ is supported for `Task<Result<,>>`

`ResultLinqExtensions` adds `Select` and `SelectMany` on `Task<Result<TOk,TErr>>`, with both synchronous and
task-returning collection selectors. Query syntax therefore works across awaited results without a custom
builder.

**Consequence:** async queries stay readable; cancellation remains an outer concern because query syntax has no
token slot.

### D20 — `Either<TLeft,TRight>` is the general two-branch union

`Either<TLeft,TRight>` provides `IsLeft`/`IsRight`, non-throwing probes, `Match`, `MapLeft`, `MapRight`, `Swap`,
`ToResult` and value equality. It is intentionally minimal and AOT-safe.

A three-or-more-way `Choice` type is retained as a rejection for this release: callers can compose nested
`Either` values or model the decision with a domain type. A full union family would multiply the surface without a
demonstrated production use case.

**Consequence:** both branches are ordinary values when success/failure would be a misleading naming.

### D21 — Error classification is a small enum; metadata stays out

`RopErrorKind` (`Failure`, `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Unexpected`) is
carried by `RopError.Kind` and can be set through factory overloads or `WithKind`. Equality and hashing include
the kind. `RopError.Combine` keeps a uniform kind and falls back to `Failure` for mixed inputs.

A `Data`/metadata dictionary remains rejected (D4/D11): it would add allocation, serialization and AOT concerns
to the core type and is better represented by `Details` or a domain-specific error type.

**Consequence:** transport layers can map categories without parsing `Code`; precise branching still uses `Code`.

### D22 — Parsing helpers use `IParsable<T>`

`Rop.Parse<T>` and `Rop.TryParse<T>` use invariant culture and the static abstract `IParsable<T>` contract.

**Consequence:** no reflection, no per-type parsing table, and AOT safety is preserved.

### D23 — ASP.NET Core integration, analyzers, FluentAssertions and recording remain out of the core package

These are retained as rejections for the core 1.x package:

- **ASP.NET Core integration:** the core is transport-neutral and dependency-free. A separate optional package can
  map `RopErrorKind` to status codes; `RopErrorKind` now makes that mapping a short, explicit function rather than
  a parsing heuristic.
- **Roslyn analyzers:** ADR 0001 D11 already rejected analyzers for the core release; the compiler warnings,
  `AnalysisMode=All` and PublicApiAnalyzers already enforce the documented rules. A standalone analyzer package
  needs its own distribution and versioning policy.
- **FluentAssertions helpers:** the repository is TUnit-only by policy. Adding a third-party assertion dependency
  for consumers conflicts with that test-toolchain decision; consumers can assert on `IsOk`/`IsErr`/`TryGet*` or
  `Match`.
- **Recording/serialization (`IRecordingSerializer` style):** D11 rejects serialization hooks and reflection.
  Consumers can serialize through `Match` with their chosen serializer.

**Consequence:** the core stays zero-dependency, AOT-safe and test-toolchain-neutral. The rejected items are
explicitly documented rather than forgotten.

### D24 — Transaction scopes remain caller-owned

`WithTransactionScope` is retained as a rejection. `System.Transactions` introduces ambient state and
sync-over-async hazards that conflict with the explicit ROP boundary model (D2/D9). Callers should open their
transaction around a `Rop.TryAsync` boundary and decide commit/rollback from the returned result.

**Consequence:** the library never owns ambient state; transaction policy stays visible at the call site.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Add every competitor API verbatim | Breaks symmetry, adds aliases and hides the active track |
| One `ErrorOr`-style always-aggregating result | Short-circuiting is the common production path and must stay allocation-light |
| `RopError.Data` dictionary | Allocation, AOT and equality stability costs without a demonstrated need |
| `Result.Of` alias | Duplicates `Ok`/`Try` and violates D6 |
| Three-way `Choice` | Nested `Either` or a domain type covers the use case with less surface |
| ASP.NET mapping in the core | Couples a transport-neutral library to a web framework |
| FluentAssertions in the core | Conflicts with the TUnit-only test policy |

## Consequences

- The 1.1 surface is additive except for `RopError` equality, which now includes `Kind`; existing errors default
  to `Failure`, so existing values keep their semantics.
- New public members are folded into `PublicAPI.Shipped.txt` as part of the 1.1 release baseline (252 symbols).
- The rejected items are now discoverable in documentation; future work can revisit them through a new ADR.
