# Benchmarks

Hot-path baselines for the two core types, produced with [BenchmarkDotNet](https://benchmarkdotnet.org/).
They exist to catch regressions and to verify the "immutable structs, no accidental allocation" promise.

## Environment

```text
BenchmarkDotNet 0.15.8
OS        : Windows 11 (10.0.26100) 24H2
CPU       : 12th Gen Intel Core i5-12500H 2.50 GHz (12 physical / 16 logical cores)
SDK       : .NET SDK 11.0.100-rc.1.26425.128
Host      : .NET 10.0.8, X64 RyuJIT x86-64-v3
Job       : ShortRun (WarmupCount=3, IterationCount=3, LaunchCount=1)
Diagnoser : MemoryDiagnoser
```

The benchmark project targets `net10.0` because BenchmarkDotNet's host runs the most mature installed runtime;
the library code compiled for `net11.0` is identical and is covered by the test suite.

`ShortRun` numbers are indicative and vary between runs (treat sub-nanosecond differences as noise). Allocation
counts are the stable signal.

## Result<TOk, TErr>

| Method | Mean | Ratio | Allocated | Notes |
| --- | ---: | ---: | ---: | --- |
| `MapOk_OnOk` | 2.54 ns | 1.00 | 0 B | success-track transform |
| `BindOk_OnOk` | 5.09 ns | 2.01 | 0 B | success-track expansion |
| `MapOk_OnErr` | 2.99 ns | 1.18 | 0 B | error passes through untouched |
| `MapErr_OnErr` | 12.64 ns | 4.99 | 48 B | `WithCode` creates a new immutable `RopError` |
| `Match_OnOk` | 1.00 ns | 0.39 | 0 B | terminal projection |
| `Validate_Two` | 0.57 ns | 0.22 | 0 B | aggregating success path; no error list allocated |
| `Sequence_16` | 70.24 ns | 20.51 | 240 B | short-circuit; materializes 16 values into a read-only view |
| `SequenceValidation_16` | 43.97 ns | 12.83 | 144 B | aggregating; pre-sized list, no spare capacity |
| `Partition_16` | 65.08 ns | 18.99 | 328 B | two pre-sized lists plus two read-only views |

## Option<T>

| Method | Mean | Ratio | Allocated |
| --- | ---: | ---: | ---: |
| `MapSome_OnSome` | 0.62 ns | 1.00 | 0 B |
| `BindSome_OnSome` | 0.73 ns | 1.21 | 0 B |
| `MapSome_OnNone` | 0.46 ns | 0.75 | 0 B |
| `Filter_OnSome` | 0.55 ns | 0.90 | 0 B |
| `Match_OnSome` | 0.75 ns | 1.24 | 0 B |

## Allocation history

| Change | `Sequence_16` allocated |
| --- | ---: |
| 1.0.0 initial (`List` → `ToArray` → `ReadOnlyCollection`) | 328 B |
| 1.0.0 after exposing an exactly-sized list directly | 240 B |
| 1.0.0 `SequenceValidation_16` with count-aware pre-sizing | 144 B |

`Rop.ToReadOnlyList` now wraps the list when `Capacity == Count` (the common case for small aggregations and
error lists) and copies only when the list has spare capacity, so a grown list's slack is never retained by the
exposed view. APIs that always materialize (`SequenceValidation`, `Partition`, `Opt.Values`) pre-size their lists
from `ICollection<T>.Count`; short-circuit APIs (`Sequence`, `Traverse`, `Opt.Sequence`) deliberately do not, so an
early error does not allocate for the full input.

## Interpretation

- Pure combinators do not allocate: `Result`/`Option` are structs, delegates are cached static lambdas in the
  benchmark code, and no defensive copies escape.
- `MapErr` allocating 48 B is expected: `RopError` is an immutable class, and `WithCode` returns a new instance.
  Error paths are not the hot path; when they are, reuse a pre-built `RopError`.
- Aggregation on the success path (`Validate_Two`) does not allocate an error list. The failure path allocates the
  list plus a read-only wrapper, which is the documented cost of collecting every error.
- `SequenceValidation_16` pre-sizes to the source count: 144 B and no list growth. `Partition_16` allocates two
  pre-sized lists plus two read-only views.
- `Sequence_16` pays for materialization (list + read-only wrapper) without pre-sizing, because an early error must
  not allocate for the whole input. The API contract is "enumerate once, immediately".

Short-run numbers are indicative, not a substitute for a full BenchmarkDotNet run on the target hardware.
Run the full job when evaluating a performance change:

```bash
dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter "*"
```

To compare two revisions, use `--filter "*" --join` on a saved results directory, or run the project on both
commits.
