# Benchmarks

Micro-benchmarks for the library's hot paths, produced with [BenchmarkDotNet](https://benchmarkdotnet.org/).

They serve two different purposes, and the split matters:

| Signal | Where it is enforced | Why |
| --- | --- | --- |
| Allocation (bytes) | test suite, `Sandlada.RopSharp.Tests/PerformanceGuards/` | deterministic, fails the build on regression |
| Time (ns) | this document, run manually or nightly | noisy on shared runners, tracked as a trend only |

Allocation is the gate. Time is informational. Never gate a pull request on a nanosecond threshold from a shared
runner; see [docs/performance-budgets.md](performance-budgets.md) for the allocation budgets and how to change them.

## Method

- Every benchmark class applies `[Config(typeof(BenchConfig))]`. The job, diagnoser, exporters and validators are
  defined in code, so a run never depends on command-line flags.
- `BenchConfig` pins `Job.ShortRun` (3 warmup, 3 iterations, 1 launch), attaches `MemoryDiagnoser`, exports
  JSON (full-compressed) and GitHub-flavored Markdown, and fails the run when the build is not optimized
  (`JitOptimizationsValidator`) or when a parameter group does not have exactly one baseline (`BaselineValidator`).
- `EnvironmentProvenance` prints the commit hash, runtime, OS, architecture and timestamp before the run, so every
  result set can be traced back to its source.
- Methods whose duration is indistinguishable from an empty method use `OperationsPerInvoke` with an internal loop.
  Without it BenchmarkDotNet reports `ZeroMeasurement` and the number is meaningless.
- `[Params]` turns a fixed size into data points. `Sequence`, `Opt.Sequence`, `Opt.Values`, `RopError.Combine` and
  `RopError` aggregation are measured at several sizes so that a change from linear to quadratic is visible.

### Hand-written baselines
Each group contains one `Baseline = true` method that is a hand-written implementation with the same observable
result. The `Ratio` column then answers "what does the abstraction cost compared to writing it out".
`RopErrorBenchmarks.Combine_CopyOnly_LowerBound` is the one exception: it only copies the inputs, so it is a lower
bound on the aggregation cost, not an equivalent implementation.

### Corrections found while building the suite

Two harness defects were found and fixed before the numbers in this document were published. They are recorded here
because they show why the numbers are trustworthy:

- `ParsingBenchmarks.Parse_Int_Success` originally parsed a 64-character string of `'7'`. That value is `7.7e18`, which
  overflows `Int32`, so the "success" case was silently measuring the **failure** path: 763 ns and 1328 B per call.
  With a genuinely valid input (`"2147483647"`) the same call takes 21.6 ns and allocates **0 B**. The allocation was
  the error object, the escaped message and the quoted input, not the parsing.
- Methods whose duration is indistinguishable from an empty method produce a meaningless number and a `ZeroMeasurement`
  warning. They now run 1000 iterations per invocation with `OperationsPerInvoke`, which is what makes the equality,
  hashing and `Option` numbers in this document readable.


## Running

```bash
# everything
dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter "*Benchmarks*"

# one class (the filter is a single glob; it cannot be repeated)
dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter "*RopErrorBenchmarks*"
```

Results are written to `artifacts/benchmarks/`:

```text
artifacts/benchmarks/
  <class>-<timestamp>.log                     console transcript
  results/<class>-report-full-compressed.json machine-readable results
  results/<class>-report-github.md            table for pasting into this document
  results/<class>-report.csv                  flat table
  results/<class>-report.html                 rendered report
```

### Comparing two revisions

Run the benchmark on both commits and compare the exported Markdown tables. Because the job, parameters and
exporters are pinned in code, the two runs are directly comparable.

```bash
git stash                       # or check out the other revision
dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter "*Benchmarks*"
```

When the working tree is already on the target commit, `BenchmarkDotNet` can compare against a previous run stored
in a results directory:

```bash
dotnet run -c Release --project Sandlada.RopSharp.Benchmarks -- --filter "*Benchmarks*" --join artifacts/benchmarks/results
```

### Trend history

The nightly workflow stores each run in the `gh-pages` branch and renders a trend chart. Time regressions raise an
alert but do not fail the workflow.

