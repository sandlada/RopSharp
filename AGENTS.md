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

## Commands

- Build: `dotnet build`
- Test (TUnit — do not introduce xUnit/NUnit/MSTest): `dotnet test`
- Single TUnit test: `dotnet test -- --treenode-filter "/*/*/<TestClass>/*"` (or `.../<TestClass>/<TestMethod>`).
  The legacy VSTest `--filter "FullyQualifiedName~..."` syntax is not available under Microsoft.Testing.Platform.
- Pack: `dotnet pack -c Release`

## Tests

- Framework is TUnit only. Load skill `csharp-tunit` before writing/editing tests.
- Cover C# call sites for new public API.
