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

    [Test]
    [NotInParallel]
    public async Task ResultPipeline_DoesNotAllocatePerIteration()
    {
        var unreachable = RopError.Create("E_X", "unreachable");

        // Warm up JIT and static delegate caches before measuring.
        for (var index = 0; index < Iterations; index++)
        {
            Consume(Rop.Ok(index).MapOk(static value => value + 1).BindOk(static value => Rop.Ok(value * 2)));
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < Iterations; index++)
        {
            Consume(Rop.Ok(index)
                .MapOk(static value => value + 1)
                .BindOk(static value => Rop.Ok(value * 2))
                .Ensure(static value => value >= 0, unreachable));
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
            Consume(Opt.Some(index).MapSome(static value => value + 1).Filter(static value => value > 0));
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < Iterations; index++)
        {
            Consume(Opt.Some(index)
                .MapSome(static value => value + 1)
                .Filter(static value => value > 0)
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
