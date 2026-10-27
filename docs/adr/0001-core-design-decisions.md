# ADR 0001 — Core design decisions for Sandlada.RopSharp 1.0

- **Status:** Accepted
- **Date:** 2026-10-27
- **Supersedes:** none
- **Related:** [ADR 0002](0002-api-gap-remediation.md)

## Context

The repository started from an early draft specification describing a symmetric, C#-only, functional-first ROP
library targeting `net10.0` and `net11.0`. The draft contained several signature-level assumptions that the C#
type system cannot express, and it left some production behaviours (argument validation, nullability, async
cancellation, collection materialization) to implementation time. The draft folder was removed after its still-
valid decisions were folded into this ADR and ADR 0002.

This ADR records the decisions that shape the public surface so future changes can be evaluated against them.

## Decisions

### D1 — Core types are readonly structs with value equality

`Result<TOk, TErr>` and `Option<T>` are `readonly struct`s. They are immutable, allocation-free as values, have
value-based equality and `GetHashCode`, and implement `IEquatable<T>` plus `==`/`!=`.

**Consequence:** `default(Option<T>)` is contractually `None`; `default(Result<,>)` is undefined and must not be
relied upon. This asymmetry is documented and intentional: an option has a natural empty value, a result does not.

### D2 — Expected failures are values; exceptions are boundary-only

No combinator throws for an expected failure. `Rop.Try`/`Rop.TryAsync` are the single place where exceptions become
`Err` values, and `OperationCanceledException` is always rethrown. `ArgumentNullException` is reserved for null
arguments passed to non-null parameters (programming errors). `InvalidOperationException` is used by test-only
`OrThrow`/`*OrThrow` helpers and by internal invariant violations.

**Consequence:** `try/catch` inside a pipeline is an anti-pattern; lift the boundary instead.

### D3 — Explicit construction rejects null; mapping may carry it

`Rop.Ok(null)` and `Opt.Some(null)` throw. A mapper passed to `MapOk` may return `null` for a nullable reference
type, producing `Result<string?, RopError>`; `RequireNotNull` converts that into an error. Without this split,
`RequireNotNull` would be unreachable (the factory would already have thrown). `Option<T>` never contains null;
nullable mappings use `BindSomeNullable`/`BindSomeNullableValue`.

**Consequence:** nullable annotations are meaningful on the success track and are preserved through the pipeline.

### D4 — Structured error model

`RopError` carries `Code` (stable, for branching), `Message` (display only), optional `Details` and optional
`Cause` (set only by `FromException`). Equality ignores `Cause` so stack-trace noise does not destabilize
comparisons. `RopError.Combine` collapses a non-empty list into `E_MULTIPLE`.

**Consequence:** domain code branches on `Code`; no `Data` bag, no reflection-based error chaining (AOT safety).

### D5 — Short-circuit and aggregating models are separate

`BindOk` and the `Combine`/`Zip`/`Sequence`/`Traverse` family short-circuit (first error wins, later functions are
not invoked). `Validate`/`SequenceValidation` aggregate every error in argument order. Conversion is explicit via
`ToValidation`/`ToSingleError`. No API silently switches models.

**Consequence:** callers pick the model by name; no configuration flags, no hidden behaviour.

### D6 — One behaviour, one name; symmetry over familiarity

The surface uses symmetric names (`MapOk`/`MapErr`, `BindOk`/`BindErr`, `TeeOk`/`TeeErr`, `MapSome`/…), never
short aliases (`Map`, `Bind`, `Tee`, `OrElse`, `AndThen`, `FlatMap`, `Tap`, `Recover`, `As`, `Ignore`). There are no
implicit conversions, no `Deconstruct`, and no operators: the active track must remain visible at the call site.

**Consequence:** migration from other libraries requires renaming, but the track is never silently swallowed.

### D7 — C# cannot express constraint-only overloads

Two overloads that differ only by generic constraints are illegal (CS0111) and unusable across extension classes
(CS0452/CS0453). Nullable value-type helpers therefore carry a `Value` suffix: `FromNullableValue`,
`ToNullableValue`, `BindSomeNullableValue`. Reference-type overloads keep the plain name.

**Consequence:** the naming is slightly asymmetric, but every call compiles without casts or type arguments.

### D8 — `OverloadResolutionPriority` resolves async mapper ambiguity

