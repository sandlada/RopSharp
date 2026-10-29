using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Hot-path baselines for the <see cref="Either{TLeft,TRight}"/> surface, the only core type without earlier coverage.
/// </summary>
[Config(typeof(BenchConfig))]
public class EitherBenchmarks
{
    private readonly Either<string, int> _right = Either.Right<string, int>(42);
    private readonly Either<string, int> _left = Either.Left<string, int>("left");

    [Benchmark(Baseline = true)]
    public Either<string, int> MapRight_OnRight() => _right.MapRight(static value => value + 1);

    [Benchmark]
    public Either<string, int> MapRight_OnLeft() => _left.MapRight(static value => value + 1);

    [Benchmark]
    public Either<int, string> Swap_OnRight() => _right.Swap();

    [Benchmark]
    public int Match_OnRight() => _right.Match(static left => left.Length, static right => right);

    [Benchmark]
    public Result<int, RopError> ToResult_OnLeft() => _left.ToResult(static left => RopError.Create("E_LEFT", left));
}