## Coverage

| Class | What it covers |
| --- | --- |
| `ResultBenchmarks` | `MapOk`, `BindOk`, `MapErr`, `Match`, `Validate` on both tracks |
| `OptionBenchmarks` | `MapSome`, `BindSome`, `Filter`, `Match` on both tracks |
| `EitherBenchmarks` | `MapRight`, `Swap`, `Match`, `ToResult` |
| `SequenceBenchmarks` | `Sequence` (success, short-circuit), `SequenceValidation` (all-error, mixed), `Partition`, at N = 16 / 256 / 4096 |
| `CollectionBenchmarks` | `Opt.Values`, `Opt.Sequence`, `Opt.Traverse`, `ToSingleError`, at N = 16 / 256 / 4096 |
| `RopErrorBenchmarks` | `RopError.Combine` at N = 1 / 10 / 100 / 1000 |
| `ParsingBenchmarks` | `Rop.Parse` / `Rop.TryParse` success, failure, long input, `Guid`, `DateTime` |
| `AsyncBenchmarks` | `MapOkAsync`, `BindOkAsync`, `MapErrAsync` on completed and pending tasks |
| `EqualityBenchmarks` | `Equals`, `GetHashCode`, `ToString` for all three core types |

## Environment

```text
BenchmarkDotNet 0.15.8
Job       : ShortRun (WarmupCount=3, IterationCount=3, LaunchCount=1)
Diagnoser : MemoryDiagnoser
```

The benchmark project targets `net10.0` because BenchmarkDotNet's host runs the most mature installed runtime;
the library code compiled for `net11.0` is identical and is covered by the test suite.

`ShortRun` numbers vary between runs. Treat sub-nanosecond differences as noise, and use the standard deviation
reported next to each mean to judge whether a difference is real. Allocation counts are the stable signal.

## Results

The tables below are pasted from the exported Markdown reports. Regenerate them after a run with:

```bash
pwsh -File tools/Update-BenchmarkTables.ps1
```

The script replaces the block between the markers below with the tables from
`artifacts/benchmarks/results/*.md`, so the document is never hand-copied.

### Provenance

```text
Commit     : 3432374 (+ the EqualityBenchmarks harness fix described below)
Runtime    : .NET 10.0.8
OS         : Microsoft Windows 10.0.26100
Arch       : X64
Processors : 16
Job        : ShortRun
```

`EqualityBenchmarks` was re-measured after its harness was corrected (see below); the library code was identical in
both runs, so the numbers describe the same library build.

<!-- RESULTS -->

### ResultBenchmarks

| Method       | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| MapOk_OnOk   |  2.907 ns | 10.916 ns | 0.5983 ns |  2.728 ns |  1.03 |    0.25 |      - |         - |          NA |
| BindOk_OnOk  |  7.824 ns | 16.813 ns | 0.9216 ns |  8.204 ns |  2.76 |    0.54 |      - |         - |          NA |
| MapOk_OnErr  |  2.522 ns |  2.860 ns | 0.1568 ns |  2.510 ns |  0.89 |    0.16 |      - |         - |          NA |
| MapErr_OnErr | 17.278 ns | 16.692 ns | 0.9149 ns | 16.842 ns |  6.10 |    1.05 | 0.0068 |      64 B |          NA |
| Match_OnOk   |  1.941 ns | 21.274 ns | 1.1661 ns |  1.331 ns |  0.69 |    0.38 |      - |         - |          NA |
| Validate_Two |  3.215 ns |  2.683 ns | 0.1471 ns |  3.251 ns |  1.14 |    0.19 |      - |         - |          NA |

### OptionBenchmarks

| Method          | Mean     | Error      | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------- |---------:|-----------:|----------:|------:|--------:|----------:|------------:|
| MapSome_OnSome  | 1.157 ns |  1.3969 ns | 0.0766 ns |  1.00 |    0.08 |         - |          NA |
| BindSome_OnSome | 1.596 ns |  4.8610 ns | 0.2665 ns |  1.38 |    0.22 |         - |          NA |
| MapSome_OnNone  | 2.158 ns | 14.4589 ns | 0.7925 ns |  1.87 |    0.61 |         - |          NA |
| Filter_OnSome   | 2.397 ns |  0.5666 ns | 0.0311 ns |  2.08 |    0.13 |         - |          NA |
| Match_OnSome    | 2.109 ns |  1.2461 ns | 0.0683 ns |  1.83 |    0.12 |         - |          NA |

