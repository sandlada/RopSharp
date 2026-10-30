# Sandlada.RopSharp

Production-grade railway-oriented programming (ROP) for modern C#.

`Sandlada.RopSharp` gives you immutable structs — `Result<TOk, TErr>`, `Option<T>` and `Either<TLeft, TRight>` —
together with a structured `RopError`, applicative validation, parsing helpers, and cancellation-aware async
combinators. Expected failures are values, never exceptions. The public surface is fully symmetric, has no
aliases, performs no reflection, and is AOT/trim friendly.

```csharp
using Sandlada.RopSharp;

// An order is priced, charged and finally shipped. Every failure is a value.
Result<string, RopError> receipt =
    Rop.Try(() => ChargeAsync(order), "E_CHARGE")
       .BindOk(payment => Rop.Ok(payment.Id))
       .Ensure(id => id.Length > 0, RopError.Create("E_RECEIPT", "the receipt id is empty"))
       .TeeOk(id => Log(id))
       .MapErr(e => e.WithCode("E_PAYMENT"));

string text = receipt.Match(
    ok: id => $"receipt {id}",
    err: e => $"fail {e.Code}: {e.Message}");
```

- **Targets:** `net10.0` and `net11.0` only.
- **Package:** `Sandlada.RopSharp` (MIT).
- **Dependencies:** none.

## The order fulfillment storyline

Every example in this repository — in the README, in the XML documentation of the library and in the Traditional
Chinese guide — is drawn from one small domain, so a reader keeps watching the same program grow instead of a
series of unrelated fragments:

| Type | Role |
| --- | --- |
| `Customer` | The buyer. The shipping address is optional, so the absence path can be shown. |
| `Address` | A postal address. |
| `Order` | The order to fulfill, with its lines. `Total` and `ItemCount` are derived. |
| `OrderLine` | One ordered line, with a derived `Amount`. |
| `Payment` | A payment attempt against an order. |
| `PaymentStatus` | The lifecycle state of a payment. |
| `Shipment` | A parcel handed over to a carrier. |

The types live in
[`Sandlada.RopSharp.Tests/DocExamples/DocExampleDomain.cs`](Sandlada.RopSharp.Tests/DocExamples/DocExampleDomain.cs)
and are compiled into the test assembly. `OrderFixtures` stands in for the external systems: `Charge` throws for a
declined order, so a boundary built on top of it shows both tracks deterministically.

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
    Opt.FromNullable(order.Customer.Address)
       .BindSome(shipping => Opt.Some(shipping))
       .RequireSome(() => RopError.Create("E_ADDRESS", "the customer has no shipping address"));
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
Option<Address> some = Opt.Some(order.Customer.Address!);
Option<Address> none = Opt.None<Address>();

Address? maybeAddress = null;
Option<Address> lifted = Opt.FromNullable(maybeAddress);          // reference types
Option<int>    count   = Opt.FromNullableValue(maybeItemCount);   // Nullable<T>

string city = some.Match(some: a => a.City, none: () => "unknown");

// default(Option<T>) is contractually None — safe in arrays, fields and generic containers
Option<Order>[] slots = new Option<Order>[3];                      // all None
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
| Aggregating combination | `Rop.Validate`, `Rop.SequenceValidation`, `Rop.TraverseValidation` |
| Explicit model conversion | `ToValidation` / `ToSingleError` / `RequireSome` / `ToOption` / `ToEither` / `Transpose` |

Every combinator is **data-last** (mapper/binder/predicate/action last), so chains read left to right.

### Model bridging

Crossing between the three models is always explicit, and every bridge names which side becomes which:

```csharp
Either<RopError, Payment> fromResult = Rop.Ok(payment).ToEither();      // Ok -> Right, Err -> Left
Result<Payment, RopError> back      = fromResult.ToResult(error => error); // Left -> Err, Right -> Ok
Either<string, Payment>  fromOption = Opt.None<Payment>().ToEither("missing"); // None -> Left, Some -> Right
Option<Payment>          present    = Either.Right<string, Payment>(payment).ToOption(); // Left is discarded
```

`ToOption` on `Result` and `Either` deliberately drops the reason; use `RequireSome` or `ToResult` when it must
survive the crossing.

## Error model

`RopError` is immutable and structured:

