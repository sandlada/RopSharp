using System.Runtime.CompilerServices;

namespace Sandlada.RopSharp.Tests.PerformanceGuards;

/// <summary>
/// Measures managed allocations on the current thread. The measured body must be synchronous and must not
/// capture per-iteration state, otherwise the closure itself is counted.
/// </summary>
internal static class AllocationMeter
{
    public const int Warmup = 1_000;
    public const int Iterations = 100_000;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static long BytesPerRun(Action<int> body, int warmup = Warmup, int iterations = Iterations)
    {
        for (var index = 0; index < warmup; index++)
        {
            body(index);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < iterations; index++)
        {
            body(index);
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        return allocated;
    }
}
