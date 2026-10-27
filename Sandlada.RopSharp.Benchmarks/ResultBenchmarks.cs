using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Hot-path baselines for the <see cref="Result{TOk,TErr}"/> surface. Decorated with the memory diagnoser so the
/// "no allocation per combinator" promise is visible next to the timings.
/// </summary>
[MemoryDiagnoser]
public class ResultBenchmarks
{
    private static readonly RopError Error = RopError.Create("E_BENCH", "failure");
    private static readonly Result<int, RopError>[] Successes =
        [.. Enumerable.Range(0, 16).Select(static value => Rop.Ok(value))];

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

    [Benchmark]
    public Result<IReadOnlyList<int>, RopError> Sequence_16() => Rop.Sequence(Successes);

    [Benchmark]
    public Result<IReadOnlyList<int>, IReadOnlyList<RopError>> SequenceValidation_16() =>
        Rop.SequenceValidation(Successes);

    [Benchmark]
    public (IReadOnlyList<int> Oks, IReadOnlyList<RopError> Errs) Partition_16() => Rop.Partition(Successes);
}