```csharp
RopError error = RopError.Create(
    code: "E_ORDER_NOT_FOUND",     // stable, machine-readable, safe to switch on
    message: "order 42 does not exist", // human-readable, display only
    details: "id=42");            // optional machine/diagnostic payload

RopError fromBoundary = RopError.FromException(exception, "E_IO"); // Cause is captured here only
RopError retagged     = error.WithCode("E_ORDER_404");

RopError aggregate = RopError.Combine([notFound, outOfStock]);
IReadOnlyList<RopError> leaves = aggregate.InnerErrors; // the originals, in supplied order
```

- `Code` and `Message` must be non-blank; blank input is a programming error (`ArgumentException`).
- `Details` may be `null` but must not be blank.
- `Kind` is a coarse category (`Failure`, `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`,
  `Unexpected`, `Timeout`, `Unavailable`, `RateLimited`) for transport/presentation mapping; branch on `Code` for
  precise domain decisions. New values are appended, so treat the enum as open-ended.
- `InnerErrors` is never `null`: atomic errors expose an empty list, while a `Combine` aggregate keeps every input in
  order (no flattening) so nothing is lost by collapsing. `WithCode` and the other `With*` methods preserve it.
- Equality compares `Kind`, `Code`, `Message` and `Details`; `Cause` and `InnerErrors` are excluded so stack-trace
  noise and structural differences cannot make two equally reported errors unequal.
- `RopError.Combine(errors)` collapses a non-empty list into `E_MULTIPLE`; a uniform `Kind` is preserved and mixed
  kinds fall back to `Failure`. A single-element call returns that error unchanged instead of wrapping it.

Recommended code convention: `E_<DOMAIN>[_<REASON>]`, uppercase with underscores.

### Custom error types

`Result<TOk, TErr>` is generic; `RopError` is the recommended default, not a constraint. Every combinator,
aggregation helper and LINQ operator already works with any `TErr`. The convenience and boundary helpers have
`TErr` overloads so a domain error type can be used end to end:

```csharp
public sealed record AppError(string Code, string Message);

Result<decimal, AppError> parsed =
    Rop.Parse<decimal, AppError>(raw, failure => new AppError(failure.Code, failure.Message));

Result<Order, AppError> loaded =
    Rop.Try(() => LoadOrder(id),
            ex => new AppError("E_IO", ex.Message),
            () => new AppError("E_NULL", "the loader returned null"));
```

- The `RopError` overloads remain the default and keep their `E_NULL_RESULT` / `(func, code)` behavior.
- `Try` / `TryAsync` / `TrySwitch` / `TrySwitchAsync` require an explicit null-result factory for
  value-producing boundaries, because the library cannot synthesize an arbitrary error. Action boundaries do
  not need one.
- `Parse` passes a `ParseFailure` (`Code`/`Message`/`Input`/`TypeName`/`Details`) to the factory, so the
  bounded, escaped diagnostics survive.
- `ToSingleError()` takes the first error; use `ToSingleError(combine)` for a custom collapse.

Caveats: `Rop.Err` rejects `null` error values; `default(Result<,>)` is undefined; `Result` equality delegates
to `EqualityComparer<TErr>.Default` (records and structs compare by value, plain classes by reference); `TErr`
is inlined in the struct, so prefer a reference type or a small struct. A custom error type also gives up
`Code`/`Kind`/`InnerErrors`; model what you need on the type itself.

## Short-circuit vs aggregating

The two models are deliberately separate and never mix implicitly:

```csharp
// Short-circuit: first error wins, later functions are not executed
Result<Order, RopError> order =
    Rop.Ok(raw)
       .BindOk(CheckCustomer)     // stops here on failure
       .BindOk(CheckAmount);

// Aggregating: every input is inspected, all errors are collected in argument order
Result<Order, IReadOnlyList<RopError>> validated =
    Rop.Validate(CheckCustomer(raw), CheckAmount(raw), CheckStock(raw),
                 (customer, amount, stock) => new Order("ord-1001", customer, stock));

// Explicit conversions between the tracks
Result<Order, IReadOnlyList<RopError>> lifted = order.ToValidation();
Result<Order, RopError> single = validated.ToSingleError();               // first error
Result<Order, RopError> merged = validated.ToSingleError(RopError.Combine);
```

`Sequence`, `Traverse` and `Partition` accept any `IEnumerable<T>` but materialize exactly once. Empty input
yields `Ok([])` (or `Some([])` for options), never `null`. `SequenceValidation` and `TraverseValidation` share
those collection rules but aggregate: every element is mapped and every error is returned in element order.

