# ADR 0011 — Custom error types for convenience and boundary APIs

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md), [ADR 0002](0002-api-gap-remediation.md),
  [ADR 0007](0007-diagnostics.md)

## Context

`Result<TOk, TErr>` has always been generic over `TErr`, and every receiver-based combinator, aggregation
helper and LINQ operator is already generic. `RopError` is the recommended production default, not a
constraint: a caller can already build `Result<Order, MyError>` and chain `MapOk`/`BindOk`/`Ensure`/`Match`
without touching `RopError`.

A set of convenience and boundary APIs nevertheless fixed `TErr` to `RopError`:

- construction and lifting: `Rop.FromNullable`, `Rop.FromNullableValue`, `Rop.SuccessIf`, `Rop.FailureIf`,
  `RequireNotNull`, `Either.ToResult`;
- aggregation collapse: the parameterless `ToSingleError()`;
- exception boundaries: `Rop.Try`, `Rop.TryAsync`, `Rop.TrySwitch`, `Rop.TrySwitchAsync`;
- parsing: `Rop.Parse`.

A caller whose error system is a domain type (`MyError`, a record, an enum or a plain string) could use the
core but hit a wall at these helpers and had to write a local adapter layer. This ADR records how each API
was generalized, which parts could not be generalized mechanically, and the trade-offs.

## Decisions

### D25 — Generic overloads for pure factories; the `RopError` overloads stay

The following APIs only used `RopError` as the type of a factory result, so they gained a `TErr` overload
without changing semantics:

```csharp
Rop.FromNullable<TOk, TErr>(TOk? value, Func<TErr> errorFactory) where TOk : class
Rop.FromNullableValue<TOk, TErr>(TOk? value, Func<TErr> errorFactory) where TOk : struct
Rop.SuccessIf<T, TErr>(bool condition, T value, TErr error)
Rop.SuccessIf<T, TErr>(bool condition, T value, Func<TErr> errorFactory)
Rop.FailureIf<T, TErr>(bool condition, T value, TErr error)
Rop.FailureIf<T, TErr>(bool condition, T value, Func<TErr> errorFactory)
ResultExtensions.RequireNotNull<T, TErr>(this Result<T?, TErr> result, Func<TErr> errorFactory)
Either<TLeft,TRight>.ToResult<TErr>(Func<TLeft, TErr> errorFactory)
ResultValidationExtensions.ToSingleError<T, TErr>(this Result<T, IReadOnlyList<TErr>> result)
```

The factories are still evaluated only on the failing branch. The existing `RopError` overloads are
unchanged and remain the preferred overload when the argument is a `RopError`, because C# prefers a
non-generic method over a generic one with identical parameter types.

### D26 — Exception boundaries take an explicit null-result factory

```csharp
Rop.Try<T, TErr>(Func<T> func, Func<Exception, TErr> errorFactory, Func<TErr> nullResultErrorFactory)
Rop.Try<TErr>(Action action, Func<Exception, TErr> errorFactory)
Rop.TryAsync<T, TErr>(Func<Task<T>>, Func<Exception, TErr>, Func<TErr>, CancellationToken = default)
Rop.TryAsync<TErr>(Func<Task>, Func<Exception, TErr>, CancellationToken = default)
Rop.TrySwitch<T, TOut, TErr>(Func<T,TOut>, Func<Exception, TErr>, Func<TErr>)
Rop.TrySwitchAsync<T, TOut, TErr>(Func<T,Task<TOut>>, Func<Exception, TErr>, Func<TErr>)
```

The `RopError` boundary synthesizes `E_NULL_RESULT` when the function returns `null`. An arbitrary `TErr`
cannot be synthesized by the library, so a two-argument generic overload would silently drop that contract.
The null-result factory is therefore required for value-producing boundaries and evaluated only when the
function actually returns `null`. Action boundaries have no payload and need no null-result factory.

The existing `RopError` overloads, including the `(func, string code)` convenience form, are retained:
`code` is a `RopError` concept and is not meaningful for an arbitrary error type.

The boundary rules are unchanged: `OperationCanceledException` is always rethrown, a null task faults with
`InvalidOperationException`, cancellation is checked before invoking the function and while awaiting, and a
factory that returns `null` faults with `InvalidOperationException`.

### D27 — Parsing exposes the complete failure payload

```csharp
Rop.Parse<T, TErr>(string? value, Func<ParseFailure, TErr> errorFactory, string code = "E_PARSE")
```

`ParseFailure` carries `Code`, `Message`, `Input` (the raw caller text), `TypeName` and `Details` (the
bounded, escaped `input=<quoted>` rendering). The factory receives the whole payload, so a custom error keeps
the same diagnostics as the `RopError` overload: the bounded 64-code-point echo, control-character escaping,
surrogate-safe truncation and the null-versus-empty distinction. `Input` is deliberately raw for callers that
need to re-process it; `Details` is the log-safe form.

### D28 — Aggregation stays function-based

`ToSingleError<T, TErr>()` takes the first error. A custom collapse strategy already exists as
`ToSingleError(combine)`, so no policy object is needed. `RopError.Combine`, `InnerErrors`, `Kind` and the
`With*` methods remain `RopError`-specific: they are the structured error system itself, not a generic
aggregation mechanism. No `IErrorPolicy<TErr>` or higher-kinded abstraction is introduced; it would add
allocation and indirection to hot paths and conflict with the AOT-friendly, dependency-free design.

### D29 — Value-type error types are supported with documented caveats

`TErr` is unconstrained, so `record struct` and enum error types work. The consequences are documented in the
README rather than hidden:

- `Rop.Err` rejects `null` for reference error types; a value-type error can be `default`, and a
  `default(Result<TOk,TErr>)` reports `IsErr` with a default error even though `default(Result<,>)` is
  documented as undefined.
- `Result<,>` equality uses `EqualityComparer<TErr>.Default`, so a record or a value type gets value equality
  while a plain class defaults to reference equality.
- `TErr` is inlined in the `Result<,>` struct; a reference type or a small struct keeps the value cheap to
  copy through combinators.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Keep `RopError`-only helpers and require adapters | The core was already generic; forcing per-project adapters is unnecessary friction |
| Add `Try<T,TErr>(func, onException)` and throw on a null result | Changes the "expected failures are values" contract and makes null handling implicit |
| Pass `TErr nullResultError` eagerly | Allocates an error on every successful call |
| Introduce `IErrorPolicy<TErr>` / HKT | Allocation, indirection and AOT cost for no demonstrated need |
| Remove the `RopError` overloads | Breaking change; `RopError` remains the recommended production default |
| Generic `Parse` returning only a message | Loses `Details`, `Input` and `TypeName`; diagnostics degrade |

## Consequences

- The public surface grows by 30 symbols, all additive, recorded in `PublicAPI.Unshipped.txt`.
- Existing call sites keep binding to the `RopError` overloads; no behavior changes on that path.
- `RopError` remains the only type with `Code`/`Kind`/`InnerErrors`; a custom error type must model its own
  structure and aggregation policy.
- 57 new TUnit tests (114 runs across `net10.0` and `net11.0`) cover class and struct error types, boundaries,
  parse diagnostics and the unchanged `RopError` overloads.
- The package version stays `0.0.1`; this is part of the initial, unpublished release.
