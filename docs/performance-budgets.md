# Performance budgets

Allocation budgets enforced by the test suite in `Sandlada.RopSharp.Tests/PerformanceGuards/`. They run in every
`dotnet test -c Release`, so a hot path that starts allocating fails the build. These budgets measure bytes, not time:
allocation counts are deterministic, while nanosecond timings on shared CI runners are not and are tracked separately
(see `docs/benchmarks.md`).

## Method

- `AllocationMeter.BytesPerRun` runs 1,000 warmup calls, then measures
  `GC.GetAllocatedBytesForCurrentThread()` across 100,000 calls (bounded checks use 10,000).
- Lambdas under test are `static` and capture no per-iteration state, so the closure itself is not counted.
- Results are sunk through `Sink` so the JIT cannot discard the call under test.
- Zero-byte budgets are exact. Bounded budgets are the observed bytes per call multiplied by 1.2, rounded up.

## Zero-byte hot paths

| Path | Test | Budget |
| --- | --- | --- |
| `Result.MapOk` on `Ok` | `ResultMapOk_OnOk_AllocatesNothing` | 0 B |
| `Result.BindOk` on `Ok` | `ResultBindOk_OnOk_AllocatesNothing` | 0 B |
| `Result.MapErr` on `Err` | `ResultMapErr_OnErr_AllocatesNothing` | 0 B |
| `Result.Match` | `ResultMatch_AllocatesNothing` | 0 B |
| `Result.Ensure` on `Ok` | `ResultEnsure_OnOk_AllocatesNothing` | 0 B |
| `Result.TryGetErr` on `Err` (includes the initialization guard) | `ResultTryGetErr_OnErr_AllocatesNothing` | 0 B |
| `Option.MapSome` on `Some` | `OptionMapSome_OnSome_AllocatesNothing` | 0 B |
| `Option.Filter` on `Some` | `OptionFilter_OnSome_AllocatesNothing` | 0 B |
| `Option.MapSome` on `None` | `OptionMapSome_OnNone_AllocatesNothing` | 0 B |
| `Either.MapRight` on `Right` | `EitherMapRight_OnRight_AllocatesNothing` | 0 B |

## Bounded hot paths

Bytes per call, measured on the reference run (16 elements for the collection paths, 10 errors for `Combine`).

| Path | Test | Observed | Budget (x1.2) |
| --- | --- | ---: | ---: |
| `Rop.Sequence`, all `Ok` | `ResultSequence_SuccessPath_StaysWithinBudget` | 272 B | 327 B |
| `Rop.Sequence`, error at index 8 (short-circuit) | `ResultSequence_ShortCircuitPath_StaysWithinBudget` | 160 B | 192 B |
| `Rop.SequenceValidation`, all `Err` | `ResultSequenceValidation_AllErrors_StaysWithinBudget` | 544 B | 653 B |
| `RopError.Combine`, 10 errors | `RopErrorCombine_Ten_StaysWithinBudget` | 1,528 B | 1,834 B |

The short-circuit budget is lower than the success budget because `Sequence` stops at the first error and does not
materialize the remaining values, which is the documented contract.

## Changing a budget

1. Measure the new value with the same harness and record the old and new numbers.
2. Explain the cause in the pull request. A rise caused by a deliberate feature is acceptable only with an ADR or
   CHANGELOG entry. A rise caused by an accidental allocation is a regression and must be fixed instead.
3. Update the constant in `AllocationBudgets.cs` and the table above in the same commit.

Lowering a budget after an optimization is encouraged and should be recorded in the same way.
