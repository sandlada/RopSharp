# Role

You are a senior C#/.NET library maintainer and code reviewer. Perform a **dimension-by-dimension, verifiable, committable improvement scan** of `Sandlada.RopSharp` (NuGet package `Sandlada.RopSharp`, namespace `Sandlada.RopSharp`).

# Repository facts (verified — do not re-litigate, but you may re-check them)

- Layout: the library project lives at the repo root as `Sandlada.RopSharp/` (31 .cs files, ~9.7k lines), `Sandlada.RopSharp.Tests/` (47 .cs files, ~8k lines, ~718 `[Test]` methods, TUnit 1.76.28), `Sandlada.RopSharp.Benchmarks/` (BenchmarkDotNet 0.15.8), `Sandlada.RopSharp.Sample/` (console sample).
- Library: `net10.0;net11.0`, `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors`, `AnalysisMode=All`, `GenerateDocumentationFile=true` (CS1591 enforces XML docs), `IsAotCompatible`/`IsTrimmable`, with `PublicAPI.Shipped.txt` (252 entries) / `PublicAPI.Unshipped.txt` (72 entries) as the public API baseline.
- Entry points: `public static partial class Rop` (Rop.cs / Rop.Async.cs / Rop.Aggregation.cs / Rop.Aggregation.Async.cs / Rop.Conditional.cs) and `public static partial class Opt` (Opt.cs / Opt.Async.cs).
- Types: `readonly struct Result<TOk,TErr>`, `Option<T>`, `Either`, `Unit`, `ParseFailure`, `RopError`, `RopErrorKind`, `DiagnosticText`; the generic types have **no** static factory members.
- Extensions: `ResultExtensions` / `ResultAsyncExtensions` (855 lines) / `ResultLinqExtensions` / `ResultTerminalExtensions` / `ResultEnsureExtensions` / `ResultValidationExtensions` / `OptionExtensions` / `OptionAsyncExtensions` (941 lines) / `OptionEnumerableExtensions` / `OptionLinqExtensions` / `OptionBridgeExtensions` / `OptionNullableExtensions` / `OptionNullableValueExtensions` / `EitherBridgeExtensions`.
- Test conventions: files named `<Subject>Tests.cs`, methods named `<Member>_<ExpectedBehavior>`; special suites are `ResultLawTests`, `RandomizedLawTests`, `PerformanceGuards/AllocationGuardTests`, `Documentation/DocExampleCompilationTests` (Roslyn type-checks XML doc examples).
- Docs: `docs/adr/0001..0011`, `docs/benchmarks.md`, `docs/verification.md`, `docs/performance-budgets.md`, `docs/releasing.md`, `docs/doc-examples.md`, `docs/zh-TW/getting-started.md`; at the repo root `README.md`, `CHANGELOG.md` (Keep-a-Changelog), `CONTRIBUTING.md`, `AGENTS.md`, `SECURITY.md`, `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `perf/aot-size.baseline.json`.
- CI: `.github/workflows/ci.yml` (restore --locked-mode → dotnet format --verify-no-changes → build -c Release → test -c Release → coverage → pack → Sample native-AOT publish + binary-size budget compared against `perf/aot-size.baseline.json`), `release.yml`, `benchmarks.yml` (nightly BDN, non-blocking).

# Hard constraints

1. Target frameworks may only be `net10.0;net11.0`; **never** add `net8.0`/`net9.0` or anything older.
2. The test framework is **TUnit only**; do not introduce xUnit/NUnit/MSTest.
3. **Add no dependencies at all** (runtime dependencies must stay at zero; no dev-time analyzer either).
4. Keep the public API style functional-first: higher-order functions, data-last parameter order (`func`/`binder`/`mapper` last), failures expressed as values rather than exceptions, no `null` on the public surface, no reflection in hot paths, AOT-friendly.
5. Breaking changes are allowed: the version **stays at 0.0.1**, and `PublicAPI.Shipped.txt` is **rewritten directly** (no need to move entries to Unshipped), but CHANGELOG must record the breaking changes.
6. The public entry points are unified to **`Result` / `Option` style**: `Rop` → `Result`, `Opt` → `Option`, **replaced outright with the old names deleted** — no `[Obsolete]` aliases. Technical premise: a non-generic class and a generic struct can coexist because they differ in arity (`Result` / `Result<TOk,TErr>`, `Option` / `Option<T>`). If a real compilation or resolution obstacle appears (e.g. CS0101, extension-method resolution ambiguity), stop and report — do not invent a third naming scheme.
7. Documentation sync is mandatory: `README.md`, `docs/**` (including ADRs), `CHANGELOG.md`, `Sandlada.RopSharp.Sample/**`, and `AGENTS.md` (add a section recording "public entry points = `Result`/`Option`" and "breaking changes must update the PublicAPI baseline") must match the code. Add a new ADR documenting the naming and breaking-change decisions; **never** rewrite the conclusions of existing ADRs.

# Execution model: advance one dimension per round

Fixed dimension order (highest priority first): **① Correctness / bug risk → ② Public API breakage & design consistency → ③ Performance & allocation → ④ Test coverage & test quality → ⑤ Accessibility & API compatibility → ⑥ Style & duplicated code**.

Each round runs in this order, and **every round must end with a commit plus a report section**:

1. **Round plan** (print it in the session first, ≤5 lines, do not wait for confirmation, then execute):
   - The concrete items to fix this round (file:line)
   - Estimated diff size and risk
   - Whether public API changes are involved
2. **Implement**: minimal, reviewable changes; one commit per logical fix.
3. **Verify** (all must pass; keep the commands aligned with CI):
   ```
   dotnet restore --locked-mode
   dotnet format RopSharp.sln --verify-no-changes --no-restore
   dotnet build -c Release --no-restore
   dotnet test -c Release --no-build
   dotnet pack -c Release --no-build
   ```
   - When the performance dimension is involved: additionally run the **subset** of affected benchmarks (`dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter <Pattern>`); **do not** run the full BDN suite. `PerformanceGuards/AllocationGuardTests` and the Sample native-AOT binary-size budget (compared against `perf/aot-size.baseline.json`) must be green.
   - Single TUnit test: `dotnet test -- --treenode-filter "/*/*/<TestClass>/*"` (**do not** use the VSTest `--filter "FullyQualifiedName~..."` syntax).
   - If a failure was introduced by you, **restore green before committing**; never commit a red test run.
4. **Commit**: commit directly on the **current branch**; do not create a branch. One commit per logical fix, with these message prefixes:
   `fix/scan:` (correctness), `refactor/scan:` (structure/duplication/naming), `test/scan:` (tests), `docs/scan:` (documentation), `chore/scan:` (repo hygiene).
   The message body states: the problem in one sentence → root cause → the change → verification commands and results.
5. **Report**: **append** this round's section to `REVIEW-<yyyyMMdd>.md` (repo root, using the actual execution date). `docs/` is for ADRs only — no scan reports there.

# Dimension details

**① Correctness / bug risk**

- `default(Option<T>)` / `default(Result<T,E>)` paths: do members such as `TryGetErr` still throw when documented as an "invalid instance", and can callers reach those paths?
- Nullability: does any public API path return `null` or allow `null`? Are the semantics of `Option<T>` vs `Option<T?>` / `OptionNullableExtensions` / `OptionNullableValueExtensions` self-consistent?
- Exception paths: do `Try` / `TryAsync` / `TryCatch`-style APIs swallow exceptions they should not (e.g. `OperationCanceledException`)?
- Async: any sync-over-async, `.Result` / `.GetAwaiter().GetResult()`, missing `ConfigureAwait`, or cancellation tokens not propagated?
- Aggregation: is the "lossless aggregation" of `Rop.Aggregation*` truly lossless — error ordering, duplicated errors, and short-circuit semantics on partial success must agree with the ADRs.
- Collections/enumeration: empty-collection and multi-match semantics of `TryFirst`/`TryLast`/`TryFind` in `OptionEnumerableExtensions`; enumerator `Dispose` and exception paths.
- `Either` bridging: can the two-way conversions in `EitherBridgeExtensions` lose error information?

**② Public API breakage & design consistency**

- Unify the entry points to `Result` / `Option` (see constraint 6), and check whether the entry-point method set and the extension-method set overlap or use inconsistent names (e.g. mixing `Bind`, `Map`, `Select`).
- Shape consistency: are analogous operations symmetric across `Result`/`Option`/`Either` (is the async capability of `Option` on par with `Result` — note ADR 0003 explicitly demands parity, so a parity gap is a defect)?
- Naming: how do long names such as `BindSomeNullableValue` fit the library-wide style? How tightly are `RopError`/`RopErrorKind` coupled to `Result`/`Option`?
- Does `PublicAPI.Shipped.txt` drift from the actual public surface (it must be resynced after renaming)?
- Are the boundaries of `Either`/`Unit`/`ParseFailure`/`DiagnosticText` clear, or do several types express the same concept?

**③ Performance & allocation**

- Implicit allocations on hot paths: closure captures, boxing (generic value types boxed to `object`/interfaces), iterators, per-call `new delegate`, and string interpolation cost on the `RopError`/`DiagnosticText` construction paths.
- Do `Equals`/`GetHashCode`/`ToString` on the `readonly struct`s allocate unnecessarily or use reflection?
- AOT/trimming: any reflection, `dynamic`, or generic virtual calls producing AOT warnings?
- Compare against `docs/performance-budgets.md` and `perf/aot-size.baseline.json` to find over-budget items. **Known lead (verify yourself)**: `perfout.txt` at the repo root is a raw output file that is tracked by git, and it records a **failing** allocation assertion (`ResultPipeline_DoesNotAllocatePerIteration`, expected <8192 bytes, measured 96168 bytes) — determine whether the budget is too strict, the test is written incorrectly, or this is a real regression.
- Splitting/refactoring must not degrade performance: compare relevant benchmark subsets before and after the change.

**④ Test coverage & test quality**

- Coverage gaps: does every public API have behavior-level tests? Pay special attention to `Rop.Aggregation*`, `Rop.Conditional`, `Either` bridging, `OptionEnumerableExtensions`, and custom error type paths.
- Assertion quality: are there tautological assertions, assertions that only check "does not throw", or missing failure-branch assertions?
- Test organization: the suite currently contains **meta-bucket** names such as `CoverageGapTests`, `AdditionalCoverageTests`, `MaintenanceHardeningTests`, `ReviewRegressionTests` (coverage-driven rather than behavior-driven). **New tests must use behavior-driven naming** (`<Subject>Tests.cs` + `<Member>_<ExpectedBehavior>`). Existing meta-bucket tests may be refactored/merged, but **assertions must never be deleted to make tests pass**; any deleted assertion must be listed under "Decisions Pending".
- Do the property tests in `RandomizedLawTests` / `ResultLawTests` cover the async variants?
- Does `Documentation/DocExampleCompilationTests` cover every XML comment that contains examples?

**⑤ Accessibility & API compatibility**

- Generic parameter naming consistency (`TOk`/`TErr` vs `TValue`/`TError` mix), type parameter counts, constraint styles.
- Overload resolution ambiguity between extension methods and instance methods (especially names shared with the BCL such as `Try*`/`Select`/`Where`).
- Are the `==`/`!=`/default-value semantics of the `readonly struct`s documented consistently?
- Breaking change inventory: a summary of every public API change in this run (removal/rename/signature change/semantic change) for CHANGELOG and ADR use.
- XML documentation completeness (CS1591 already enforces presence, but **semantic** accuracy must be checked: `<param>`, `<returns>`, `<exception>`, examples vs actual behavior).

**⑥ Style & duplicated code**

- Splitting large files: should `OptionAsyncExtensions.cs` (941 lines), `ResultAsyncExtensions.cs` (855 lines), `Rop.cs` (638 lines), `RopError.cs` (597 lines) be split into per-domain subdirectories/files?
- Duplicated implementations: **known lead** — `OptionNullableExtensions.cs` and `OptionNullableValueExtensions.cs` are both 67 lines and differ in only 29 lines (near-clones); extract a shared generic core.
- Structural symmetry between `Rop` and `Opt` (after renaming: `Result` and `Option`) so two hand-written implementations cannot drift apart.
- Does the actual code style match `.editorconfig`? Is the mismatch between the global `GenerateDocumentationFile=false` in `Directory.Build.props` and the library's override to `true` misleading?
- Naming consistency: after unifying to `Result`/`Option` style, are there leftover `Rop*`-prefixed types (`RopError`, `RopErrorKind`)? **Do not rename them on your own** — `RopError` shares lineage with the package name `RopSharp`; list it under "Decisions Pending".

**Repo hygiene (its own small bucket, `chore/scan:`)**

- `perfout.txt` and `probeout.txt` are raw scratch outputs tracked by git → remove them from version control and add them to `.gitignore` (follow any existing ignore conventions in the repo).
- Look for other artifacts that should not be committed (bin/obj leaks, temporary scripts, raw benchmark outputs).
- The misleading global `GenerateDocumentationFile=false` default in `Directory.Build.props` → evaluate switching the default to `true` or documenting it with a comment; if the blast radius is large, list it under "Decisions Pending".

# Abort conditions (stop and report instead of deciding on your own)

1. build or test **fails 3 times in a row** within a single round and the root cause is still unidentified.
2. You would need to **add any dependency** (including an analyzer), or would need to relax `TreatWarningsAsErrors` / `AnalysisMode=All` / CS1591 to make the code pass.
3. Fixing it requires changing **core semantics**: error propagation strategy, aggregation short-circuit/lossless semantics, Option/Result default-value behavior, or any API shape change beyond the `Result`/`Option` entry-point rename.
4. Renaming `Rop`→`Result` / `Opt`→`Option` hits a real compilation or resolution obstacle (e.g. arity conflict, extension-method resolution ambiguity) that cannot be solved without violating constraint 6.

When aborting: state the symptom, the approaches already attempted, the candidate solutions with their respective costs, give your recommendation, and wait for instructions.

# Prohibited actions

- Do not change target frameworks, `LangVersion`, nullable settings, `TreatWarningsAsErrors`, `AnalysisMode`, or the CS1591 requirement.
- Do not introduce xUnit/NUnit/MSTest, do not add runtime dependencies, do not use `unsafe` (unless explicitly asked).
- Do not weaken assertions, delete tests, or skip cases in order to make the suite pass.
- Do not commit any state that fails `dotnet build` / `dotnet test` / `dotnet format --verify-no-changes`.
- Do not rewrite the conclusions of existing ADRs; new ADRs are appended only.
- Do not perform one giant refactor in one giant commit; every round must be independently reviewable and revertible.

# Report format

File: `REVIEW-<yyyyMMdd>.md` at the repo root. Append per round, structured as follows:

## Part 1 — Implemented

| # | Dimension | Severity | File:Line | Problem | Fix | Commit | Verification |
|---|-----------|----------|-----------|---------|-----|--------|--------------|
| 1 | ① Correctness | High | `File.cs:123` | ... | ... | `abc1234` | `build✅ test✅ format✅` |

(One row per landed fix; the verification column records the actual commands run and their results.)

## Part 2 — Decisions Pending

| # | Dimension | Problem | Impact | Proposed Solution | Estimated Diff Size | Cost | Reversibility |
|---|-----------|---------|--------|-------------------|---------------------|------|---------------|

(Items not done this round but genuinely problematic — especially: the breaking API change inventory, whether to unify the `RopError`/`RopErrorKind` prefix, the large-file split plan, adjudication of the failing allocation gate, and the test meta-bucket refactor plan.)

## Part 3 — Commit History

All commits of this run, in chronological order (hash + subject).

# Definition of done

All six dimensions have been processed, and:

- Every round has: plan → change → all three verifications green → its own commit → report section;
- The report file exists at the repo root;
- `dotnet build -c Release`, `dotnet test -c Release`, and `dotnet format --verify-no-changes` are all green;
- CHANGELOG records the breaking changes, `AGENTS.md` is updated, and README/docs/Sample match the code;
- The final message states: the number of fixes per dimension, the verification results, and the number of items still pending your decision.
