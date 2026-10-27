# Sandlada.RopSharp

Production-grade railway-oriented programming (ROP) for modern C#.

`Sandlada.RopSharp` gives you immutable structs — `Result<TOk, TErr>`, `Option<T>` and `Either<TLeft, TRight>` —
together with a structured `RopError`, applicative validation, parsing helpers, and cancellation-aware async
combinators. Expected failures are values, never exceptions. The public surface is fully symmetric, has no
aliases, performs no reflection, and is AOT/trim friendly.

```csharp
using Sandlada.RopSharp;

Result<string, RopError> greeting =
    Rop.Ok(user)
       .BindOk(u => ValidateName(u.Name))
       .MapOk(name => name.Trim())
       .TeeOk(name => Log(name))
       .MapErr(e => e.WithCode("E_USER_NAME"));

string text = greeting.Match(
    ok: name => $"hello {name}",
    err: e => $"fail {e.Code}: {e.Message}");
```

- **Targets:** `net10.0` and `net11.0` only.
- **Package:** `Sandlada.RopSharp` (MIT).
- **Dependencies:** none.

## Install

```bash
dotnet add package Sandlada.RopSharp
```

## Three types, three questions

| Question | Type | Success | Absence/failure |
| --- | --- | --- | --- |
| "Did it fail, and why?" | `Result<TOk, TErr>` | `Ok(value)` | `Err(error)` |
| "Is there a value?" | `Option<T>` | `Some(value)` | `None` |
| "Which of two ordinary values?" | `Either<TLeft, TRight>` | `Left(left)` | `Right(right)` |

`Option` never carries a reason. When the reason matters, cross over explicitly with `RequireSome`.
When both branches are ordinary values (not success/failure), use `Either<TLeft,TRight>` instead of forcing a
result convention:

```csharp
Result<Address, RopError> address =
    Opt.FromNullable(user.Profile)
       .BindSome(p => Opt.FromNullable(p.Address))
       .RequireSome(() => RopError.Create("E_ADDRESS", "address required"));
```

## Result quick start

```csharp
// Construction (production default: Err = RopError)
Result<int, RopError> ok  = Rop.Ok(42);
Result<int, RopError> err = Rop.Err<int>(RopError.Create("E_X", "bad"));

// Custom error types are allowed (tests, interop), RopError is recommended in production
Result<int, string> custom = Rop.Ok<int, string>(42);

// Non-throwing branch queries
if (result.TryGetOk(out var value)) { /* ... */ }
if (result.TryGetErr(out var error)) { /* ... */ }

// The only full exit
string text = result.Match(ok: v => v.ToString(), err: e => e.Code);

// Lightweight exits (explicit defaults only)
int fallback = result.BindErrValue(0);
int lazy     = result.BindErrValue(() => ComputeDefault());
```

`OrThrow()`, `OkOrThrow()` and `ErrOrThrow()` exist for tests and boundaries only.

## Option quick start

```csharp
Option<string> some = Opt.Some("x");
Option<string> none = Opt.None<string>();

string? maybe = null;
Option<string> lifted = Opt.FromNullable(maybe);              // reference types
Option<int>    age    = Opt.FromNullableValue(maybeAge);      // Nullable<T>

string text = some.Match(some: v => v, none: () => "anonymous");

// default(Option<T>) is contractually None — safe in arrays, fields and generic containers
Option<Order>[] slots = new Option<Order>[3];                 // all None
```

## Mental model

| Concept | Member |
| --- | --- |
| Success-track transform | `MapOk` / `MapSome` |
| Error-track transform | `MapErr` (no `MapNone`: a missing value has nothing to map) |
| Success-track expansion | `BindOk` / `BindSome` |
| Failure-track recovery | `BindErr` / `BindNone` |
| Failure-track recovery with value | `BindErrValue` / `BindNoneValue` |
| Observation without changing track | `TeeOk` / `TeeErr` / `TeeSome` / `TeeNone` |
| Assertion | `Ensure` (Result) / `Filter` (Option) |
| Value replacement | `MapOkValue` / `MapSomeValue` |
| Full exit | `Match` / `MatchFirst` |
| Action exit | `Switch` / `SwitchAsync` / `SwitchFirst` |
| Non-throwing probe | `TryGetOk` / `TryGetErr` / `TryGetValue` |
| Short-circuit combination | `Rop.Combine`, `Rop.Zip`, `Rop.Sequence`, `Rop.Traverse` |
| Aggregating combination | `Rop.Validate`, `Rop.SequenceValidation` |
| Explicit model conversion | `ToValidation` / `ToSingleError` / `RequireSome` / `ToOption` / `Transpose` |

