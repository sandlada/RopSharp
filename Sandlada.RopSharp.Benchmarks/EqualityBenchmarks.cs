using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Value semantics of the three core types. Equality and hashing are the paths used when results are used as
/// dictionary keys or compared in collections, and <c>ToString</c> is the path used in diagnostics. The equality and
/// hashing calls are far below the resolution of a single call and are measured with <c>OperationsPerInvoke</c>.
/// </summary>
[Config(typeof(BenchConfig))]
public class EqualityBenchmarks
{
    private readonly Result<int, RopError> _okA = Rop.Ok(42);
    private readonly Result<int, RopError> _okB = Rop.Ok(42);
    private readonly Result<int, RopError> _err = Rop.Err<int>(RopError.Create("E_BENCH", "failure"));

    private readonly Option<int> _someA = Opt.Some(42);
    private readonly Option<int> _someB = Opt.Some(42);
    private readonly Option<int> _none = Opt.None<int>();

    private readonly Either<string, int> _rightA = Either.Right<string, int>(42);
    private readonly Either<string, int> _rightB = Either.Right<string, int>(42);

    [Benchmark(Baseline = true, OperationsPerInvoke = 1000)]
    public int Result_Equals_SameValue()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _okA.Equals(_okB) ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Result_Equals_Different()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _okA.Equals(_err) ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Result_GetHashCode()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _okA.GetHashCode();
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Option_Equals_SameValue()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _someA.Equals(_someB) ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Option_Equals_None()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _someA.Equals(_none) ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Option_GetHashCode()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _someA.GetHashCode();
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Either_Equals_SameValue()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _rightA.Equals(_rightB) ? 1 : 0;
        }

        return total;
    }

    [Benchmark(OperationsPerInvoke = 1000)]
    public int Either_GetHashCode()
    {
        var total = 0;
        for (var index = 0; index < 1000; index++)
        {
            total += _rightA.GetHashCode();
        }

        return total;
    }

    [Benchmark]
    public string Result_ToString() => _err.ToString();

    [Benchmark(OperationsPerInvoke = 1000)]
    public string Option_ToString()
    {
        var text = string.Empty;
        for (var index = 0; index < 1000; index++)
        {
            text = _none.ToString();
        }

        return text;
    }


    [Benchmark]
    public string Either_ToString() => _rightA.ToString();
}
