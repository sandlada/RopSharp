# Changelog

All notable changes to `Sandlada.RopSharp` are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.1] - 2026-10-27

Initial public release. Consolidates the prior 1.0.0 and 1.1.0 development milestones into the first
published version.

### Added

- `Result<TOk, TErr>`: immutable readonly struct with `IsOk`/`IsErr`, non-throwing `TryGetOk`/`TryGetErr`,
  value equality and readable `ToString`.
- `Option<T>`: immutable readonly struct where `default` is contractually `None`.
- `Either<TLeft, TRight>`: `Left`/`Right`, `Match`, `MapLeft`, `MapRight`, `Swap`, `ToResult`, probes and
  value equality.
- `RopError`: structured, immutable error with `Code`/`Message`/`Details`/`Cause`/`Kind`, `Create`,
  `FromException`, `With*` and `Combine`; `RopErrorKind` (`Failure`, `Validation`, `NotFound`, `Conflict`,
  `Unauthorized`, `Forbidden`, `Unexpected`).
- `Rop` entry point: `Ok`, `Err`, `Try`, `TryAsync`, `Switch`, `TrySwitch`, `FromNullable`,
  `FromNullableValue`, `SuccessIf`, `FailureIf`, `Parse`, `TryParse`, `Combine`, `Zip`, `Validate`,
  `Sequence`, `SequenceValidation`, `Traverse`, `Partition`, `SequenceAsync`, `TraverseAsync`,
  `CombineAsync`, `ValidateAsync`, `ZipAsync`, `PartitionAsync`.
- `Opt` entry point: `Some`, `None`, `FromNullable`, `FromNullableValue`, `When`, `Combine`, `Zip`,
  `Sequence`, `Traverse`, `Values`.
- `OptionEnumerableExtensions`: `TryFirst`, `TryLast`, `TryFind`, `Choose`, `Execute`, `ExecuteNoValue`.
- `ResultTerminalExtensions`: `Switch`, `SwitchAsync`, `MatchFirst`, `SwitchFirst`.
- `ResultEnsureExtensions`: `FailIf` and `ThenEnsure`.
- Full symmetric combinator surface: `MapOk`/`MapErr`, `BindOk`/`BindErr`, `BindErrValue`,
  `TeeOk`/`TeeErr`, `Ensure`, `MapOkValue`, `RequireNotNull`, `Match`, `OrThrow`.
- Option surface: `MapSome`, `BindSome`, `BindSomeNullable`, `BindSomeNullableValue`, `BindNone`,
  `BindNoneValue`, `TeeSome`, `TeeNone`, `Filter`, `MapSomeValue`, `Flatten`, `RequireSome`, `ToOption`,
  `ToNullable`, `ToNullableValue`, `Transpose`.
- Cancellation-aware async combinators for both domains, plus async LINQ `Select`/`SelectMany` on
  `Task<Result<TOk,TErr>>`.
- Explicit short-circuit ↔ aggregating conversions: `ToValidation` and `ToSingleError`.
- LINQ query syntax: `Select`/`SelectMany` for results and options, plus `Where` for options.

### Changed

- `RopError` equality and hashing now include `Kind`; existing errors default to `Failure`.
- Targets `net10.0` and `net11.0` only; `LangVersion` latest; nullable reference types enabled.
- `Result<,>` and `Option<>` are `readonly struct`s with value equality.

### Documentation

- ADR 0001 (core design decisions) and ADR 0002 (gap-by-gap decision matrix and retained scope boundaries).
- Removed the outdated draft `spec/` folder after migrating still-valid decisions into `docs/adr/`.
- Traditional Chinese guide, benchmark baselines and verification gates under `docs/`.

### Quality gates

- Zero-warning build for `net10.0` and `net11.0` with warnings treated as errors.
- Public API baseline enforced by `Microsoft.CodeAnalysis.PublicApiAnalyzers`.
- Formatting verified with `dotnet format --verify-no-changes`.
- TUnit test suite (425 tests per target framework at the 1.1.0 milestone) covering algebraic laws,
  invariants, guards, aggregation, LINQ and async/cancellation semantics.
- Code coverage measured with `Microsoft.Testing.Extensions.CodeCoverage` (100% lines / 99.4% branches).
- Trim-published self-contained sample verified end to end (`PublishTrimmed` + run).
- Native AOT publish + run verified locally and in CI.
- BenchmarkDotNet baselines for the hot Result/Option combinators published in `docs/benchmarks.md`.
- XML documentation for every public member.
- `AnalysisMode=All` with justified local suppressions.
- `EnablePackageValidation` enabled; NuGet package ships both target frameworks, XML docs, README and LICENSE.
- `CONTRIBUTING.md`, `SECURITY.md` and `.gitattributes` included.
- Repository flattened to top-level project folders; public API unchanged.
- NuGet lock files committed for the test and benchmark graphs; CI and release restore with
  `--locked-mode`.
- AOT/trim/single-file analyzers enabled and clean.
- Aggregation views reuse exactly-sized lists; always-materializing aggregations pre-size lists from the
  source count.
- Pass-through combinators preserve a legal `Ok(null)` payload.
- Cancellation observed by the active track and by every element of `SequenceAsync`/`TraverseAsync`.
- Value arguments validated at entry without boxing.
- `RopError.Combine` validates every element before its single-element shortcut.
- NuGet package ships both target frameworks, XML docs, README and LICENSE.
- `NeutralLanguage=en`; `IncludeSymbols` + `snupkg` + `EmbedUntrackedSources` for Source Link debugging.
- `IsAotCompatible`, `IsTrimmable` and analyzers enabled.
- `Deterministic=true` builds.
- `Microsoft.CodeAnalysis.PublicApiAnalyzers` enforces `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`.
- `ManagePackageVersionsCentrally=true` with `CentralPackageTransitivePinningEnabled=true`.
- `RestorePackagesWithLockFile=false` for the library; lock files committed for test/benchmark graphs.
- CI/CD workflows removed by maintainer; build, test, trim verify and release are now run manually
  (see `dotnet build` / `dotnet test` / `dotnet pack` / `dotnet nuget push`).

### Notes

- NuGet package ID: `Sandlada.RopSharp`.
