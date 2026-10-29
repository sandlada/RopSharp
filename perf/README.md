# perf

Performance baselines that are enforced in CI.

| File | Gate | Where it runs |
| --- | --- | --- |
| `aot-size.baseline.json` | native AOT binary size | `.github/workflows/ci.yml`, job `native-aot` |

## AOT size gate

The native AOT binary is the most stable proxy for "did this change make the library heavier?". Unlike a
nanosecond threshold it does not depend on the machine, so it can block a merge.

The gate fails when the published `Sandlada.RopSharp.Sample.exe` grows by more than `tolerancePercent` (10%) over the
baseline.

### Updating the baseline

1. Publish the native image locally:

   ```bash
   dotnet publish Sandlada.RopSharp.Sample/Sandlada.RopSharp.Sample.csproj \
     -c Release -r win-x64 -p:PublishAot=true -o artifacts/aot
   ```

2. Record the new size of `Sandlada.RopSharp.Sample.exe` in `aot-size.baseline.json`.
3. Update `measuredWith` so the provenance of the new number is known.
4. Explain in the pull request why the size changed. A size increase caused by a deliberate feature is acceptable;
   one caused by accidental code growth is a regression and should be fixed instead.

The baseline is measured on Windows because the CI job publishes for `win-x64`. A different operating system or SDK
produces a different size, so do not compare a local number from another environment against this file.

### First run caveat

The checked-in baseline was measured on a developer machine, not on the CI runner. The native image size depends on the
Windows build and on the Visual C++ toolchain version, so the first CI run may report a number that differs from the
baseline by more than the tolerance and fail the job. If that happens, check the reported size, confirm that it is a
toolchain difference rather than a code change, and update the baseline in a separate pull request that says so.
