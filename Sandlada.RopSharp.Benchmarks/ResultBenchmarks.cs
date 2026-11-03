using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Hot-path baselines for the <see cref="Result{TOk,TErr}"/> surface. Sequence and partition scaling lives in
/// <see cref="SequenceBenchmarks"/>.
/// </summary>
[Config(typeof(BenchConfig))]
public class ResultBenchmarks
{
    private static readonly RopError Error = RopError.Create("E_BENCH", "failure");

    private readonly Result<int, RopError> _ok = Result.Ok(42);
    private readonly Result<int, RopError> _err = Result.Err<int>(Error);

    [Benchmark(Baseline = true)]
    public Result<int, RopError> MapOk_OnOk() => _ok.MapOk(static value => value + 1);

    [Benchmark]
    public Result<int, RopError> BindOk_OnOk() => _ok.BindOk(static value => Result.Ok(value + 1));

    [Benchmark]
    public Result<int, RopError> MapOk_OnErr() => _err.MapOk(static value => value + 1);

    [Benchmark]
    public Result<int, RopError> MapErr_OnErr() => _err.MapErr(static error => error.WithCode("E_OTHER"));

    [Benchmark]
    public int Match_OnOk() => _ok.Match(static value => value, static _ => -1);

    [Benchmark]
    public Validation<int, RopError> Validate_Two() =>
        Validation.Zip(Result.Ok(1), Result.Ok(2), static (first, second) => first + second);
}
