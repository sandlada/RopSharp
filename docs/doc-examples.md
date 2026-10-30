# Documentation examples

Every public member of `Sandlada.RopSharp` carries a worked example in its XML documentation. This guide
describes how those examples are written, why they look the way they do, and which tests keep them honest.

## Why the examples look the way they do

A single-line example such as `result.MapOk(x => x.Id)` teaches nothing: the reader cannot tell what the API is
for, where it fits in a pipeline, or what the failure track produces. Every example in this library is therefore
written as a small fragment of a **real pipeline** that

1. composes the documented API with at least one other API of the library,
2. states the outcome of every track in a trailing comment, and
3. is a self-contained, compilable fragment.

The examples are the primary usage documentation of the package, so they are treated as code, not as prose.

## The shared storyline

All examples draw from a single "order fulfillment" storyline:

| Type | Role in the storyline |
| --- | --- |
| `Customer` | The buyer. The shipping address is optional. |
| `Address` | A postal address. |
| `Order` | The order to fulfill, with its lines. |
| `OrderLine` | One ordered line, with a derived `Amount`. |
| `Payment` | A payment attempt against an order. |
| `PaymentStatus` | The lifecycle state of a payment. |
| `Shipment` | A parcel handed over to a carrier. |
| `OrderFixtures` | Deterministic stand-ins for the external systems. |

Those types live in
[`Sandlada.RopSharp.Tests/DocExamples/DocExampleDomain.cs`](../Sandlada.RopSharp.Tests/DocExamples/DocExampleDomain.cs).
They are compiled twice: once into the test assembly, and once - verbatim - as an extra syntax tree inside the
example compilation, so the snippets are type checked against the very same declarations the documentation
mentions.

Because every example uses the same domain, a reader who moves from `Rop.Try` to `ResultExtensions.MapOk` to
`Rop.Aggregation.Validate` keeps watching one program grow instead of a series of unrelated fragments.

## The template

```csharp
/// <summary>
/// One or two sentences describing what the member does.
/// </summary>
/// <remarks>
/// <para>
/// Why the member exists and where it belongs in a pipeline. Cross references use <see cref="..."/>.
/// </para>
/// <code><![CDATA[
/// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
///     .MapOk(static payment => payment.Id);
///
/// // Ok(receiptId) when the payment provider answered,
/// // Err(E_CHARGE) when the provider threw - the mapper is never evaluated on the error track.
/// ]]></code>
/// </remarks>
```

### Rules

- **The example lives in `<remarks>`, inside `<code><![CDATA[ ... ]]></code>`.** The CDATA wrapper keeps `<`,
  `>` and `&` readable, so `Option<T>` and `=>` do not have to be escaped. `<see cref="..."/>` outside the
  CDATA block is still escaped normally.
- **No isolated calls.** The fragment must reference at least two distinct members of the library, so the
  documented API is always seen in the company it keeps.
- **The outcome is spelled out.** A `//` comment names what the fragment produces on every track. For `Result`
  that is `Ok(...)` and `Err(...)`, for `Option` it is `Some(...)` and `None`, for `Either` it is `Left(...)` and
  `Right(...)`.
- **The fragment is self-contained.** Every local variable is declared inside the fragment; no name may refer to
  something the reader cannot see.
- **No `return`.** Fragments are compiled as method bodies, so they bind their results to a local variable
  instead of returning them.
- **Fragments that await are compiled as `async` methods.** A fragment containing `await` is automatically
  hosted in an `async Task` method.

### Length tiers

Not every member deserves a long story, but every member deserves an example.

| Tier | Applies to | Typical length |
| --- | --- | --- |
| A | Combinators, boundaries, aggregation, bridges, terminals | 15-30 lines |
| B | Properties, probes, enum values, conversions | 6-12 lines |
| C | `Equals`, `GetHashCode`, `ToString`, operators | 3-5 lines |

A tier C example is short but still real: it shows the member in the middle of a working pipeline instead of
isolating it.

## The guardrails

Two test classes keep the documentation honest.

### `DocExampleCompilationTests`

Extracts every `<code><![CDATA[ ... ]]></code>` fragment from the library sources and compiles all of them in a
single Roslyn compilation against the real `Sandlada.RopSharp.dll` and the runtime reference assemblies. A
fragment that does not compile fails the build with the offending file, line and diagnostic.

The same class enforces the composition rule (at least two distinct library members per fragment), the
track-comment rule (at least two track markers such as `Ok(`, `Err(`, `Some(`, `None`, `Left(`, `Right(` in a
comment) and the no-`return` rule.

### `DocExampleCoverageTests`

Enumerates the public surface with Roslyn - types, methods, every overload, properties, operators, constructors
and enum values - and fails when a member has no example attached. It also verifies that every `<code>` block is
properly wrapped in CDATA.

## Running the checks

```bash
dotnet build
dotnet test -- --treenode-filter "/*/*/DocExample*/*"
```

Run them after every file you touch. The tests are the specification; the prose in this guide explains the
intent behind them.