Every combinator is **data-last** (mapper/binder/predicate/action last), so chains read left to right.

## Error model

`RopError` is immutable and structured:

```csharp
RopError error = RopError.Create(
    code: "E_USER_NOT_FOUND",     // stable, machine-readable, safe to switch on
    message: "user 42 not found", // human-readable, display only
    details: "id=42");            // optional machine/diagnostic payload

RopError fromBoundary = RopError.FromException(exception, "E_IO"); // Cause is captured here only
RopError retagged     = error.WithCode("E_USER");
```

- `Code` and `Message` must be non-blank; blank input is a programming error (`ArgumentException`).
- `Details` may be `null` but must not be blank.
- `Kind` is a coarse category (`Failure`, `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`,
  `Unexpected`) for transport/presentation mapping; branch on `Code` for precise domain decisions.
- Equality compares `Kind`, `Code`, `Message` and `Details`; `Cause` is excluded so stack-trace noise cannot make
  two logically equal errors unequal.
- `RopError.Combine(errors)` collapses a non-empty list into `E_MULTIPLE`; a uniform `Kind` is preserved and mixed
  kinds fall back to `Failure`.

Recommended code convention: `E_<DOMAIN>[_<REASON>]`, uppercase with underscores.

## Short-circuit vs aggregating

The two models are deliberately separate and never mix implicitly:

```csharp
// Short-circuit: first error wins, later functions are not executed
Result<Order, RopError> order =
    Rop.Ok(raw)
       .BindOk(CheckName)     // stops here on failure
       .BindOk(CheckAmount);

// Aggregating: every input is inspected, all errors are collected in argument order
Result<Order, IReadOnlyList<RopError>> validated =
    Rop.Validate(CheckName(raw), CheckAmount(raw), CheckStock(raw),
                 (name, amount, stock) => new Order(name, amount, stock));

// Explicit conversions between the tracks
Result<Order, IReadOnlyList<RopError>> lifted = order.ToValidation();
Result<Order, RopError> single = validated.ToSingleError();               // first error
Result<Order, RopError> merged = validated.ToSingleError(RopError.Combine);
```

`Sequence`, `Traverse` and `Partition` accept any `IEnumerable<T>` but materialize exactly once. Empty input
yields `Ok([])` (or `Some([])` for options), never `null`.

## Async and cancellation

The map/bind/tee combinators have `...Async` counterparts. The token is optional and always last; cancellation
is reported as `OperationCanceledException` and never converted into an error, because errors describe domain
failures.

```csharp
Task<Result<string, RopError>> chained =
    GetUserAsync(id, ct)
        .MapOkAsync(u => u.Name, ct)
        .BindOkAsync(u => GetOrderAsync(u.Id, ct), ct);

// Cancellation-aware mapper receives the same token; the library never creates its own CTS
Task<Result<string, RopError>> ct2 =
    GetUserAsync(id, ct).MapOkAsync((u, token) => FetchNameAsync(u, token), ct);
```

Semantics:

- Cancelled token + pending antecedent → the returned task is cancelled, the callback is not invoked.
- The track that would invoke a callback checks the token first, so a cancelled token never starts new user work
  (`Ok` for `MapOk`/`BindOk`/`TeeOk`, `Err` for `MapErr`/`BindErr`/`TeeErr`, and the matching `Some`/`None` track).
- The pass-through track (no callback to run) returns the original value without observing the token.
- `SequenceAsync` and `TraverseAsync` check the token before the loop and before every element, so an
  already-completed task cannot bypass cancellation and `TraverseAsync` never maps after cancellation.
- Library code always uses `ConfigureAwait(false)`.
- Async argument validation surfaces through the returned task (the methods are asynchronous).
- A callback that returns a `Task` must not return `null`; that programming error faults with
  `InvalidOperationException` naming the callback (`mapper`, `binder`, `fallback`, `action` or `predicate`).

`Rop.TryAsync`, `Rop.SequenceAsync` and `Rop.TraverseAsync` follow the same rules. `SequenceAsync` awaits in
enumeration order so the reported failure is deterministic; start tasks eagerly if they should run concurrently.

## Boundaries

Exceptions become domain values in exactly one place:

```csharp
Result<int, RopError> parsed = Rop.Try(() => int.Parse(text), ex => RopError.FromException(ex, "E_PARSE"));
Result<int, RopError> byCode = Rop.Try(() => int.Parse(text), code: "E_PARSE");

Result<string, RopError> required = Rop.FromNullable(maybeName, () => RopError.Create("E_NULL", "name required"));
Result<int, RopError> requiredAge  = Rop.FromNullableValue(maybeAge, () => RopError.Create("E_AGE", "age required"));
```