## Async and cancellation

The map/bind/tee combinators have `...Async` counterparts for both `Result` and `Option`. The token is optional
and always last; cancellation is reported as `OperationCanceledException` and never converted into an error,
because errors describe domain failures.

```csharp
Task<Result<string, RopError>> chained =
    ChargeAsync(order, ct)
        .MapOkAsync(payment => payment.Id, ct)
        .BindOkAsync(id => GetShipmentAsync(id, ct), ct);

// Cancellation-aware mapper receives the same token; the library never creates its own CTS
Task<Result<string, RopError>> ct2 =
    ChargeAsync(order, ct).MapOkAsync((payment, token) => FetchReceiptAsync(payment, token), ct);
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

The aggregating traversal helpers keep their no-short-circuit contract on the async track:
`Rop.SequenceValidationAsync` and `Rop.TraverseValidationAsync` (plain and cancellation-aware mapper) await every
task and return every error in source order. A faulted task surfaces as an exception rather than an error, and when
several tasks fault the first fault wins while the rest are still observed.

```csharp
Result<IReadOnlyList<OrderLine>, IReadOnlyList<RopError>> validated =
    await Rop.TraverseValidationAsync(order.Lines, (line, token) => ValidateLineAsync(line, token), ct);

// Same aggregation without async work
Result<IReadOnlyList<OrderLine>, IReadOnlyList<RopError>> sync =
    Rop.TraverseValidation(order.Lines, ValidateLine);
```

`Option` mirrors that surface for map/bind/tee/filter, aggregation and LINQ. A synchronous `Option<T>` can be
lifted directly with the async
`MapSomeAsync` / `BindSomeAsync` / `BindNoneAsync` / `TeeSomeAsync` / `TeeNoneAsync` / `FilterAsync`
overloads; `Opt.CombineAsync` / `ZipAsync` await every input, while `Opt.SequenceAsync` / `TraverseAsync`
await in order and stop at the first `None`. `Task<Option<T>>` supports query syntax through
`Select` / `SelectMany` / `Where`:

```csharp
Option<string> city = await GetAddressAsync(customer.Id, ct)
    .MapSomeAsync(address => address.City, ct)
    .FilterAsync(value => value.Length > 0, ct);
```

## Boundaries

Exceptions become domain values in exactly one place:

```csharp
Result<decimal, RopError> parsed = Rop.Try(() => decimal.Parse(text), ex => RopError.FromException(ex, "E_PARSE"));
Result<decimal, RopError> byCode = Rop.Try(() => decimal.Parse(text), code: "E_PARSE");

// Side-effecting boundaries keep the result composable via Unit
Result<Unit, RopError> saved = Rop.Try(() => repository.Save(order), ex => RopError.FromException(ex, "E_SAVE"));
Task<Result<Unit, RopError>> sent = Rop.TryAsync(() => gateway.SendAsync(order, ct), "E_SEND", ct);

Result<Address, RopError> required = Rop.FromNullable(customer.Address, () => RopError.Create("E_ADDRESS", "the customer has no shipping address"));
Result<int, RopError> requiredCount = Rop.FromNullableValue(maybeItemCount, () => RopError.Create("E_COUNT", "the order has no item count"));
```

- `OperationCanceledException` is always rethrown, never converted.
- A boundary function returning `null` yields `Err` with code `E_NULL_RESULT` — an `Ok(null)` is never produced
  by `Try`.
- `Rop.Switch` / `Rop.TrySwitch` lift a plain function without and with exception conversion; `Rop.SwitchAsync` /
  `Rop.TrySwitchAsync` are the awaited counterparts. They adapt a function, they do not consume an existing result
  (that is `Switch`/`SwitchAsync` on `Result`).
- `Rop.Try(Action, ...)` and `Rop.TryAsync(Func<Task>, ...)` cover boundaries with no payload; they return
  `Result<Unit, RopError>` so the outcome still flows through the normal combinators.
- Do not pass a `Task`-returning lambda to the synchronous `Rop.Try<T>`: the `Task` would become the success payload.
  Use `Rop.TryAsync` for asynchronous work.

## Parsing

`Rop.Parse<T>` and `Rop.TryParse<T>` use the invariant culture and the static abstract `IParsable<T>` contract.
A parse failure echoes the offending input in the message and in `Details` as `input=<quoted>`, so the error is
self-describing without logging the raw payload separately.

```csharp
Result<decimal, RopError> amount = Rop.Parse<decimal>(raw, "E_AMOUNT");
// Err: Code=E_AMOUNT, Message='19,50' is not a valid Decimal., Details=input="19,50"
```

The echo is bounded to 64 characters, truncates on a code-point boundary, and escapes quotes, backslashes and
control characters. `null` renders as the literal `null` instead of an empty pair of quotes, so "missing" and
"empty" stay distinguishable.

## LINQ

```csharp
// Result: select + from (no where — there is no error value to produce for a predicate)
var total = from customer in GetCustomer(id)
            from order in GetOrder(customer.Id)
            select order.Total;

