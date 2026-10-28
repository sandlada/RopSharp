# ADR 0010 — Concurrency, streaming and arity stay caller-owned

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md) (D6, D9),
  [ADR 0002](0002-api-gap-remediation.md) (D18, D23), [ADR 0006](0006-async-boundaries.md),
  [ADR 0007](0007-diagnostics.md)

## Context

The 1.1 review left three larger design topics open. Each would add a new concern rather than a missing behaviour,
so they were evaluated together against the library's design rules:

1. **Higher-arity `Combine` / `Validate`** — the family stops at three inputs.
2. **Bounded concurrency** — async aggregation is either sequential (`SequenceAsync`, `TraverseAsync`) or
   `Task.WhenAll` (`CombineAsync`, validation), with no throttling helper.
3. **`IAsyncEnumerable<T>` support** — traversal accepts `IEnumerable<T>` only.

## Decision matrix

| Topic | Decision | Why |
| --- | --- | --- |
| Higher-arity `Combine`/`Validate` (4+) | Rejected | Nested calls and `SequenceValidation`/`TraverseValidation` already express it; each extra arity would add four public signatures (sync/async, `Result`/`Option`) and a wider merger delegate than is readable |
| Bounded concurrency (`maxDegreeOfParallelism`) | Rejected for the core | Scheduling policy is caller-owned (D9); the BCL already offers `Parallel.ForEachAsync` and `Chunk`/`Task.WhenAll`, and adding a scheduler would introduce non-deterministic failure ordering and a second cancellation model |
| `IAsyncEnumerable<T>` traversal | Rejected for the core | Streaming is a consumption model, not a result model; a `await foreach` loop plus `Rop.SequenceAsync` is already the composition, and a second enumeration surface would double the traversal API |

### D55 — Arity stops at three

`Combine`, `Zip` and `Validate` keep their 2- and 3-input shapes. Larger aggregations use a collection:

```csharp
// instead of Combine(a, b, c, d, merger)
Result<Order, IReadOnlyList<RopError>> order =
    Rop.Validate(CheckCustomer(c), CheckAmount(a), CheckStock(s), (customer, amount, stock) => ...);
```

or, when the inputs already live in a collection, `SequenceValidation`/`TraverseValidation`. The same guidance is
added to the README API map section.

**Consequence:** no combinatorial growth of signatures, and the 3-input form stays readable.

### D56 — Concurrency policy stays at the call site

The async helpers keep two explicit modes: sequential (`SequenceAsync`, `TraverseAsync`) and await-all
(`CombineAsync`, `ValidateAsync`, validation traversal). The library never throttles, never starts tasks on the
caller's behalf, and never creates a `CancellationTokenSource` (D9).

Callers that need bounded fan-out compose it with the BCL:

```csharp
var pending = source.Select(async item => await ProcessAsync(item, ct));
Result<IReadOnlyList<Out>, RopError> result = await Rop.SequenceAsync(pending, ct);
```

or, for order-independent work, `Parallel.ForEachAsync` with a collected `ConcurrentBag`.

**Consequence:** failure ordering stays deterministic for the sequential helpers, and callers keep full control of
scheduling, retries and back-off.

### D57 — `IAsyncEnumerable<T>` is composed, not wrapped

Async streams are consumed at the boundary and materialized into the existing helpers:

```csharp
var mapped = new List<Result<Order, RopError>>();
await foreach (var row in rows.WithCancellation(ct))
{
    mapped.Add(ParseRow(row));
}

Result<IReadOnlyList<Order>, RopError> result = await Rop.SequenceAsync(mapped, ct);
```

Adding `TraverseAsync(IAsyncEnumerable<T>, ...)` overloads would duplicate the traversal contract (short-circuit,
ordering, cancellation) on a second source shape without removing the loop.

**Consequence:** the core stays zero-dependency and `IEnumerable<T>`-based; streaming operators remain available to
callers through the BCL or a dedicated package.

### D58 — Re-evaluation trigger

These decisions are revisited if any of the following becomes true:

- a consumer demonstrates an arity-4+ `Validate` call site that cannot be expressed readably as a record or
  collection;
- bounded concurrency is requested with a concrete ordering contract that the BCL composition cannot meet;
- the library gains an async-stream dependency or the target frameworks drop `System.Linq.Async` compatibility
  concerns.

**Consequence:** the scope is explicit and time-bound rather than an implicit omission.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Add arity 4–6 `Combine`/`Validate` | 12+ new signatures, wide delegate types, and no collection story for arity 7 |
| Add `TraverseAsync(..., int maxConcurrency, ...)` | Non-deterministic failure ordering or a costly deterministic implementation; duplicates BCL scheduling |
| Add `IAsyncEnumerable` overloads for every traversal helper | Doubles the API surface for a composition that is already one loop |
| Put all three behind a `Rop.Concurrency`/`Rop.Streams` namespace | New namespaces and dependency-shaped concerns in a zero-dependency core |
| Leave the topics undocumented | The review would resurface them without context |

## Consequences

- No new public API from this evaluation.
- Concurrency and streaming remain explicit caller decisions, consistent with D9.
- The rationale and the re-evaluation trigger are recorded, so future requests start from a decision instead of a
  blank page.
