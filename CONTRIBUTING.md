# Contributing to Sandlada.RopSharp

## Prerequisites

- .NET SDK **11.0** (preview is fine; `global.json` pins the feature band) with the **10.0** runtime/targeting pack,
  because the library and tests target `net10.0;net11.0`.
- Windows, Linux or macOS. The library is pure managed code.

## Build, test, pack

```bash
dotnet restore --locked-mode
dotnet format RopSharp.sln --verify-no-changes --no-restore
dotnet build -c Release
dotnet test -c Release --no-build
dotnet pack -c Release --no-build
```

`dotnet test` runs Microsoft.Testing.Platform (TUnit). Run a single test with tree filters:

```bash
dotnet test -- --treenode-filter "/*/*/ResultCoreTests/*"
dotnet test -- --treenode-filter "/*/*/ResultCoreTests/OrThrow_ReturnsValueOnSuccess"
```

The legacy VSTest `--filter "FullyQualifiedName~..."` syntax is not supported in this mode.

### NuGet lock files

`Sandlada.RopSharp.Tests/` and `Sandlada.RopSharp.Benchmarks/` commit `packages.lock.json`. The library and sample have no runtime package dependencies (their only package is an SDK-versioned analyzer), so they intentionally do not lock.

When you change a package version in `Directory.Packages.props`, refresh the lock files and commit them:

```bash
dotnet restore --force-evaluate
dotnet restore --locked-mode   # must succeed
```


## Design rules (non-negotiable)

1. **Symmetry over brevity.** `MapOk`/`MapErr`, `BindOk`/`BindErr`, `TeeOk`/`TeeErr`, `MapSome`/…
   No shortened aliases (`Map`, `Bind`, `Tee`, `OrElse`, `AndThen`, `FlatMap`, `Tap`, `Recover`, `As`, `Ignore`).
2. **Data-last.** The mapper/binder/predicate/action is the last parameter so chains read left to right.
3. **No implicit conversions, no operators, no `Deconstruct`.** The active track must stay visible.
4. **Expected failures are values.** Never throw for a domain failure. Only `ArgumentNullException` (null input),
   `OperationCanceledException` (cancellation) and internal invariant violations may throw.
5. **No null on the public surface.** Collections are never null; `Ok(null)`/`Some(null)` are rejected at the
   factory. A mapper may produce a nullable payload; `RequireNotNull` collapses it.
6. **`Option` never contains null.** Use `BindSomeNullable`/`BindSomeNullableValue` for nullable mappings.
7. **Short-circuit and aggregating models never mix implicitly.** New APIs must pick one and document it.
8. **Allocation- and AOT-friendly.** No reflection, no dynamic code, no boxing of the core types on hot paths.
9. **Every public member has XML documentation.** `CS1591` is an error.
10. **The public API is locked.** `Sandlada.RopSharp/PublicAPI.Shipped.txt` must match the surface exactly.

## Adding or changing public API

1. Implement with XML docs and nullable annotations.
2. Add TUnit coverage for every new call site (including null guards and the opposite track).
3. Add the new symbol to `PublicAPI.Unshipped.txt` (the analyzer reports the exact signature in `RS0016`).
4. On release, move the entries to `PublicAPI.Shipped.txt` and keep both files ordinally sorted.
5. Record design decisions in `docs/adr/` (add a new ADR for behaviour or surface changes) and update
   `CHANGELOG.md` for user-visible changes.

## Testing expectations

- Framework: **TUnit only** (do not add xUnit, NUnit or MSTest).
- Cover: success track, failure track, opposite-track non-execution, null/blank guards, cancellation, and
  aggregation ordering.
- Keep the seeded randomized law tests green (`RandomizedLawTests`).
- The allocation smoke tests (`PerformanceSmokeTests`) must stay green; do not introduce per-iteration allocations
  in the hot combinators.

Measure coverage before/after a change:

```bash
dotnet test -c Release -f net11.0 --coverage --coverage-output-format cobertura --coverage-output coverage.xml
```

## Performance changes

Run the relevant benchmarks before and after:

```bash
dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter "*"
```

Report allocation changes in the pull request and update `docs/benchmarks.md` when the baseline shifts.

## Commit / PR checklist

- [ ] `dotnet format RopSharp.sln --verify-no-changes` passes
- [ ] `dotnet build -c Release` has zero warnings
- [ ] `dotnet test -c Release` is green
- [ ] `dotnet pack -c Release` succeeds (package validation is enabled)
- [ ] Public API baseline updated deliberately
- [ ] `CHANGELOG.md` updated for user-visible changes
- [ ] Docs updated (`README.md`, `docs/`, or `docs/adr/`)

## Repository layout

```text
Sandlada.RopSharp/              library (shippable)
Sandlada.RopSharp.Tests/        TUnit suite (not shippable)
Sandlada.RopSharp.Sample/       runnable example
Sandlada.RopSharp.Benchmarks/   BenchmarkDotNet project
docs/                           ADRs, guides, verification and benchmark records
```
