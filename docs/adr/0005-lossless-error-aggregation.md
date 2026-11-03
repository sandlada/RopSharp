# ADR 0005 — Lossless `RopError` aggregation

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0002](0002-api-gap-remediation.md) (D21, error classification),
  [ADR 0004](0004-validation-traversal.md)

## Context

Aggregating APIs (`Rop.Validate`, `Rop.SequenceValidation`, `Rop.TraverseValidation` and their async variants)
return `Result<T, IReadOnlyList<TErr>>`. Callers usually collapse that track with
`ToSingleError(RopError.Combine)` before handing it to a transport layer.

`RopError.Combine` produced a correct *summary* — code `E_MULTIPLE`, joined messages, distinct codes in `Details` —
but discarded the individual errors. A caller that needed to map each failure (for example to field-level validation
messages) had to keep both the list and the combined value alive, and `ToSingleError` made that impossible after the
collapse. The XML docs described the result as a "combined" error, which overpromised the structure.

## Decisions

### D35 — `InnerErrors` is non-null and empty for atomic errors

`RopError.InnerErrors` is an `IReadOnlyList<RopError>` that is never `null`. Errors created by `Create` or
`FromException` expose an empty list.

**Consequence:** callers can walk the property without null checks, and `Count == 0` is the simple "atomic" test.

### D36 — `Combine` preserves the supplied errors in order and does not flatten

The composite keeps exactly the elements passed to `Combine`, in order. A nested aggregate stays nested, so a caller
that combines groups sees the grouping instead of a flat bag of leaves.

A single-element call remains an identity operation: it returns that error unchanged, including its own
`InnerErrors`, so the result may not carry the `E_MULTIPLE` code.

**Consequence:** no information is lost by collapsing, and `RopError.Combine` remains associative in structure
(`Combine([a, b, c])` and `Combine([Combine([a, b]), c])` both expose all three leaves, with the second keeping the
inner grouping).

### D37 — `With*` preserves `InnerErrors`, and the aggregate snapshots its inputs

`WithCode`, `WithMessage`, `WithDetails` and `WithKind` carry the existing `InnerErrors` list to the new instance, so
re-tagging an aggregate does not strip its leaves. `Combine` copies the input list into a read-only wrapper; mutating
the caller's list afterwards cannot change a previously created aggregate.

**Consequence:** aggregates are immutable values like every other `RopError`, even when the source collection is not.

### D38 — Equality includes `InnerErrors` and still ignores `Cause`

- **Revised:** 2026-11-03

Equality compares `Kind`, `Code`, `Message`, `Details` and `InnerErrors` in order, recursively. `Cause` remains
excluded so stack-trace differences do not make two logically equal errors unequal. Comparison and hashing use an
explicit stack rather than recursive calls, and both terminate when a cycle appears on the current path.

`RopError.Flatten()` returns every leaf in the order implied by nested `Combine` calls; aggregate nodes themselves are
omitted.

**Consequence:** two aggregates that collapse to the same summary but differ in their child lists no longer compare
equal. Callers that only need the collapsed summary can still branch on `Code`, `Message` and `Details`; callers that
need every leaf use `Flatten()`.

### D39 — The addition is additive

The public constructor shape is unchanged (the class has no public constructor). `InnerErrors` is a new get-only
property appended to `PublicAPI.Unshipped.txt`; no signature changes, so the release stays source- and
binary-compatible.

**Consequence:** consumers can opt into the structure without a migration.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Flatten nested aggregates inside `Combine` | Loses the grouping a caller deliberately created and silently rewrites their structure |
| Include `InnerErrors` in `Equals`/`GetHashCode` | Initially rejected to preserve summary-only equality; **revised in D38 (2026-11-03)** because duplicate aggregates with the same summary but different leaves were indistinguishable |
| Introduce an `AggregateError : RopError` subtype | `RopError` is `sealed`; a subtype would break exhaustive handling and add a second error concept |
| Return a tuple `(RopError Summary, IReadOnlyList<RopError> Errors)` | Changes every call site and forces callers to thread two values through the pipeline |
| Keep the leaf list by reference | A mutable caller-owned list would make an otherwise immutable error change over time |
| Add a separate `RopError.Aggregate` factory | `Combine` already owns the collapse; a second entry point would be an alias |

## Consequences

- Collapsing a validation track no longer discards the individual failures.
- Aggregates remain immutable and cheap to compare.
- The public API baseline grows by one property.