### EitherBenchmarks

| Method           | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| MapRight_OnRight |  2.010 ns |  4.268 ns | 0.2339 ns |  1.01 |    0.14 |      - |         - |          NA |
| MapRight_OnLeft  |  2.176 ns |  2.530 ns | 0.1387 ns |  1.09 |    0.12 |      - |         - |          NA |
| Swap_OnRight     |  2.012 ns |  2.892 ns | 0.1585 ns |  1.01 |    0.12 |      - |         - |          NA |
| Match_OnRight    |  1.030 ns |  4.871 ns | 0.2670 ns |  0.52 |    0.13 |      - |         - |          NA |
| ToResult_OnLeft  | 18.238 ns | 35.706 ns | 1.9572 ns |  9.15 |    1.23 | 0.0068 |      64 B |          NA |

### SequenceBenchmarks

| Method                     | N    | Mean         | Error         | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |----- |-------------:|--------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **Sequence_Manual_AllOk**      | **16**   |     **67.66 ns** |    **192.528 ns** |  **10.553 ns** |  **1.02** |    **0.19** | **0.0127** |      **-** |     **120 B** |        **1.00** |
| Sequence_AllOk             | 16   |    191.23 ns |    170.617 ns |   9.352 ns |  2.87 |    0.38 | 0.0254 |      - |     240 B |        2.00 |
| Sequence_ErrorAtMiddle     | 16   |     70.63 ns |      9.176 ns |   0.503 ns |  1.06 |    0.13 | 0.0136 |      - |     128 B |        1.07 |
| SequenceValidation_AllErr  | 16   |    244.38 ns |    120.476 ns |   6.604 ns |  3.67 |    0.46 | 0.0544 |      - |     512 B |        4.27 |
| SequenceValidation_HalfErr | 16   |     99.94 ns |     68.523 ns |   3.756 ns |  1.50 |    0.19 | 0.0263 |      - |     248 B |        2.07 |
| Partition_Half             | 16   |    152.24 ns |    147.501 ns |   8.085 ns |  2.28 |    0.30 | 0.0501 | 0.0005 |     472 B |        3.93 |
|                            |      |              |               |            |       |         |        |        |           |             |
| **Sequence_Manual_AllOk**      | **256**  |    **775.17 ns** |  **2,285.233 ns** | **125.261 ns** |  **1.02** |    **0.20** | **0.1144** | **0.0005** |    **1080 B** |        **1.00** |
| Sequence_AllOk             | 256  |  2,120.83 ns |    518.503 ns |  28.421 ns |  2.78 |    0.39 | 0.2394 | 0.0019 |    2256 B |        2.09 |
| Sequence_ErrorAtMiddle     | 256  |    614.97 ns |    755.394 ns |  41.406 ns |  0.81 |    0.12 | 0.1254 |      - |    1184 B |        1.10 |
| SequenceValidation_AllErr  | 256  |  2,399.59 ns |  2,192.500 ns | 120.178 ns |  3.15 |    0.46 | 0.5741 | 0.0114 |    5408 B |        5.01 |
| SequenceValidation_HalfErr | 256  |    834.58 ns |    580.603 ns |  31.825 ns |  1.10 |    0.16 | 0.1278 |      - |    1208 B |        1.12 |
| Partition_Half             | 256  |  1,149.59 ns |    909.570 ns |  49.857 ns |  1.51 |    0.22 | 0.4578 | 0.0324 |    4312 B |        3.99 |
|                            |      |              |               |            |       |         |        |        |           |             |
| **Sequence_Manual_AllOk**      | **4096** | **10,154.91 ns** |  **5,697.087 ns** | **312.277 ns** |  **1.00** |    **0.04** | **1.7395** | **0.1907** |   **16440 B** |        **1.00** |
| Sequence_AllOk             | 4096 | 15,015.20 ns |  9,603.116 ns | 526.379 ns |  1.48 |    0.06 | 3.4943 | 0.3815 |   33072 B |        2.01 |
| Sequence_ErrorAtMiddle     | 4096 |  8,069.23 ns |  5,346.780 ns | 293.075 ns |  0.80 |    0.03 | 1.7624 | 0.0610 |   16640 B |        1.01 |
| SequenceValidation_AllErr  | 4096 | 32,090.18 ns |  8,065.710 ns | 442.109 ns |  3.16 |    0.09 | 8.7280 | 2.1667 |   82304 B |        5.01 |
| SequenceValidation_HalfErr | 4096 | 11,206.65 ns | 13,104.673 ns | 718.311 ns |  1.10 |    0.07 | 1.7548 | 0.0916 |   16568 B |        1.01 |
| Partition_Half             | 4096 | 17,576.52 ns |  9,298.264 ns | 509.669 ns |  1.73 |    0.06 | 6.9733 | 6.9580 |   65752 B |        4.00 |