// Option: select + from + where (where is just Filter)
var total = from address in Opt.FromNullable(customer.Address)
            from line in Opt.Some(order.Lines[0])
            where line.Quantity > 0
            select line.Amount;
```

## Nullability rules

- `Rop.Ok(null)`, `Rop.Err(null)`, `Opt.Some(null)` throw `ArgumentNullException` (input guards, programming errors).
- A mapper is allowed to produce a nullable success payload: `Result<string?, RopError>`. Enforce non-nullness with
  `RequireNotNull`; use `Option` when absence is expected.
- `Option<T>` never contains `null`; map nullable results with `BindSomeNullable` / `BindSomeNullableValue`.
- `ToNullable` is the only place the library hands out `null` for reference types; cross back with
  `Opt.FromNullable` at the consuming boundary.
- Collection helpers treat `null` as a programming error: `TryFirst`, `TryLast`, `TryFind` and the projecting
  `Choose` throw `ArgumentNullException` when the element, stored value or mapped value is `null`, instead of
  producing `Some(null)` or a null list entry. Filter the sequence or lift with `Opt.FromNullable` first.

```csharp
Result<Address, RopError> address =
    GetOrder(id)
        .MapOk(o => o.Customer.Address)                          // Address?
        .RequireNotNull(() => RopError.Create("E_ADDRESS", "the customer has no shipping address"));
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

### Scope decisions

Three larger topics are deliberately caller-owned (ADR 0010):

- **Arity stops at three.** `Combine`/`Zip`/`Validate` cover 2 and 3 inputs; larger aggregations use a record, a
  collection with `SequenceValidation`/`TraverseValidation`, or nested calls.
- **Concurrency policy stays at the call site.** The async helpers are sequential or await-all; for bounded fan-out
  compose `Task.WhenAll`/`Parallel.ForEachAsync` at the boundary. The library never throttles or creates a token
  source.