- `OperationCanceledException` is always rethrown, never converted.
- A boundary function returning `null` yields `Err` with code `E_NULL_RESULT` — an `Ok(null)` is never produced
  by `Try`.
- `Rop.Switch` lifts a plain function without catching; `Rop.TrySwitch` lifts it with exception conversion.

## LINQ

```csharp
// Result: select + from (no where — there is no error value to produce for a predicate)
var total = from user in GetUser(id)
            from order in GetOrder(user.Id)
            select order.Total;

// Option: select + from + where (where is just Filter)
var sum = from a in Opt.Some(1)
          from b in Opt.Some(2)
          where a < b
          select a + b;
```

## Nullability rules

- `Rop.Ok(null)`, `Rop.Err(null)`, `Opt.Some(null)` throw `ArgumentNullException` (input guards, programming errors).
- A mapper is allowed to produce a nullable success payload: `Result<string?, RopError>`. Enforce non-nullness with
  `RequireNotNull`; use `Option` when absence is expected.
- `Option<T>` never contains `null`; map nullable results with `BindSomeNullable` / `BindSomeNullableValue`.
- `ToNullable` is the only place the library hands out `null` for reference types; cross back with
  `Opt.FromNullable` at the consuming boundary.

```csharp
Result<string, RopError> name =
    GetUser(id)
        .MapOk(u => u.Name)                                     // string?
        .RequireNotNull(() => RopError.Create("E_NULL", "name required"));
```

## Design rules

1. **Symmetry over brevity.** `MapOk`/`MapErr`, `BindOk`/`BindErr`, `TeeOk`/`TeeErr`. There is no `Map`, `Bind` or
   `Tee` shorthand; aliases are rejected by design.
2. **Two static entry points.** `Rop` (results and boundaries) and `Opt` (options). Receiver-based operations are
   extension methods only.
3. **No implicit conversions and no operators.** `T → Result`, `T → Option`, `Result → T`, `Result → bool`,
   `Deconstruct` and `|`/`>>` are all rejected: they hide the track.
4. **No null on the public surface.** Aggregations return empty collections, never `null`.
5. **No reflection on hot paths, no boxing of the core types.**
6. **`default(Option<T>)` is `None`; `default(Result<,>)` is undefined** — never rely on it.

## API map

### Result (sync)

```text
Rop.Ok<T>(value)                              Rop.Err<T>(error)
Rop.Ok<TOk,TErr>(value)                       Rop.Err<TOk,TErr>(error)
TryGetOk / TryGetErr / OkOrDefault / IsOk / IsErr
MapOk / MapErr / TeeOk / TeeErr
Ensure(predicate, error) / Ensure(predicate, errorFactory)
MapOkValue / RequireNotNull
BindOk / BindErr / BindErrValue(value|factory)
Match(ok, err) / OrThrow / OkOrThrow / ErrOrThrow
```

### Result (aggregation and boundaries)

```text
Rop.Combine(a, b, merger)                     Rop.Combine(a, b, c, merger)
Rop.Zip(a, b)                                 Rop.Zip(a, b, c)
Rop.Validate(a, b, merger)                    Rop.Validate(a, b, c, merger)
Rop.Sequence(results)                         Rop.SequenceValidation(results)
Rop.Traverse(source, mapper)                  Rop.Partition(results)
ToValidation() / ToSingleError() / ToSingleError(combine)
Rop.Try(func, errorFactory) / Rop.Try(func, code)
Rop.TryAsync(func, errorFactory, ct) / Rop.TryAsync(func, code, ct)
Rop.Switch(func) / Rop.TrySwitch(func, errorFactory)
Rop.FromNullable(value, errorFactory) / Rop.FromNullableValue(value, errorFactory)
Rop.SequenceAsync(tasks, ct) / Rop.TraverseAsync(source, mapper, ct)
Select / SelectMany
```

### Result (async)

```text
AsTask
MapOkAsync  (sync mapper | async mapper | CT-aware async mapper)   on Task<Result<,>> and Result<,>
MapErrAsync (sync mapper | async mapper | CT-aware async mapper)   on Task<Result<,>> and Result<,>
BindOkAsync (async binder | CT-aware binder)                       on Task<Result<,>> and Result<,>
BindErrAsync(async fallback | CT-aware fallback)                   on Task<Result<,>> and Result<,>
TeeOkAsync / TeeErrAsync (async action | CT-aware action)
CombineAsync (2, 3) / ValidateAsync (2, 3) / ZipAsync (2, 3) / PartitionAsync
Select / SelectMany on Task<Result<,>> (async LINQ)
```

