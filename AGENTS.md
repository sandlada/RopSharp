# AGENTS.md — RopSharp (Sandlada.RopSharp)

NuGet ROP / Railway & Result library for C#. Package ID: `Sandlada.RopSharp`, org: `Sandlada`.

## Targets — do not downgrade

- `net10.0;net11.0` only (`net11.0` primary). Never add `net8.0`/`net9.0` or earlier.
- `LangVersion: latest`, C# 15 syntax allowed/expected. Keep nullable-reference-types enabled.
- Public API is C#-only. F# support is dropped.

## API style — functional-first

- Prefer higher-order functions, curried / partial-application-friendly shapes, data-last parameter order (e.g. `func`/`binder`/`mapper` last) so chaining works.
- `Result`/`Option`-style types: no throwing on expected failure paths, no `null` returns on public surface. C# overloads allowed.
- Keep core types allocation- and AOT-friendly; avoid reflection in hot paths.

## Public entry points

- The static entry points are `Result` (creates `Result<TOk, TErr>`) and `Option` (creates `Option<T>`). The former
  names `Rop` and `Opt` are gone — see [ADR 0012](docs/adr/0012-result-and-option-entry-points.md).
- A non-generic class and a generic struct may share a name because their arity differs: `Result`/`Result<TOk, TErr>`
  and `Option`/`Option<T>` coexist in the `Sandlada.RopSharp` namespace.
- `RopError` and `RopErrorKind` keep their names: they carry the product lineage, not an entry-point role.

## Public API baseline

- `Sandlada.RopSharp/PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` are enforced by
  `Microsoft.CodeAnalysis.PublicApiAnalyzers`. Every public API change must update them in the same commit.
- A breaking change (removal, rename, signature change) must be recorded in `CHANGELOG.md` and, when it changes a
  decision, in a new ADR or by revising the existing ADR in place (add a `- **Revised:** YYYY-MM-DD` line under the
  title; do not leave two contradictory ADR copies).

## Commands

- Build: `dotnet build`
- Test (TUnit — do not introduce xUnit/NUnit/MSTest): `dotnet test`
- Single TUnit test: `dotnet test -- --treenode-filter "/*/*/<TestClass>/*"` (or `.../<TestClass>/<TestMethod>`).
  The legacy VSTest `--filter "FullyQualifiedName~..."` syntax is not available under Microsoft.Testing.Platform.
- Pack: `dotnet pack -c Release`

## Tests

- Framework is TUnit only. Load skill `csharp-tunit` before writing/editing tests.
- Cover C# call sites for new public API.
