using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Collection-oriented helpers. These materialize lists, so they are the paths where the library's internal
/// <c>ToReadOnlyList</c> choices show up in the numbers.
/// </summary>
[Config(typeof(BenchConfig))]
public class CollectionBenchmarks
{
    private static readonly RopError Error = RopError.Create("E_BENCH", "failure");

    [Params(16, 256, 4096)]
    public int N { get; set; }

    private Option<int>[] _allSome = [];
    private Option<int>[] _halfNone = [];
    private Result<int, RopError>[] _allOk = [];

    [GlobalSetup]
    public void Setup()
    {
        _allSome = [.. Enumerable.Range(0, N).Select(static value => Option.Some(value))];
        _halfNone = [.. Enumerable.Range(0, N).Select(static value => value % 2 == 0 ? Option.Some(value) : Option.None<int>())];
        _allOk = [.. Enumerable.Range(0, N).Select(static value => Result.Ok(value))];
    }

    [Benchmark(Baseline = true)]
    public IReadOnlyList<int> Values_AllSome() => Option.Values(_allSome);

    [Benchmark]
    public Option<IReadOnlyList<int>> Sequence_AllSome() => Option.Sequence(_allSome);

    [Benchmark]
    public Option<IReadOnlyList<int>> Sequence_HalfNone() => Option.Sequence(_halfNone);

    [Benchmark]
    public Option<IReadOnlyList<int>> Traverse_AllSome() =>
        Option.Traverse(Enumerable.Range(0, N), static value => Option.Some(value));

    [Benchmark]
    public Result<IReadOnlyList<int>, RopError> ToSingleError_FromValidation() =>
        Validation.Sequence(_allOk).Collapse(static errors => errors[0]);
}
