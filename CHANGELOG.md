# Changelog

All notable changes to `Sandlada.RopSharp` are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.1] - Unreleased

Initial public release, not yet published. Consolidates the prior 1.0.0 and 1.1.0 development milestones and the
additive work below into the first version.

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
- `Option` asynchronous parity with `Result`:
  - `MapSomeAsync`, `BindSomeAsync`, `BindNoneAsync`, `TeeSomeAsync`, `TeeNoneAsync` and `FilterAsync`
    now accept a synchronous `Option<T>` source, including cancellation-aware callback overloads.
  - `Opt.CombineAsync` / `Opt.ZipAsync` (2 and 3 inputs) and `Opt.SequenceAsync` / `Opt.TraverseAsync`
    (including a cancellation-aware mapper) mirror the synchronous aggregation helpers.
  - `Select` / `SelectMany` / `Where` now work on `Task<Option<T>>`, so query syntax spans awaited options.
- ADR 0003 documents the Option async parity decisions and cancellation semantics.
- Validation traversal is complete on every track:
  - `Rop.TraverseValidation` maps a whole sequence and collects every error, complementing `Rop.SequenceValidation`.
  - `Rop.SequenceValidationAsync` and `Rop.TraverseValidationAsync` (including a cancellation-aware mapper)
    await every task and return every error in source order instead of short-circuiting.
- ADR 0004 documents the validation traversal decisions and fault/cancellation behaviour.
- `RopError` aggregation is now lossless:
  - `RopError.InnerErrors` exposes the errors supplied to `RopError.Combine`, in order, and is an empty list for
    atomic errors instead of `null`.
  - `WithCode` / `WithMessage` / `WithDetails` / `WithKind` preserve `InnerErrors`; nested aggregates are kept
    nested rather than flattened.
  - The aggregate snapshots its inputs, so mutating the caller's list afterwards cannot change it.
- ADR 0005 documents the structured aggregation and equality decisions.
- Async boundary and lift parity:
  - `Rop.Try(Action, ...)` and `Rop.TryAsync(Func<Task>, ...)` (factory and `code` shapes) cover boundaries with no
    payload and return `Result<Unit, RopError>`.
  - `Rop.SwitchAsync` and `Rop.TrySwitchAsync` lift asynchronous functions into task-wrapped results, mirroring
    `Rop.Switch` / `Rop.TrySwitch`.
  - The `Rop.Try<T>` docs now warn that a `Task`-returning lambda belongs in `Rop.TryAsync`.
- ADR 0006 documents the boundary/lift decisions and naming.
- Diagnostics:
  - `RopErrorKind` gains `Timeout`, `Unavailable` and `RateLimited` (appended values `7`–`9`, no renumbering).
  - `Rop.Parse` now echoes the offending input in `Details` as `input=<quoted>`, reports `null` distinctly from an
    empty string, truncates the echo to 64 code points and escapes quotes, backslashes and control characters.
- ADR 0007 documents the diagnostic-text policy and the open-ended enum contract.
- Model bridging is now complete:
  - `Result<TOk,TErr>.ToEither()` maps success to `Right` and failure to `Left` (lossless round trip).
  - `Option<T>.ToEither(left | leftFactory)` maps `Some` to `Right` and `None` to `Left`.
  - `Either<TLeft,TRight>.ToOption()` keeps the right branch and explicitly discards the left value.
- ADR 0008 documents the bridge orientation, loss rules and validation behaviour.
- `OptionEnumerableExtensions` now enforces the "`Option` never contains null" rule on input as well:
  `TryFirst`, `TryLast`, `TryFind` and the projecting `Choose` throw `ArgumentNullException` with a message that
  names the offending source (`source` or `mapper`) instead of leaking `Some(null)` or a null list entry.
- ADR 0009 documents the null-element policy.
- Scope decisions recorded (ADR 0010, no new API): higher-arity `Combine`/`Validate`, bounded concurrency and
  `IAsyncEnumerable<T>` traversal stay caller-owned; the README design rules now state the alternatives.
