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

    private readonly Result<int, RopError> _ok = Rop.Ok(42);
    private readonly Result<int, RopError> _err = Rop.Err<int>(Error);

    [Benchmark(Baseline = true)]
    public Result<int, RopError> MapOk_OnOk() => _ok.MapOk(static value => value + 1);

    [Benchmark]
    public Result<int, RopError> BindOk_OnOk() => _ok.BindOk(static value => Rop.Ok(value + 1));

    [Benchmark]
    public Result<int, RopError> MapOk_OnErr() => _err.MapOk(static value => value + 1);

    [Benchmark]
    public Result<int, RopError> MapErr_OnErr() => _err.MapErr(static error => error.WithCode("E_OTHER"));

    [Benchmark]
    public int Match_OnOk() => _ok.Match(static value => value, static _ => -1);

    [Benchmark]
    public Result<int, IReadOnlyList<RopError>> Validate_Two() =>
        Rop.Validate(Rop.Ok(1), Rop.Ok(2), static (first, second) => first + second);
}