- **`IAsyncEnumerable<T>` is composed, not wrapped.** Materialize with `await foreach` and pass the results to
  `Rop.SequenceAsync`/`SequenceValidationAsync` (or `Opt.SequenceAsync`).

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
Rop.Traverse(source, mapper)                  Rop.TraverseValidation(source, mapper)
Rop.Partition(results)
ToValidation() / ToSingleError() / ToSingleError(combine)
Rop.Try(func, errorFactory) / Rop.Try(func, code)
Rop.Try(action, errorFactory) / Rop.Try(action, code)                 -> Result<Unit, RopError>
Rop.TryAsync(func, errorFactory, ct) / Rop.TryAsync(func, code, ct)
Rop.TryAsync(action, errorFactory, ct) / Rop.TryAsync(action, code, ct) -> Task<Result<Unit, RopError>>
Rop.Switch(func) / Rop.TrySwitch(func, errorFactory)
Rop.SwitchAsync(func) / Rop.TrySwitchAsync(func, errorFactory)
Rop.FromNullable(value, errorFactory) / Rop.FromNullableValue(value, errorFactory)
Rop.SequenceAsync(tasks, ct) / Rop.TraverseAsync(source, mapper, ct)
Rop.SequenceValidationAsync(tasks, ct) / Rop.TraverseValidationAsync(source, mapper, ct)
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
SequenceValidationAsync / TraverseValidationAsync (async | CT-aware mapper)
Select / SelectMany on Task<Result<,>> (async LINQ)
```

### Option

```text
Opt.Some / Opt.None / Opt.FromNullable / Opt.FromNullableValue / Opt.When
Opt.Combine (2, 3) / Opt.Zip (2, 3) / Opt.Sequence / Opt.Traverse / Opt.Values
Opt.CombineAsync (2, 3) / Opt.ZipAsync (2, 3)
Opt.SequenceAsync / Opt.TraverseAsync (async | CT-aware mapper)
MapSome / TeeSome / TeeNone / Filter / MapSomeValue
BindSome / BindSomeNullable / BindSomeNullableValue / BindNone / BindNoneValue
Flatten / Match / OrThrow
RequireSome(error|factory) / ToOption / ToEither(value|factory) / ToNullable / ToNullableValue / Transpose
AsTask / MapSomeAsync (sync on Task; async | CT-aware on Option and Task)
BindSomeAsync (async | CT-aware) / BindNoneAsync (async | CT-aware)
TeeSomeAsync (2) / TeeNoneAsync (2) / FilterAsync (sync | async | CT-aware)
Select / SelectMany / Where (Option and Task sources)
```

### Collection, terminals, parsing and the general union (1.1)

```text
TryFirst / TryLast / TryFind / Choose / Execute / ExecuteNoValue   (OptionEnumerableExtensions)
SuccessIf / FailureIf (value | factory)                          (Rop)
Rop.Parse<T> / Rop.TryParse<T>                                   (invariant culture, IParsable<T>)
Switch / SwitchAsync / SwitchFirst / MatchFirst                  (ResultTerminalExtensions)
FailIf / ThenEnsure                                              (ResultEnsureExtensions)
Either.Left / Either.Right / Match / MapLeft / MapRight / Swap / ToResult / ToOption
Result<,>.ToEither / Option<>.ToEither (value | factory)
```

### Custom error types

Every entry below keeps an identical `RopError` overload; the `TErr` form is additive.

```text
FromNullable<TOk,TErr>(value, errorFactory)            FromNullableValue<TOk,TErr>(value, errorFactory)
SuccessIf<T,TErr>(condition, value, error|factory)     FailureIf<T,TErr>(condition, value, error|factory)
RequireNotNull<T,TErr>(errorFactory)                   Either.ToResult<TErr>(errorFactory)
ToSingleError<T,TErr>()                                ToSingleError<T,TErr>(combine)
Try<T,TErr>(func, errorFactory, nullResultErrorFactory)
Try<TErr>(action, errorFactory)
TryAsync<T,TErr>(func, errorFactory, nullResultErrorFactory, ct)
TryAsync<TErr>(action, errorFactory, ct)
TrySwitch<T,TOut,TErr>(func, errorFactory, nullResultErrorFactory)
TrySwitchAsync<T,TOut,TErr>(func, errorFactory, nullResultErrorFactory)
Parse<T,TErr>(value, Func<ParseFailure,TErr>, code = "E_PARSE")
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
[`docs/adr/0002-api-gap-remediation.md`](docs/adr/0002-api-gap-remediation.md). Option asynchronous parity is
recorded in [`docs/adr/0003-option-async-parity.md`](docs/adr/0003-option-async-parity.md), validation traversal
in [`docs/adr/0004-validation-traversal.md`](docs/adr/0004-validation-traversal.md), lossless error aggregation in
[`docs/adr/0005-lossless-error-aggregation.md`](docs/adr/0005-lossless-error-aggregation.md), asynchronous
boundaries in [`docs/adr/0006-async-boundaries.md`](docs/adr/0006-async-boundaries.md), diagnostics in
[`docs/adr/0007-diagnostics.md`](docs/adr/0007-diagnostics.md), model bridging in
[`docs/adr/0008-model-bridges.md`](docs/adr/0008-model-bridges.md), the null-element policy in
[`docs/adr/0009-null-elements.md`](docs/adr/0009-null-elements.md), the concurrency/streaming scope in
[`docs/adr/0010-concurrency-and-streaming-scope.md`](docs/adr/0010-concurrency-and-streaming-scope.md), and
custom error types in [`docs/adr/0011-custom-error-types.md`](docs/adr/0011-custom-error-types.md).

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
Documentation-example rules (how the XML examples are written and kept compiling):
[`docs/doc-examples.md`](docs/doc-examples.md).
Benchmark baselines: [`docs/benchmarks.md`](docs/benchmarks.md).
Verification gates (including the trim-published run): [`docs/verification.md`](docs/verification.md).

## Contributing and security

- Contribution rules, design constraints and the public-API procedure: [CONTRIBUTING.md](CONTRIBUTING.md).
- Vulnerability reporting: [SECURITY.md](SECURITY.md) (private advisory; please do not open a public issue).

## License

MIT — see [LICENSE](LICENSE).
