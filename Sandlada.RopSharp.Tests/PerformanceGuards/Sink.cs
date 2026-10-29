using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests.PerformanceGuards;

/// <summary>
/// Keeps measured results observable so the JIT cannot discard the call under test. Generic overloads avoid boxing.
/// </summary>
internal static class Sink
{
    private static int _value;

    public static void Consume<T, TErr>(Result<T, TErr> result) => _value += result.IsOk ? 1 : 0;

    public static void Consume<T>(Option<T> option) => _value += option.IsSome ? 1 : 0;

    public static void Consume<TLeft, TRight>(Either<TLeft, TRight> either) => _value += either.IsRight ? 1 : 0;

    public static void Consume(RopError? error) => _value += error is null ? 0 : 1;

    public static void Consume(bool value) => _value += value ? 1 : 0;

    public static void Consume(int value) => _value += value;
}