### Option

```text
Opt.Some / Opt.None / Opt.FromNullable / Opt.FromNullableValue / Opt.When
Opt.Combine (2, 3) / Opt.Zip (2, 3) / Opt.Sequence / Opt.Traverse / Opt.Values
MapSome / TeeSome / TeeNone / Filter / MapSomeValue
BindSome / BindSomeNullable / BindSomeNullableValue / BindNone / BindNoneValue
Flatten / Match / OrThrow
RequireSome(error|factory) / ToOption / ToNullable / ToNullableValue / Transpose
AsTask / MapSomeAsync (3) / BindSomeAsync (3) / BindNoneAsync (2)
TeeSomeAsync (2) / TeeNoneAsync (2) / FilterAsync (sync|async predicate)
Select / SelectMany / Where
```

### Collection, terminals, parsing and the general union (1.1)

```text
TryFirst / TryLast / TryFind / Choose / Execute / ExecuteNoValue   (OptionEnumerableExtensions)
SuccessIf / FailureIf (value | factory)                          (Rop)
Rop.Parse<T> / Rop.TryParse<T>                                   (invariant culture, IParsable<T>)
Switch / SwitchAsync / SwitchFirst / MatchFirst                  (ResultTerminalExtensions)
FailIf / ThenEnsure                                              (ResultEnsureExtensions)
Either.Left / Either.Right / Match / MapLeft / MapRight / Swap / ToResult
```

## Anti-patterns

- `try/catch` around a combinator chain — lift the boundary with `Rop.Try` instead.
- Using `OrThrow` in production — exit with `Match`, `BindErrValue` or `TryGet*`.
- Using `null` to mean failure — use `Err`.
- Using `Option` for domain failure that carries a code/message — use `Result`.
- Wrapping a collection in `Option` — an empty collection is already empty.
- Mixing `Option` and `Nullable<T>` inside one public signature — convert once at the boundary.
- Branching on `Message` — branch on `Code`.
- Using `Validate` where a short-circuit is required — use `BindOk`.
- Mutating external state in `Tee*` and depending on its execution order — logging and metrics only.

## Targets, performance, and compatibility

- `net10.0`;`net11.0`, `LangVersion` latest, nullable reference types enabled.
- `IsAotCompatible`, trim/AOT analyzers enabled; no reflection, no dynamic code.
- `Result<,>` and `Option<>` are `readonly struct`s with value equality; they are immutable and safe to share
  across threads. Combinators do not lock; user callbacks are the caller's responsibility.
- Aggregations materialize once and expose `IReadOnlyList<T>` backed by a read-only wrapper.
- Async mapper overloads use `OverloadResolutionPriority` so `x => Task<T>` binds to the asynchronous overload
  without casts.

## Implementation notes

The library started from an early draft specification. The still-valid decisions (including C#-specific
corrections such as the `...Value` suffix for nullable value-type helpers) are recorded in
[`docs/adr/0001-core-design-decisions.md`](docs/adr/0001-core-design-decisions.md). API additions and the
rationale for capabilities that remain deliberately out of scope are recorded in
[`docs/adr/0002-api-gap-remediation.md`](docs/adr/0002-api-gap-remediation.md).

## Repository layout

```text
Sandlada.RopSharp/              library (net10.0; net11.0)
Sandlada.RopSharp.Tests/        TUnit suite (net10.0; net11.0)
Sandlada.RopSharp.Sample/       runnable multi-scenario tour (see its README)
Sandlada.RopSharp.Benchmarks/   BenchmarkDotNet baselines
docs/                           ADRs, benchmarks and the Traditional Chinese guide
CONTRIBUTING.md / SECURITY.md   contribution rules and vulnerability policy
```

Traditional Chinese guide: [`docs/zh-TW/getting-started.md`](docs/zh-TW/getting-started.md).
Benchmark baselines: [`docs/benchmarks.md`](docs/benchmarks.md).
Verification gates (including the trim-published run): [`docs/verification.md`](docs/verification.md).

## Contributing and security

- Contribution rules, design constraints and the public-API procedure: [CONTRIBUTING.md](CONTRIBUTING.md).
- Vulnerability reporting: [SECURITY.md](SECURITY.md) (private advisory; please do not open a public issue).

## License

MIT — see [LICENSE](LICENSE).
