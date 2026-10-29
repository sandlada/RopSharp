using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Hot-path baselines for the <see cref="Option{T}"/> surface. The combinators run below the resolution of a
/// single call, so they are measured with <c>OperationsPerInvoke</c> to keep the numbers meaningful.
/// </summary>
[Config(typeof(BenchConfig))]
public class OptionBenchmarks
{
    private readonly Option<int> _some = Opt.Some(42);
    private readonly Option<int> _none = Opt.None<int>();

    [Benchmark(Baseline = true, OperationsPerInvoke = 1000)]
    public int MapSome_OnSome()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _some.MapSome(static value => value + 1).IsSome ? 1 : 0;
        }

        return total;
    }

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
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _none.MapSome(static value => value + 1).IsSome ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Filter_OnSome()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _some.Filter(static value => value > 0).IsSome ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Match_OnSome()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _some.Match(static value => value, static () => -1);
        }

        return total;
    }
}
