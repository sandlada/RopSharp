namespace Sandlada.RopSharp;

internal static class OptionTaskCore
{
    internal static async Task<Option<TOut>> MapSome<T, TOut>(
        Task<Option<T>> source, Func<T, TOut> mapper, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value)) return Option.None<TOut>();
        cancellationToken.ThrowIfCancellationRequested();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("MapSome");
        return Option.Some(mapper(value));
    }

    internal static async Task<Option<TOut>> MapSomeAsync<T, TOut>(
        Task<Option<T>> source, Func<T, CancellationToken, Task<TOut>> mapper, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value)) return Option.None<TOut>();
        cancellationToken.ThrowIfCancellationRequested();
        return Option.Some(await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false));
    }

    internal static async Task<Option<TOut>> BindSome<T, TOut>(
        Task<Option<T>> source, Func<T, Option<TOut>> binder, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value)) return Option.None<TOut>();
        cancellationToken.ThrowIfCancellationRequested();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("BindSome");
        return binder(value);
    }

    internal static async Task<Option<TOut>> BindSomeAsync<T, TOut>(
        Task<Option<T>> source, Func<T, CancellationToken, Task<Option<TOut>>> binder, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value)) return Option.None<TOut>();
        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(binder(value, cancellationToken), "binder").ConfigureAwait(false);
    }

    internal static async Task<Option<T>> BindNone<T>(
        Task<Option<T>> source, Func<CancellationToken, Task<Option<T>>> fallback, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.TryGetValue(out _)) return option;
        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(fallback(cancellationToken), "fallback").ConfigureAwait(false);
    }

    internal static async Task<Option<T>> Filter<T>(
        Task<Option<T>> source, Func<T, bool> predicate, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value)) return option;
        cancellationToken.ThrowIfCancellationRequested();
        return predicate(value) ? option : Option.None<T>();
    }

    internal static async Task<Option<T>> FilterAsync<T>(
        Task<Option<T>> source, Func<T, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value)) return option;
        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(predicate(value, cancellationToken), "predicate").ConfigureAwait(false)
            ? option
            : Option.None<T>();
    }

    internal static async Task<Option<T>> TeeSome<T>(Task<Option<T>> source, Action<T> action, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.TryGetValue(out var value)) { cancellationToken.ThrowIfCancellationRequested(); action(value); }
        return option;
    }

    internal static async Task<Option<T>> TeeSomeAsync<T>(
        Task<Option<T>> source, Func<T, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.TryGetValue(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value, cancellationToken), "action").ConfigureAwait(false);
        }
        return option;
    }

    internal static async Task<Option<T>> TeeNone<T>(Task<Option<T>> source, Action action, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.IsNone) { cancellationToken.ThrowIfCancellationRequested(); action(); }
        return option;
    }

    internal static async Task<Option<T>> TeeNoneAsync<T>(
        Task<Option<T>> source, Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var option = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(cancellationToken), "action").ConfigureAwait(false);
        }
        return option;
    }

    internal static async Task<Option<TOut>> CombineAsync<T1, T2, TOut>(
        Task<Option<T1>> first, Task<Option<T2>> second, Func<T1, T2, TOut> merger, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(first, second).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Option.Combine(await first.ConfigureAwait(false), await second.ConfigureAwait(false), merger);
    }

    internal static async Task<Option<TOut>> CombineAsync<T1, T2, T3, TOut>(
        Task<Option<T1>> first, Task<Option<T2>> second, Task<Option<T3>> third,
        Func<T1, T2, T3, TOut> merger, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(first, second, third).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Option.Combine(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            await third.ConfigureAwait(false),
            merger);
    }

    internal static async Task<Option<IReadOnlyList<T>>> SequenceAsync<T>(
        IEnumerable<Task<Option<T>>> tasks, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var values = new List<T>();
        foreach (var task in tasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (task is null) throw new InvalidOperationException("The sequence contained a null task.");
            var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!option.TryGetValue(out var value)) return Option.None<IReadOnlyList<T>>();
            values.Add(value);
        }
        return Option.Some(Result.ToReadOnlyList(values));
    }

    internal static async Task<Option<IReadOnlyList<TOut>>> TraverseAsync<T, TOut>(
        IEnumerable<T> source, Func<T, CancellationToken, Task<Option<TOut>>> mapper, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var values = new List<TOut>();
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pending = mapper(element, cancellationToken);
            if (pending is null) throw new InvalidOperationException("The mapper returned a null task.");
            var option = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!option.TryGetValue(out var value)) return Option.None<IReadOnlyList<TOut>>();
            values.Add(value);
        }
        return Option.Some(Result.ToReadOnlyList(values));
    }
}
