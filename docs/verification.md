# Verification gates

Everything in this file is reproducible on a clean checkout. The commands are the same ones used by CI
(`.github/workflows/ci.yml`) and the release workflow.

## Summary

| Gate | Command | Expected |
| --- | --- | --- |
| Restore | `dotnet restore --locked-mode` | success (test/benchmark lock files enforced) |
| Formatting | `dotnet format RopSharp.sln --verify-no-changes --no-restore` | exit 0 |
| Build | `dotnet build -c Release` | 0 warnings, 0 errors |
| Tests | `dotnet test -c Release --no-build` | 1398 passed (699 on net10.0 + 699 on net11.0), 0 failed, 0 skipped |
| Coverage | see below | 100% line / 99.84% branch (library, net11.0) |
| Pack | `dotnet pack -c Release` | `Sandlada.RopSharp.0.0.1.nupkg` + `.snupkg`; package validation enabled |
| Consumer | local feed + fresh console apps | package restores on `net10.0` and `net11.0`, programs run |
| Public API | (build) | `PublicAPI.Shipped.txt` (252 symbols) + `PublicAPI.Unshipped.txt` (72 pending symbols) match the surface exactly |
| Dependencies | `dotnet list package --vulnerable --include-transitive` | no vulnerable packages |
| XML docs | build + XML inventory | every documented member has a summary |
| Trim runtime | see below | trimmed sample prints the same output as the normal build |
| Native AOT | see below | PublishAot=true native image builds and runs |

## Test matrix

The test project targets `net10.0` and `net11.0`; `dotnet test` runs both (699 tests per target, 1398 runs). Consecutive runs were green with no flakes. Coverage includes:

- construction/equality/invariants and null guards,
- the algebraic laws (functor/monad laws for both tracks, plus seeded randomized checks),
- short-circuit vs aggregating semantics, single enumeration, ordering,
- Option bridges and transposes,
- Option collection helpers (`TryFirst`/`TryLast`/`TryFind`/`Choose`/`Execute`),
- conditional construction, parsing, action terminals (`Switch`/`MatchFirst`/`SwitchFirst`) and
  `FailIf`/`ThenEnsure`,
- async aggregation (`CombineAsync`/`ValidateAsync`/`ZipAsync`/`PartitionAsync`) and async LINQ,
- Option async parity: sync-source async map/bind/tee/filter, `Opt.CombineAsync`/`ZipAsync`/
  `SequenceAsync`/`TraverseAsync`, and `Task<Option<T>>` query syntax,
- validation traversal: `Rop.TraverseValidation`, `Rop.SequenceValidationAsync` and
  `Rop.TraverseValidationAsync` error ordering, full mapping, faults, cancellation and null tasks,
- `RopError.InnerErrors`: aggregate structure, single-element identity, nested aggregation, immutable snapshots,
  `With*` propagation and equality exclusion,
- async boundaries: `Rop.Try(Action)`, `Rop.TryAsync(Func<Task>)`, `Rop.SwitchAsync` and `Rop.TrySwitchAsync`
  success, exception, null-task, null-payload and cancellation paths,
- diagnostics: `RopErrorKind` append-only numbering, `Rop.Parse` null/empty distinction, bounded echoing by Unicode
  code point, control/line-separator/bidi escaping, lone-surrogate escaping and surrogate-safe truncation,
- hardening: null-returning error factories across `MapErr`/`Ensure`/`FailIf`/`RequireSome`/`Either.ToResult`/
  `ToSingleError`, null-producing `IParsable<T>` implementations, null tasks in `PartitionAsync`, and linear
  `RopError.Combine` deduplication,
- invariants: single-pass enumeration for every sequence helper, read-only materialized collections, concurrent
  readers of shared immutable values and documented `default` behavior (an uninitialized `Result<,>` faults from
  `TryGetErr` and every terminal/error-track read instead of leaking a null error),
- model bridges: `Result`→`Either`, `Option`→`Either` (value and factory) and `Either`→`Option`, including null
  payloads, lazy factories and lossless round trips,
- null-element policy: `TryFirst`, `TryLast`, `TryFind` and the projecting `Choose` reject null elements with the
  `source`/`mapper` parameter name,
- `Either<TLeft,TRight>` equality, mapping, swapping and result conversion,
- `RopErrorKind` classification and combination semantics,
- custom error types: generic factory/boundary/parse overloads, class and record-struct errors, null-result
  factories, cancellation and the unchanged `RopError` overloads,
- async overload resolution, cancellation-aware overloads, `OperationCanceledException` propagation,
- opposite-track non-execution for every `Tee*` variant,
- allocation smoke tests for the hot Result/Option pipelines.