- Custom error types are supported end to end:
  - `Rop.FromNullable`, `Rop.FromNullableValue`, `Rop.SuccessIf`, `Rop.FailureIf`, `RequireNotNull`,
    `Either.ToResult` and the parameterless `ToSingleError()` now have `TErr` overloads. The `RopError`
    overloads are unchanged and remain the default.
  - `Rop.Try`, `Rop.TryAsync`, `Rop.TrySwitch` and `Rop.TrySwitchAsync` have `TErr` overloads that take an
    explicit `Func<TErr>` null-result factory, preserving the "function returns null → `Err`" contract and the
    `OperationCanceledException` pass-through.
  - `Rop.Parse<T, TErr>` accepts a `Func<ParseFailure, TErr>` factory; `ParseFailure` carries
    `Code`/`Message`/`Input`/`TypeName`/`Details`, so the bounded, escaped diagnostics are preserved.
- ADR 0011 documents the custom-error-type decisions and the value-type caveats.
- Release engineering: `.github/workflows/ci.yml` builds, tests, measures coverage, packs and publishes the
  native AOT sample, and `.github/workflows/release.yml` packs and pushes the tagged/manual version. The package
  version stays `0.0.1` until a release is tagged; the published version is supplied by the `vX.Y.Z` tag or the
  workflow `version` input at pack time. The same steps remain runnable manually (`dotnet build` / `dotnet test` /
  `dotnet pack` / `dotnet nuget push`).

### Changed

- `RopError` equality and hashing now include `Kind`; existing errors default to `Failure`.
- Targets `net10.0` and `net11.0` only; `LangVersion` latest; nullable reference types enabled.
- `Result<,>` and `Option<>` are `readonly struct`s with value equality.

### Fixed

- Diagnostic echoing now matches the documented contract: `Rop.Parse` truncates at 64 Unicode code points instead of
  64 UTF-16 code units, escapes lone surrogates so the rendered text is always well-formed, and escapes
  `U+2028`/`U+2029` plus bidirectional control characters so a payload cannot forge a log line or reorder terminal
  output.
- `Rop.TryParse` and the `Rop.Parse` overloads treat an `IParsable<T>` implementation that reports success with a
  null value as a parse failure instead of throwing from `Opt.Some`/`Rop.Ok`.
- Lazy error factories passed to `MapErr`, `Ensure`, `FailIf`, `RequireSome`, `Either.ToResult` and
  `ToSingleError(combine)` now fault with `InvalidOperationException` when they return null, matching the
  documented behavior of the boundary and conversion helpers.
- `Rop.PartitionAsync` rejects a null task with `InvalidOperationException`, matching `SequenceValidationAsync` and
  `TraverseValidationAsync`.
- `RopError.Combine` deduplicates messages and codes with ordinal hash sets, so the aggregation is linear in the
  number of errors instead of quadratic.
- `default(Result<TOk,TErr>)` no longer leaks a null error: `TryGetErr`, `Match`, `Switch`, the error-track
  combinators and `ErrOrThrow` now fault with `InvalidOperationException` instead of handing `null` to user code.
  Valid results and non-nullable value-type errors are unchanged.

### Documentation

- ADR 0007 records the diagnostic escaping and code-point truncation contract, including the maintenance revision.
- ADR 0001 (core design decisions) and ADR 0002 (gap-by-gap decision matrix and retained scope boundaries).
- Removed the outdated draft `spec/` folder after migrating still-valid decisions into `docs/adr/`.
- Traditional Chinese guide, benchmark baselines and verification gates under `docs/`.

### Quality gates

- Zero-warning build for `net10.0` and `net11.0` with warnings treated as errors.
- Public API baseline enforced by `Microsoft.CodeAnalysis.PublicApiAnalyzers`.
- Formatting verified with `dotnet format --verify-no-changes`.
- TUnit test suite (1398 runs, 699 per target framework) covering algebraic laws, invariants, guards,
  aggregation, LINQ, Option async parity, validation traversal, async boundaries, diagnostics, model bridges,
  custom error types, async/cancellation semantics, single enumeration and concurrent readers.
- Code coverage measured with `Microsoft.Testing.Extensions.CodeCoverage` (100% lines / 99.84% branches on `net11.0`).
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

### Notes

- NuGet package ID: `Sandlada.RopSharp`.
