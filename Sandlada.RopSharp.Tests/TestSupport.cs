using System.Collections;

namespace Sandlada.RopSharp.Tests;

internal static class Sample
{
    internal static RopError Error(string code = "E_TEST", string message = "failed") =>
        RopError.Create(code, message);

    internal sealed record User(string Name, int Age);

    internal sealed record Order(string Name, decimal Amount);

    internal sealed record Profile(string? Nickname, int? Score);
}

internal static class TestSupport
{
    internal static async Task AssertSequenceAsync<T>(IEnumerable<T> actual, params T[] expected)
    {
        var list = actual as IReadOnlyList<T> ?? actual.ToList();
        await Assert.That(list.Count).IsEqualTo(expected.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            await Assert.That(list[index]).IsEqualTo(expected[index]);
        }
    }

    internal static async Task<Exception?> CatchAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    internal static async Task<Exception?> CatchAsync<TOk, TErr>(Func<ResultTask<TOk, TErr>> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    internal static async Task<Exception?> CatchAsync<T>(Func<OptionTask<T>> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    internal static async Task<Exception?> CatchAsync<T, TErr>(Func<ValidationTask<T, TErr>> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    internal static Exception? Catch(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    internal static TErr RequireErr<TOk, TErr>(Result<TOk, TErr> result) =>
        result.TryGetErr(out var error)
            ? error
            : throw new InvalidOperationException($"Expected Err but was {result}.");

    internal static TErr ExpectErr<TOk, TErr>(this Result<TOk, TErr> result) => RequireErr(result);

    internal static IReadOnlyList<TErr> ExpectErrors<T, TErr>(this Validation<T, TErr> validation)
    {
        if (!validation.TryGetErrors(out var errors))
        {
            throw new InvalidOperationException($"Expected validation failure but was {validation}.");
        }

        return errors;
    }
}

internal sealed class CountingSequence<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _inner;

    internal CountingSequence(IEnumerable<T> inner) => _inner = inner;

    internal int GetEnumeratorCalls { get; private set; }

    internal int ItemsYielded { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        GetEnumeratorCalls++;
        foreach (var item in _inner)
        {
            ItemsYielded++;
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
