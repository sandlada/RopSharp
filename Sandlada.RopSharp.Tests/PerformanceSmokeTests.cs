using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Coarse allocation smoke checks for the hot combinators. These are not micro-benchmarks: they guard the
/// "immutable structs, no accidental boxing" promise with a generous tolerance so they stay stable across runtimes.
/// </summary>
public sealed class PerformanceSmokeTests
{
    private const int Iterations = 2_000;
    private const long ToleranceBytes = 8 * 1024;

    private static int _sink;

    // The delegates are hoisted on purpose: a delegate that is created inside the measured loop measures the
    // compiler's lambda caching instead of the library, and the caching is a release-only optimization.
    private static readonly Func<int, int> AddOne = static value => value + 1;
    private static readonly Func<int, Result<int, RopError>> DoubleAndWrap = static value => Result.Ok(value * 2);
    private static readonly Func<int, bool> IsNonNegative = static value => value >= 0;
    private static readonly Func<int, bool> IsPositive = static value => value > 0;

    [Test]
    [NotInParallel]
    public async Task ResultPipeline_DoesNotAllocatePerIteration()
    {
        var unreachable = RopError.Create("E_X", "unreachable");

        // Warm up JIT and static delegate caches before measuring.
        for (var index = 0; index < Iterations; index++)
        {
            Consume(Result.Ok(index).MapOk(AddOne).BindOk(DoubleAndWrap));
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < Iterations; index++)
        {
            Consume(Result.Ok(index)
                .MapOk(AddOne)
                .BindOk(DoubleAndWrap)
                .Ensure(IsNonNegative, unreachable));
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        await Assert.That(allocated).IsLessThan(ToleranceBytes);
    }

    [Test]
    [NotInParallel]
    public async Task OptionPipeline_DoesNotAllocatePerIteration()
    {
        for (var index = 0; index < Iterations; index++)
        {
            Consume(Option.Some(index).MapSome(AddOne).Filter(IsPositive));
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < Iterations; index++)
        {
            Consume(Option.Some(index)
                .MapSome(AddOne)
                .Filter(IsPositive)
                .BindNoneValue(0));
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        await Assert.That(allocated).IsLessThan(ToleranceBytes);
    }

    [Test]
    public async Task CoreTypes_AreValueTypes()
    {
        await Assert.That(typeof(Result<int, string>).IsValueType).IsTrue();
        await Assert.That(typeof(Option<int>).IsValueType).IsTrue();
        await Assert.That(typeof(Unit).IsValueType).IsTrue();
        await Assert.That(typeof(Result<int, string>).IsSealed).IsTrue();
    }

    private static void Consume(Result<int, RopError> result) => Consume(result.IsOk);

    private static void Consume(Option<int> option) => Consume(option.IsSome);

    private static void Consume(bool value)
    {
        // Prevent the JIT from discarding the pipeline entirely without allocating.
        _sink += value ? 1 : 0;
    }
}