### CollectionBenchmarks

| Method                       | N    | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------- |----- |------------:|-------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **Values_AllSome**               | **16**   |    **40.19 ns** |    **22.577 ns** |   **1.237 ns** |  **1.00** |    **0.04** | **0.0153** |      **-** |     **144 B** |        **1.00** |
| Sequence_AllSome             | 16   |    63.69 ns |    57.468 ns |   3.150 ns |  1.59 |    0.08 | 0.0254 |      - |     240 B |        1.67 |
| Sequence_HalfNone            | 16   |    14.71 ns |    12.676 ns |   0.695 ns |  0.37 |    0.02 | 0.0076 |      - |      72 B |        0.50 |
| Traverse_AllSome             | 16   |    85.48 ns |    26.176 ns |   1.435 ns |  2.13 |    0.07 | 0.0297 |      - |     280 B |        1.94 |
| ToSingleError_FromValidation | 16   |    45.76 ns |    58.861 ns |   3.226 ns |  1.14 |    0.08 | 0.0153 |      - |     144 B |        1.00 |
|                              |      |             |              |            |       |         |        |        |           |             |
| **Values_AllSome**               | **256**  |   **359.13 ns** |   **120.237 ns** |   **6.591 ns** |  **1.00** |    **0.02** | **0.1173** | **0.0005** |    **1104 B** |        **1.00** |
| Sequence_AllSome             | 256  |   569.19 ns |    59.567 ns |   3.265 ns |  1.59 |    0.03 | 0.2394 | 0.0019 |    2256 B |        2.04 |
| Sequence_HalfNone            | 256  |    13.01 ns |     5.288 ns |   0.290 ns |  0.04 |    0.00 | 0.0076 |      - |      72 B |        0.07 |
| Traverse_AllSome             | 256  |   678.93 ns |   305.184 ns |  16.728 ns |  1.89 |    0.05 | 0.2432 | 0.0019 |    2296 B |        2.08 |
| ToSingleError_FromValidation | 256  |   431.27 ns |   643.789 ns |  35.288 ns |  1.20 |    0.09 | 0.1173 | 0.0010 |    1104 B |        1.00 |
|                              |      |             |              |            |       |         |        |        |           |             |
| **Values_AllSome**               | **4096** | **5,331.78 ns** | **5,896.690 ns** | **323.217 ns** | **1.002** |    **0.07** | **1.7471** | **0.0992** |   **16464 B** |       **1.000** |
| Sequence_AllSome             | 4096 | 7,165.84 ns | 6,609.682 ns | 362.299 ns | 1.347 |    0.09 | 3.5019 | 0.3815 |   33072 B |       2.009 |
| Sequence_HalfNone            | 4096 |    12.92 ns |     9.997 ns |   0.548 ns | 0.002 |    0.00 | 0.0076 |      - |      72 B |       0.004 |
| Traverse_AllSome             | 4096 | 8,004.71 ns | 1,170.585 ns |  64.164 ns | 1.505 |    0.08 | 3.5095 | 0.4883 |   33112 B |       2.011 |
| ToSingleError_FromValidation | 4096 | 5,619.42 ns | 3,138.610 ns | 172.038 ns | 1.056 |    0.06 | 1.7471 | 0.1907 |   16464 B |       1.000 |

