using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Hot-path baselines for the <see cref="Option{T}"/> surface.
/// </summary>
[Config(typeof(BenchConfig))]
public class OptionBenchmarks
{
    private readonly Option<int> _some = Opt.Some(42);
    private readonly Option<int> _none = Opt.None<int>();

    [Benchmark(Baseline = true)]
    public Option<int> MapSome_OnSome() => _some.MapSome(static value => value + 1);

    [Benchmark(OperationsPerInvoke = 1000)]
    public int BindSome_OnSome()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _some.BindSome(static value => Opt.Some(value + 1)).IsSome ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int MapSome_OnNone()
    {
        var count = 0;
        for (var index = 0; index < 1000; index++)
        {
            count += _none.MapSome(static value => value + 1).IsSome ? 1 : 0;
        }

        return count;
    }

    [Benchmark]
    public Option<int> Filter_OnSome() => _some.Filter(static value => value > 0);

    [Benchmark]
    public int Match_OnSome() => _some.Match(static value => value, static () => -1);
}
