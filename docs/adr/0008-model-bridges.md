# ADR 0008 — Complete the model-bridging triangle

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md) (D5, D6),
  [ADR 0002](0002-api-gap-remediation.md) (D20)

## Context

The library exposes three models — `Result<TOk,TErr>`, `Option<T>` and `Either<TLeft,TRight>` — and ADR 0001 (D5)
requires every crossing to be explicit. The bridges were asymmetric:

- `Result` ↔ `Option` was complete (`RequireSome` and `ToOption`).
- `Either` → `Result` existed (`Either.ToResult`).
- `Result` → `Either`, `Option` → `Either` and `Either` → `Option` did not exist.

Callers that modelled a two-branch domain value had to hand-write `Match` to reach `Either`, and callers that only
needed presence had to project `Either` through `ToResult` plus `ToOption`. Both workarounds lose the naming
guarantee that the library otherwise provides.

## Decisions

### D47 — `Result.ToEither` maps success to `Right` and failure to `Left`

`Result<TOk,TErr>.ToEither()` returns `Either<TErr,TOk>`. The right branch always carries the value; the left branch
always carries the reason. `ToResult` recovers the original result with an identity error factory, so the crossing is
lossless in both directions.

A success carrying `null` (possible after a nullable mapping) throws `ArgumentNullException` from `Either.Right`,
exactly as `Rop.Ok(null)` would; the bridge never invents a left value.

**Consequence:** a either-shaped API can consume results directly, and the value/reason orientation is consistent
with `Either.ToResult`.

### D48 — `Option.ToEither` needs an explicit left value

`Option<T>.ToEither(TLeft left)` and `Option<T>.ToEither(Func<TLeft> leftFactory)` return `Either<TLeft,T>`:
`Some` becomes `Right`, `None` becomes `Left`.

The value overload validates the left value eagerly (even for `Some`), matching `RequireSome(error)`. The factory
overload is evaluated only for `None` and rejects a `null` result with `InvalidOperationException`, matching
`Rop.Materialize`.

**Consequence:** absence gains a reason at the boundary, and the cost of building that reason stays under the
caller's control.

### D49 — `Either.ToOption` is explicitly lossy

`Either<TLeft,TRight>.ToOption()` returns `Some` for the right branch and `None` for the left branch. The left value
is discarded and cannot be recovered; the XML docs, README and the bridge class remarks say so, and direct callers to
`ToResult` when the reason matters.

**Consequence:** the triangle closes without pretending that `Option` can carry the reason. The loss is named, not
hidden.

### D50 — No implicit conversions and no fourth bridge class

All bridges are ordinary methods or extension methods on the types they start from. No implicit operators, no
`IConvertible` implementation, no separate converter object. `EitherBridgeExtensions` holds the two "into either"
extensions; `ToOption` lives on the struct next to `ToResult`, mirroring how `OptionBridgeExtensions` owns the
Option/Result pair.

**Consequence:** the active model stays visible at the call site (ADR 0001, D6), and the bridge surface is discoverable
through the receiver.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Implicit conversions between the three models | Violates D5/D6 and hides which branch was taken |
| `Result.ToEither` that maps failure to `Right` | Inconsistent with `Either.ToResult`; the reason must not silently become the value |
| `Either.ToOption(Func<TLeft, RopError>)` | That is `ToResult(...).ToOption()`; adding it would be an alias |
| Lazy-only `Option.ToEither` | Forces closure allocation for the common constant case |
| `Option.ToEither` without validating `left` | Would throw later from `Either.Left` with a less specific call site |
| A single `Convert`/`Bridge` static class | Hides the receiver and breaks the extension-method discovery pattern |
| Extending `OptionBridgeExtensions` with `Either` members | Mixes two concept pairs in one class; the existing remarks describe it as the Option/Result bridge |

## Consequences

- Every pair among `Result`, `Option` and `Either` now has a named bridge in both directions where a meaningful one
  exists.
- Lossy conversions are explicit and documented.
- The public API baseline grows by one struct method and three extension methods (plus their declaring type).
