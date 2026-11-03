# ADR 0013 — Three-state tags for `Result`, `Either` and `ParseFailure`

- **Status:** Accepted
- **Date:** 2026-11-03
- **Supersedes:** [ADR 0001](0001-core-design-decisions.md) D1 consequence for value-type `TErr` at `default(Result<,>)`;
  implicit `default(Either<,>)` as `Right` in tests and docs
- **Related:** [ADR 0002](0002-api-gap-remediation.md) D20

## Context

`Result<TOk,TErr>` stored its track in a single `bool`. `default(Result<,>)` therefore looked like `Err`, and when
`TErr` was a non-nullable value type, `default(TErr)` made the default result indistinguishable from a legitimate
`Err(0)`.

`Either<TLeft,TRight>` stored left vs right in a single `bool`. `default(Either<,>)` behaved like `IsRight == true`,
which is not a valid either and is misleading for reference-type right payloads.

`ParseFailure` was a `readonly record struct` whose `default` exposed null `Code`/`Message` without faulting.

## Decision

Use a `byte` tag (`0` = uninitialized, `1`/`2` = the two active states) on `Result`, `Either` and `ParseFailure`.
Do not add a public `IsInitialized` property; uninitialized state is observed only through the existing members.

`Option<T>` is unchanged: `default` remains `None`.

## Consequences

- Breaking change for callers that relied on `default(Either<,>)` as `Right` or on `default(Result<string,int>)` as
  `Err(0)`.
- Hash codes and equality treat uninitialized values as a distinct state.
- `Either.BindLeft`/`BindRight` complete the synchronous either surface without async counterparts.
