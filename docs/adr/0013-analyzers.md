# ADR 0013 — Contract analyzers in the package

- **Status:** Accepted
- **Date:** 2026-11-03
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md) (D11, analyzers deferred for 0.0.1),
  [ADR 0006](0006-async-boundaries.md) (D43, task-like `Try` guard)

## Context

0.0.2 adds stricter runtime and API contracts: uninitialized `default` values for `Result`/`Either`/validation
types, discarded railway values, synchronous combinators that must not receive task-returning delegates, and stable
`RopError` codes. Some of these rules are easy to violate accidentally at compile time. Shipping Roslyn analyzers in
the main NuGet package gives callers immediate feedback without a separate package.

## Decisions

### D44 — Ship `Sandlada.RopSharp.Analyzers` inside the library nupkg

The analyzer assembly targets `netstandard2.0`, references `Microsoft.CodeAnalysis.CSharp` with
`PrivateAssets="all"`, sets `EnforceExtendedAnalyzerRules` and `IsRoslynComponent`, and is packed to
`analyzers/dotnet/cs/Sandlada.RopSharp.Analyzers.dll`. It is not a runtime dependency of the library.

**Consequence:** consumers reference only `Sandlada.RopSharp`; analyzers load automatically in the compiler.

### D45 — Diagnostic IDs and severities

| ID | Severity | Rule |
| --- | --- | --- |
| `ROP0001` | Error | `default` or a parameterless constructor on `Result<,>`, `Either<,>`, `Validation<,>`, `ParseFailure`, `ResultTask<,>`, `OptionTask<>`, or `ValidationTask<,>` (not in array initializers) |
| `ROP0002` | Error | Those types used as expression statements or assigned to `_` |
| `ROP0003` | Error | A synchronous `Func`/`Func<,>` parameter receives a lambda or method group that returns `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>`; methods whose names end with `Async` are excluded |
| `ROP0004` | Warning | `RopError.Create` is called with a compile-time string code that does not match `^E_[A-Z0-9_]+$` |

**Consequence:** the four IDs are stable extension points; severity matches whether the mistake is a hard contract
break or a naming hygiene issue.

### D46 — Tests reference the analyzer assembly explicitly

`Sandlada.RopSharp.Tests` uses a normal `ProjectReference` to the analyzer project and passes the analyzer into
`CSharpCompilation.WithAnalyzers`. The test project does **not** register the analyzer as a compiler analyzer, because
the suite must still compile `default(Result<,>)` and similar patterns to lock the runtime uninitialized contract.

**Consequence:** analyzer behaviour is verified without blocking execution tests.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Separate analyzer package | TASK 0.0.2 requires analyzers in the main nupkg path |
| Analyzer-only enforcement for `Try` task-like success | Already enforced at runtime in 0.0.2; analyzers focus on compile-time footguns |
| Data-flow analysis for unused locals | Deferred; first version stays syntactic/semantic on obvious patterns |

## Consequences

- Sample and consumer projects that reference the library get the four rules during build.
- The analyzer project commits its own `packages.lock.json` for reproducible restore.
- ADR 0001 D11 (“no analyzers in 0.0.1”) remains historical; 0.0.2 adds the shipped analyzer assembly.
