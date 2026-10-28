# ADR 0009 — Null elements in the Option collection helpers

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md) (D1, D6), [ADR 0002](0002-api-gap-remediation.md) (D15)

## Context

`Option<T>` never contains `null`: `Opt.Some(null)` throws and `Opt.FromNullable` is the documented way to lift a
nullable value. The collection helpers in `OptionEnumerableExtensions` were mostly consistent with that rule because
they delegated to `Opt.Some`, but two problems remained:

- the resulting exception message talked about `Some` and a `value` parameter, which hides *where* the null came
  from (the sequence, the dictionary, or a mapper), and
- `Choose(source, mapper)` materialized whatever the mapper returned, so a `null` projection silently became a list
  entry — the only helper that could still leak `null` to a caller.

## Decisions

### D51 — A null element is a programming error in every collection helper

`TryFirst`, `TryLast` (with and without a predicate), `TryFind` and `Choose(source, mapper)` throw
`ArgumentNullException` when the value they would wrap or materialize is `null`.

The parameter name identifies the source of the null:

- sequence or dictionary elements use `source` (matching `RopError.Combine`, which reports a null element against
  `errors`);
- a mapped value uses `mapper`.

**Consequence:** the failure points at the argument the caller can fix, and `Option` keeps its "never null" invariant
on the way out as well as on the way in.

### D52 — The message states the rule and the alternative

The messages are fixed constants:

- `"The sequence contained null. An Option never contains null; remove the null element or use Opt.FromNullable."`
- `"The mapper returned null. An Option never contains null; return Opt.None or use Opt.FromNullable instead."`

**Consequence:** a log line explains both the rule and the fix without the caller having to consult the docs.

### D53 — The policy is documented at the type, the members and the README

The class remarks describe the rule once; each affected member documents the element-null case in its
`<exception>` tag; the README nullability section repeats it. `Choose(source)` is unchanged because an
`Option<T>` cannot contain a null in the first place.

**Consequence:** the behavior is discoverable from IntelliSense and from the single nullability section instead of
being an implementation detail.

### D54 — Null-projection is rejected, not skipped or coerced

`Choose(source, mapper)` throws instead of dropping null projections or converting them to `None`. The helper is a
projection over already-present options; silently changing the element count would hide a mapper bug and make the
result length depend on data.

**Consequence:** callers that expect nulls must model them explicitly (`Opt.FromNullable` inside the mapper, then
`Choose()`), which keeps the loss visible.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Treat null elements as `None` and skip them | Contradicts "`Option` never contains null" and silently changes lengths |
| Let `Opt.Some` throw with its generic message | The exception cannot name the sequence, dictionary or mapper |
| Throw `InvalidOperationException` | The input data is at fault; every other null-input guard in the library throws `ArgumentNullException` |
| Add a `where T : notnull` constraint | The helpers must also work for value types, and a constraint would not express "the runtime value is non-null" for reference types |
| Return `Option<T?>` | Would push nullability into every caller and break the core invariant |
| Add a `SkipNulls`/`WhereNotNull` helper | A filtering helper is a different API; `Opt.FromNullable` already covers the lifting case |

## Consequences

- The "never null" invariant is enforced on both directions of the collection boundary.
- Null-data bugs surface at the helper that observed them, with an actionable message.
- No public signature changes; behavior changes only for sequences, dictionary values or projections that were
  already violating the `Option` contract.
