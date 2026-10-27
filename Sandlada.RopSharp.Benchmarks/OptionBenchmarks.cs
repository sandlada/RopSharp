using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Hot-path baselines for the <see cref="Option{T}"/> surface.
/// </summary>
[MemoryDiagnoser]
public class OptionBenchmarks
{
    private readonly Option<int> _some = Opt.Some(42);
    private readonly Option<int> _none = Opt.None<int>();

    [Benchmark(Baseline = true)]
    public Option<int> MapSome_OnSome() => _some.MapSome(static value => value + 1);

    [Benchmark]
    public Option<int> BindSome_OnSome() => _some.BindSome(static value => Opt.Some(value + 1));

    [Benchmark]
    public Option<int> MapSome_OnNone() => _none.MapSome(static value => value + 1);

    [Benchmark]
    public Option<int> Filter_OnSome() => _some.Filter(static value => value > 0);

    [Benchmark]
    public int Match_OnSome() => _some.Match(static value => value, static () => -1);
}
