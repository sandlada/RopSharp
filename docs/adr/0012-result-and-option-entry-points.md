# ADR 0012 — The entry points are named `Result` and `Option`

- **Status:** Accepted
- **Date:** 2026-10-31
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md), [ADR 0002](0002-api-gap-remediation.md),
  [ADR 0003](0003-option-async-parity.md)

## Context

The library shipped two static entry points that create the two value types:

- `Rop` created `Result<TOk, TErr>`;
- `Opt` created `Option<T>`.

The names were abbreviations of the product ("Rop" for railway-oriented programming, "Opt" for option), but they
worked against the shape of the API. A reader who saw `Rop.Ok(...)` had to know the library's history to guess that
the value being produced is a `Result`; a reader who saw `Opt.Some(...)` had to make the same jump for `Option`.
Every other type in the library already used the domain name (`Result`, `Option`, `Either`, `Unit`,
`ParseFailure`, `DiagnosticText`), so the entry points were the only names in the public surface that did not
describe what they return.

The rename is a breaking change. The package version stays at `0.0.1` (nothing has been published yet), so the
public API baseline is rewritten directly instead of being migrated through `PublicAPI.Unshipped.txt`.

## Decisions

### D27 — `Rop` becomes `Result`, `Opt` becomes `Option`

```csharp
Result.Ok(42)                  // was Rop.Ok(42)
Result.Err<int>(error)         // was Rop.Err<int>(error)
Result.Try(() => Charge())     // was Rop.Try(() => Charge())
Option.Some(address)           // was Opt.Some(address)
Option.None<Address>()         // was Opt.None<Address>()
Option.FromNullable(value)     // was Opt.FromNullable(value)
```

A non-generic class and a generic struct may share a name as long as their arity differs, so the namespace holds
`Result` and `Result<TOk, TErr>` next to each other, and `Option` and `Option<T>` next to each other. No alias,
no `[Obsolete]` shim and no third naming scheme is introduced: the old names are deleted, and the compiler
resolves the arity.

The source files follow the same rule: the entry points live in `Result.EntryPoints*.cs` and
`Option.EntryPoints*.cs`, next to the `Result.cs` and `Option.cs` value types.

### D28 — `RopError` and `RopErrorKind` keep their names

`RopError` is the structured error of the product. It is a value, not an entry point, and its name carries the
`RopSharp` lineage that the package, the repository and the organization all share. Renaming it to `Error` would
collide with `System.Exception`-style naming and would put the product lineage at risk for no consistency gain.
The type therefore keeps its name, and the entry points carry the domain names.

### D29 — CA1716 is suppressed for the `Option` entry point

Visual Basic reserves `Option` as a keyword, so `CA1716` fires on the `Option` class. The rule is suppressed with
a documented `[SuppressMessage]` on the type instead of weakening the analyzer configuration, because the name is
part of the published contract and the suppression is scoped to exactly this one type. The value type `Option<T>`
never triggered the rule.

### D30 — Earlier ADRs keep the old names

ADRs 0001 to 0011 are a record of the decisions as they were taken, so their code samples keep the `Rop` and `Opt`
names. This ADR is the reference for the current names. Living documentation (README, `docs/**` outside this
folder, the sample, the changelog) is updated to the new names.

## Consequences

- Every consumer must rename `Rop.` to `Result.` and `Opt.` to `Option.`; the compiler finds every call site,
  because the old names no longer exist.
- `Sandlada.RopSharp.PublicAPI.Shipped.txt` and `Sandlada.RopSharp.PublicAPI.Unshipped.txt` are rewritten with the
  new names.
- The package, the assembly and the namespace keep the `Sandlada.RopSharp` name; only the entry points move.
