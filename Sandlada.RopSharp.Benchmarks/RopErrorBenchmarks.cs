using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Scaling of <see cref="RopError.Combine"/>, which must stay linear in the number of errors.
/// <c>Combine_CopyOnly_LowerBound</c> is the cheapest possible aggregation (copying the inputs into a list). It is a
/// lower bound for the cost, not a semantically equivalent implementation.
/// </summary>
[Config(typeof(BenchConfig))]
public class RopErrorBenchmarks
{
    private RopError[] _errors = [];

    [Params(1, 10, 100, 1000)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _errors = [.. Enumerable.Range(0, N).Select(static i => RopError.Create($"E_{i % 32}", $"message {i}"))];
    }

    [Benchmark(Baseline = true)]
    public List<RopError> Combine_CopyOnly_LowerBound() => new(_errors);

    [Benchmark]
    public RopError Combine() => RopError.Combine(_errors);
}
