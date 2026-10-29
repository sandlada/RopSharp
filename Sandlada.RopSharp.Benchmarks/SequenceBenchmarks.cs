using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Scaling of the aggregate operations over N results. The manual baseline is a hand-written loop with the same
/// observable result, so the ratio shows the cost of the abstraction at each size.
/// </summary>
[Config(typeof(BenchConfig))]
public class SequenceBenchmarks
{
    private static readonly RopError Error = RopError.Create("E_BENCH", "failure");

    [Params(16, 256, 4096)]
    public int N { get; set; }

    private Result<int, RopError>[] _allOk = [];
    private Result<int, RopError>[] _errorAtMiddle = [];
    private Result<int, RopError>[] _allErr = [];

    [GlobalSetup]
    public void Setup()
    {
        _allOk = [.. Enumerable.Range(0, N).Select(static value => Rop.Ok(value))];
        _errorAtMiddle = [.. Enumerable.Range(0, N).Select(value => value == N / 2 ? Rop.Err<int>(Error) : Rop.Ok(value))];
        _allErr = [.. Enumerable.Range(0, N).Select(static _ => Rop.Err<int>(Error))];
    }

    [Benchmark(Baseline = true)]
    public Result<IReadOnlyList<int>, RopError> Sequence_Manual_AllOk()
    {
        var values = new List<int>(_allOk.Length);
        foreach (var result in _allOk)
        {
            if (!result.TryGetOk(out var value))
            {
                return Rop.Err<IReadOnlyList<int>>(Error);
            }

            values.Add(value);
        }

        return Rop.Ok<IReadOnlyList<int>>(values);
    }

    [Benchmark]
    public Result<IReadOnlyList<int>, RopError> Sequence_AllOk() => Rop.Sequence(_allOk);

    [Benchmark]
    public Result<IReadOnlyList<int>, RopError> Sequence_ErrorAtMiddle() => Rop.Sequence(_errorAtMiddle);

    [Benchmark]
    public Result<IReadOnlyList<int>, IReadOnlyList<RopError>> SequenceValidation_AllErr() =>
        Rop.SequenceValidation(_allErr);

    [Benchmark]
    public Result<IReadOnlyList<int>, IReadOnlyList<RopError>> SequenceValidation_HalfErr() =>
        Rop.SequenceValidation(_errorAtMiddle);

    [Benchmark]
    public (IReadOnlyList<int> Oks, IReadOnlyList<RopError> Errs) Partition_Half() => Rop.Partition(_errorAtMiddle);
}