`x => FetchAsync(x)` is convertible to both `Func<T, U>` (with `U = Task<X>`) and `Func<T, Task<U>>`. Priority
attributes (sync 0, async 1, cancellation-aware async 2) make the intended overload win without requiring callers
to write casts.

**Consequence:** the async surface stays overload-based instead of growing `...Async` names for mapper shapes.

### D9 — Cancellation is not a domain failure

Async combinators accept an optional trailing `CancellationToken`, waiting with `WaitAsync`. Cancellation produces
`OperationCanceledException` and cancels the returned task; it is never mapped to `Err`/`None`. Cancellation-aware
overloads forward the same token and never create their own `CancellationTokenSource`. Library awaits use
`ConfigureAwait(false)`.

**Consequence:** callers distinguish "business failure" from "cancelled" by task state, not by inspecting errors.

### D10 — Collections are materialized once

Sequencing/validation/traversal/partition APIs enumerate their source exactly once, immediately, and return
`IReadOnlyList<T>` backed by a read-only wrapper (`Array.Empty<T>` when empty). Public APIs never return null
collections.

**Consequence:** callers must not rely on deferred execution; this is documented.

### D11 — AOT/trim safety and no hot-path reflection

The library uses no reflection, no dynamic code, no serialization hooks and no `MakeGenericType`. Analyzers
(`IsAotCompatible`, trim, AOT, single-file) are enabled with warnings as errors.

**Consequence:** features that would require reflection (error chaining, dynamic metadata, analyzers) are rejected.

### D12 — Test toolchain is TUnit on Microsoft.Testing.Platform

Tests use TUnit exclusively. `global.json` selects the MTP `dotnet test` runner. Filtering uses
`--treenode-filter`; the legacy VSTest `--filter FullyQualifiedName~...` syntax does not exist in this mode.
Tests target both `net10.0` and `net11.0`.

**Consequence:** the documented single-test command in `AGENTS.md` reflects MTP syntax.

### D13 — The public API is locked with a shipped baseline

`Microsoft.CodeAnalysis.PublicApiAnalyzers` runs with warnings as errors. Every public symbol is listed in
`PublicAPI.Shipped.txt`; new members must be added to `PublicAPI.Unshipped.txt` explicitly. Accidental removals or
signature changes fail the build.

**Consequence:** public API changes are deliberate and reviewable; the 1.0 surface is frozen.

### D14 — Strict analyzers on the library, justified suppressions only

The library builds with `AnalysisMode=All` and warnings as errors. Suppressions are local and carry a
justification: `CA1031` on `Rop.Try`/`Rop.TryAsync` (converting any non-cancellation exception is the boundary's
contract) and `CA1716` on `Option<T>` (the established functional name; the API is C#-only). `CA1508` was fixed by
restructuring the aggregation helpers, and `CA1715` by renaming the `U` type parameter to `TOut`. The test project
keeps the default analysis level so test naming conventions do not produce noise.

**Consequence:** the library has no unexplained analyzer suppressions; new warnings fail the build.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Classes instead of structs | Allocation on every combinator; equality semantics need overriding anyway |
| `Result<TOk>` with implicit `RopError` default | C# has no type-parameter defaults; factories provide the default instead |
| Single aggregating model (always collect errors) | Short-circuiting is the common production case and must stay allocation-light |
| Implicit `T → Result` / `Result → T` conversions | Hides the track; causes surprising overload resolution |
| `OrElse`/`Bind` naming | Breaks the documented symmetry and encourages mixing tracks |
| Mapping cancellation to `Err` | Blurs domain failure with control flow; breaks .NET task semantics |
| `ValueTask` overloads | Doubles the surface for no measured benefit in this release |

## Consequences

- The public API is intentionally narrow and symmetric; new members must map onto the existing naming matrix or be
  rejected.
- Some call sites need explicit type arguments (for example when the compiler cannot infer `TErr`); the
  single-type-parameter factories mitigate the common cases.
- Nullable annotations are part of the contract and are preserved across combinators.

## Future work

1. Benchmarks (BenchmarkDotNet) for hot combinators and aggregation paths; publish allocation numbers.
2. SourceLink and repository metadata once the package has a canonical git remote.
3. Consider `ValueTask` overloads only with measured demand.
4. Revisit `net12.0` support after its release; never back-port below `net10.0`.