### RopErrorBenchmarks

| Method                      | N    | Mean          | Error          | StdDev        | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |----- |--------------:|---------------:|--------------:|------:|--------:|--------:|-------:|----------:|------------:|
| **Combine_CopyOnly_LowerBound** | **1**    |     **22.712 ns** |      **17.333 ns** |     **0.9501 ns** |  **1.00** |    **0.05** |  **0.0068** |      **-** |      **64 B** |        **1.00** |
| Combine                     | 1    |      3.450 ns |       4.091 ns |     0.2242 ns |  0.15 |    0.01 |       - |      - |         - |        0.00 |
|                             |      |               |                |               |       |         |         |        |           |             |
| **Combine_CopyOnly_LowerBound** | **10**   |     **34.319 ns** |      **66.484 ns** |     **3.6442 ns** |  **1.01** |    **0.13** |  **0.0144** |      **-** |     **136 B** |        **1.00** |
| Combine                     | 10   |    817.436 ns |   1,343.940 ns |    73.6659 ns | 24.00 |    2.90 |  0.1450 |      - |    1368 B |       10.06 |
|                             |      |               |                |               |       |         |         |        |           |             |
| **Combine_CopyOnly_LowerBound** | **100**  |    **110.210 ns** |      **84.355 ns** |     **4.6238 ns** |  **1.00** |    **0.05** |  **0.0910** | **0.0002** |     **856 B** |        **1.00** |
| Combine                     | 100  |  5,611.333 ns |   1,342.142 ns |    73.5673 ns | 50.97 |    1.94 |  1.0376 |      - |    9792 B |       11.44 |
|                             |      |               |                |               |       |         |         |        |           |             |
| **Combine_CopyOnly_LowerBound** | **1000** |    **905.230 ns** |     **984.065 ns** |    **53.9399 ns** |  **1.00** |    **0.07** |  **0.8554** | **0.0248** |    **8056 B** |        **1.00** |
| Combine                     | 1000 | 63,870.243 ns | 132,388.735 ns | 7,256.6733 ns | 70.73 |    7.92 | 10.0098 | 0.7935 |   94632 B |       11.75 |

### ParsingBenchmarks

| Method                 | Mean        | Error       | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |------------:|------------:|----------:|------:|--------:|-------:|----------:|------------:|
| Parse_Int_Success      |    21.59 ns |    40.04 ns |  2.195 ns |  1.01 |    0.12 |      - |         - |          NA |
| Parse_Int_Failure      |   250.69 ns |   201.25 ns | 11.031 ns | 11.69 |    1.11 | 0.0534 |     504 B |          NA |
| Parse_Int_LongFailure  | 1,734.84 ns | 1,713.35 ns | 93.915 ns | 80.91 |    7.98 | 0.1411 |    1360 B |          NA |
| Parse_Long_Success     |    38.37 ns |    91.26 ns |  5.002 ns |  1.79 |    0.26 |      - |         - |          NA |
| TryParse_Int_Success   |    22.57 ns |    99.85 ns |  5.473 ns |  1.05 |    0.24 |      - |         - |          NA |
| TryParse_Int_Failure   |    17.93 ns |    12.88 ns |  0.706 ns |  0.84 |    0.08 |      - |         - |          NA |
| Parse_Guid_Success     |    48.29 ns |    85.78 ns |  4.702 ns |  2.25 |    0.27 |      - |         - |          NA |
| Parse_DateTime_Success |   205.58 ns |   679.61 ns | 37.252 ns |  9.59 |    1.72 |      - |         - |          NA |

### AsyncBenchmarks