Single test (Microsoft.Testing.Platform syntax):

```bash
dotnet test -- --treenode-filter "/*/*/ResultCoreTests/*"
dotnet test -- --treenode-filter "/*/*/ResultCoreTests/OrThrow_ReturnsValueOnSuccess"
```

## Code coverage

```bash
dotnet test -c Release -f net11.0 --coverage \
  --coverage-output-format cobertura \
  --coverage-output cobertura.net11.xml \
  --results-directory artifacts/coverage
```

Latest measurement (`net11.0`, `Sandlada.RopSharp` assembly):

| Metric | Covered | Total | Rate |
| --- | ---: | ---: | ---: |
| Lines | 1484 | 1484 | 100% |
| Branches | 615 | 616 | 99.84% |

Every line is covered. The single remaining partial branch is the left-null arm of `Either.GetHashCode`, which is
unreachable through the public surface: `Either.Left` rejects null and `default(Either<,>)` is a right value. CI runs
this command and uploads the Cobertura report as an artifact (informational, no hard threshold).

## Packaged artifact consumption

The shipped `.nupkg` is installed from a local feed into fresh console apps and executed:

```bash
# Pack into a folder feed. NuGet requires an absolute path for folder sources.
dotnet pack Sandlada.RopSharp/Sandlada.RopSharp.csproj -c Release -o "$PWD/artifacts/local-feed"

dotnet new console -o consumer-net11 -f net11.0
dotnet add consumer-net11/consumer-net11.csproj package Sandlada.RopSharp \
  --version 0.0.1 --source "$PWD/artifacts/local-feed"
dotnet run --project consumer-net11/consumer-net11.csproj -c Release
```

Verified output on both `net10.0` and `net11.0`:

```text
ok:42
left-value
```

The cached package hash matched the freshly packed `.nupkg`, proving the consumer used this build.

## Trim verification

```bash
dotnet publish Sandlada.RopSharp.Sample/Sandlada.RopSharp.Sample.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishTrimmed=true -p:TrimMode=link -o artifacts/trim
./artifacts/trim/Sandlada.RopSharp.Sample.exe
```

Verified output matches the untrimmed run:

```text
=== 1. Fulfillment saga - multi-service tracks with reverse-order compensation ===
-- happy path: live tax, payment timeout retried once, all steps commit
   validate: Ada, 1 line(s)
   price: subtotal 39.98, discount 4.00, tax 5%, total 37.78
   reserve: WH-A for ord-1
   payment: gateway timeout, retrying once
   authorize: auth-retry-2 for 37.78
   ship: ground TRK-ord-1
   => OK ord-1: 37.78, ground TRK-ord-1, 2 compensation(s) armed

-- payment declined after inventory was reserved
   validate: Ada, 1 line(s)
   price: subtotal 19.99, discount 0.00, tax 5%, total 20.99
   reserve: WH-A for ord-2
   compensate: release ord-2 at WH-A
   => ERR E_DECLINED: issuer declined the card | compensation: all succeeded

-- shipment rejected and the payment void also fails
   validate: Ada, 1 line(s)
   price: subtotal 7.50, discount 0.00, tax 5%, total 7.88
   reserve: WH-A for ord-3
   authorize: auth-void-fails for 7.88
   compensate: void auth-void-fails FAILED
   compensate: release ord-3 at WH-A
   => ERR E_ADDRESS_REJECTED: carrier rejected the address | compensation failures: E_VOID_FAILED: issuer refused to void auth-void-fails

-- tax service outage recovered by the fallback rate
   validate: Ada, 1 line(s)
   tax: E_TAX_SERVICE, fallback 20%
   price: subtotal 19.99, discount 0.00, tax 20%, total 23.99
   reserve: WH-A for ord-4
   authorize: auth-primary for 23.99
   ship: ground TRK-ord-4
   => OK ord-4: 23.99, ground TRK-ord-4, 2 compensation(s) armed

-- invalid request: header and every line aggregate into one failure
   => ERR E_MULTIPLE: customer '' is too short; unknown coupon: BAD; quantity 0 must be positive; unknown sku: SKU-404 (E_CUSTOMER_TOO_SHORT, E_COUPON_UNKNOWN, E_QUANTITY, E_SKU_UNKNOWN) | compensation: all succeeded

-- tax outage with no fallback
   validate: Ada, 1 line(s)
   => ERR E_TAX_SERVICE: tax service has no route for this country (no fallback for country=ZZ) | compensation: all succeeded

-- cancellation before the first remote call
   validate: Ada, 1 line(s)
   => OperationCanceledException, no compensation ran

=== 2. Carrier quotes - eager fan-out, partial failure, retry and late selection failure ===
-- domestic: one rejection, one timeout, the timeout is retried
   bid ground 19.00 in 3d
   bid air    38.50 in 2d
   bid budget ERR E_NO_ROUTE: budget carrier does not serve this lane
   bid flaky  ERR E_CARRIER: flaky carrier timed out
   retry flaky after E_CARRIER
   => OK ground 19.00 in 3d (1 carrier(s) failed)
   carrier calls: 4

-- express with a one-day deadline: bids exist but none is eligible
   bid ground 19.00 in 3d
   bid air    38.50 in 2d
   bid budget ERR E_NO_ROUTE: budget carrier does not serve this lane
   => ERR E_DEADLINE: no carrier meets the 1-day deadline
   carrier calls: 2

-- remote lane: every carrier fails, all errors are combined
   bid ground ERR E_CARRIER: ground does not serve remote lanes
   bid air    ERR E_CARRIER: air does not serve remote lanes
   bid budget ERR E_NO_ROUTE: budget carrier does not serve this lane
   bid flaky  ERR E_CARRIER: flaky carrier timed out
   retry ground after E_CARRIER
   retry air after E_CARRIER
   retry flaky after E_CARRIER
   no bid survived (4 error(s))
   => ERR E_MULTIPLE: ground does not serve remote lanes; air does not serve remote lanes; budget carrier does not serve this lane; flaky does not serve remote lanes (E_CARRIER, E_NO_ROUTE)
   carrier calls: 6

-- gold member: the winning bid is discounted
   bid ground 19.00 in 3d
   bid air    38.50 in 2d
   => OK ground 16.15 in 3d (0 carrier(s) failed)
   carrier calls: 2

-- cancellation before any carrier is called
   => OperationCanceledException, carrier calls: 0

=== 3. Settlement batch - per-row pipelines, grouped failures and recovery after a state change ===
-- first pass
   retry row-4: E_CHANNEL_BUSY
   retry row-6: E_CHANNEL_BUSY
   settled row-1: 25.00 from acct-1
   settled row-4: 40.00 from acct-4
   settled row-8: 5.00 from acct-2
   failed  E_INSUFFICIENT_FUNDS: balance 10.00 < 60.00
   failed  E_MULTIPLE: account is frozen; balance 100.00 < 200.00; daily limit 100.00 < 200.00 (E_ACCOUNT_FROZEN, E_INSUFFICIENT_FUNDS, E_LIMIT_EXCEEDED)
   failed  E_ACCOUNT_UNKNOWN: no such account: acct-9
   failed  E_CHANNEL_BUSY: channel unavailable for row-6 (attempt 2)
   failed  E_LIMIT_EXCEEDED: daily limit 50.00 < 75.00
   summary: 3 settled, 5 failed
   codes: E_ACCOUNT_UNKNOWN x1, E_CHANNEL_BUSY x1, E_INSUFFICIENT_FUNDS x1, E_LIMIT_EXCEEDED x1, E_MULTIPLE x1

-- retry after top-up (1 row(s))
   settled row-2: 60.00 from acct-2
   summary: 1 settled, 0 failed

-- cancellation while the batch is in flight
   => OperationCanceledException before any charge attempt (attempts: 0)

=== 4. Parsing, Either, async aggregation and collection helpers ===
   parsed: 82
   either: refund
   option last: 3
```

The trim/AOT analyzers (`IsAotCompatible`, `EnableAotAnalyzer`, `EnableTrimAnalyzer`,
`EnableSingleFileAnalyzer`) are enabled with warnings as errors, and the library uses no reflection.

## Native AOT

Native AOT requires the platform native toolchain (on Windows: Visual Studio "Desktop development with C++" with
the MSVC x64 build tools and a Windows SDK). Verified locally on this machine (MSVC 14.51.36231, Windows SDK
10.0.26100) with .NET SDK 11.0.100 RC.1:

```bash
dotnet publish Sandlada.RopSharp.Sample/Sandlada.RopSharp.Sample.csproj -c Release -r win-x64 -p:PublishAot=true -o artifacts/aot
./artifacts/aot/Sandlada.RopSharp.Sample.exe
```

Result: publish succeeds with no AOT/trim warnings, producing a ~1.6 MB native executable (plus `.pdb`) that prints
the same output as the JIT run. On a machine without MSVC the publish fails in native toolchain discovery before
compiling any library code; that is an environment prerequisite, not a library defect. The analyzers still enforce
AOT safety on every build, and the `native-aot` CI job (GitHub-hosted Windows runner with the C++ workload)
publishes and runs the sample with `PublishAot=true` on every push.

## Benchmark baselines

See [`benchmarks.md`](benchmarks.md). Re-run with:

```bash
dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter "*"
```