| Method                | Mean      | Error      | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------:|-----------:|---------:|------:|--------:|-------:|----------:|------------:|
| MapOkAsync_Completed  |  32.22 ns |  40.315 ns | 2.210 ns |  1.00 |    0.08 | 0.0246 |     232 B |        1.00 |
| MapOkAsync_Pending    |  34.19 ns |  25.389 ns | 1.392 ns |  1.06 |    0.07 | 0.0238 |     224 B |        0.97 |
| BindOkAsync_Completed |  37.86 ns |  17.738 ns | 0.972 ns |  1.18 |    0.07 | 0.0255 |     240 B |        1.03 |
| BindOkAsync_Pending   | 101.74 ns | 113.854 ns | 6.241 ns |  3.17 |    0.25 | 0.0373 |     352 B |        1.52 |
| MapErrAsync_OnErr     |  47.39 ns |  12.546 ns | 0.688 ns |  1.48 |    0.09 | 0.0314 |     296 B |        1.28 |
| MapOkAsync_OnErr      |  29.08 ns |   1.894 ns | 0.104 ns |  0.91 |    0.05 | 0.0170 |     160 B |        0.69 |

### EqualityBenchmarks

| Method                  | Mean      | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Result_Equals_SameValue |  1.422 ns |  0.0268 ns | 0.0015 ns |  1.00 |    0.00 |      - |         - |          NA |
| Result_Equals_Different |  1.237 ns |  0.5625 ns | 0.0308 ns |  0.87 |    0.02 |      - |         - |          NA |
| Result_GetHashCode      |  2.848 ns |  0.4870 ns | 0.0267 ns |  2.00 |    0.02 |      - |         - |          NA |
| Option_Equals_SameValue |  1.509 ns |  3.8873 ns | 0.2131 ns |  1.06 |    0.13 |      - |         - |          NA |
| Option_Equals_None      |  1.123 ns |  0.0872 ns | 0.0048 ns |  0.79 |    0.00 |      - |         - |          NA |
| Option_GetHashCode      |  2.799 ns |  0.7833 ns | 0.0429 ns |  1.97 |    0.03 |      - |         - |          NA |
| Either_Equals_SameValue |  1.729 ns |  0.4987 ns | 0.0273 ns |  1.22 |    0.02 |      - |         - |          NA |
| Either_GetHashCode      |  2.871 ns |  0.3545 ns | 0.0194 ns |  2.02 |    0.01 |      - |         - |          NA |
| Result_ToString         | 62.584 ns | 20.1724 ns | 1.1057 ns | 44.01 |    0.67 | 0.0127 |     120 B |          NA |
| Option_ToString         |  1.253 ns |  0.3456 ns | 0.0189 ns |  0.88 |    0.01 |      - |         - |          NA |
| Either_ToString         | 45.101 ns | 14.6586 ns | 0.8035 ns | 31.71 |    0.49 | 0.0042 |      40 B |          NA |
## Interpretation

- Pure combinators do not allocate: `Result`, `Option` and `Either` are structs, the benchmark delegates are cached
  static lambdas, and no defensive copies escape.
- `MapErr` allocates because `RopError` is an immutable class and `WithCode` returns a new instance. Error paths are
  not the hot path; when they are, reuse a pre-built `RopError`.
- Aggregation on the success path (`Validate`) does not allocate an error list. The failure path allocates the list
  plus a read-only wrapper, which is the documented cost of collecting every error.
- `SequenceValidation` pre-sizes to the source count; `Sequence` deliberately does not, so an early error does not
  allocate for the whole input. The API contract is "enumerate once, immediately".
- The async paths pay for the state machine. A completed task avoids the suspension cost, so the completed and
  pending variants are measured separately.

## Allocation history

| Change | `Sequence_16` allocated |
| --- | ---: |
| 1.0.0 initial (`List` → `ToArray` → `ReadOnlyCollection`) | 328 B |
| 1.0.0 after exposing an exactly-sized list directly | 240 B |
| 1.0.0 `SequenceValidation_16` with count-aware pre-sizing | 144 B |

`Rop.ToReadOnlyList` wraps the list when `Capacity == Count` (the common case for small aggregations and error
lists) and copies only when the list has spare capacity, so a grown list's slack is never retained by the exposed
view. APIs that always materialize (`SequenceValidation`, `Partition`, `Opt.Values`) pre-size their lists from
`ICollection<T>.Count`; short-circuit APIs (`Sequence`, `Traverse`, `Opt.Sequence`) deliberately do not, so an early
error does not allocate for the full input.
