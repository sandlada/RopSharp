namespace Sandlada.RopSharp;

internal static class ValidationTaskCore
{
    internal static async Task<Validation<TOut, TErr>> Map<T, TOut, TErr>(
        Task<Validation<T, TErr>> source, Func<T, TOut> mapper, CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("Map");
        return validation.Map(mapper);
    }

    internal static async Task<Validation<TOut, TErr>> MapAsync<T, TOut, TErr>(
        Task<Validation<T, TErr>> source, Func<T, CancellationToken, Task<TOut>> mapper, CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!validation.TryGetSuccess(out var value))
        {
            return validation.Map<TOut>(static _ => default!);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var mapped = await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false);
        return Validation.Success<TOut, TErr>(mapped);
    }

    private static async Task<IReadOnlyList<TErr>> CopyErrorsAsync<T, TErr>(
        Validation<T, TErr> validation, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        return validation.TryGetErrors(out var errors) ? errors : Array.Empty<TErr>();
    }

    internal static async Task<Validation<TOut, TErr>> ZipAsync<T1, T2, TOut, TErr>(
        Task<Result<T1, TErr>> first, Task<Result<T2, TErr>> second, Func<T1, T2, TOut> merger, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(first, second).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Validation.Zip(await first.ConfigureAwait(false), await second.ConfigureAwait(false), merger);
    }

    internal static async Task<Validation<TOut, TErr>> ZipAsync<T1, T2, T3, TOut, TErr>(
        Task<Result<T1, TErr>> first, Task<Result<T2, TErr>> second, Task<Result<T3, TErr>> third,
        Func<T1, T2, T3, TOut> merger, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(first, second, third).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Validation.Zip(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            await third.ConfigureAwait(false),
            merger);
    }

    internal static async Task<Validation<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        IEnumerable<Task<Result<T, TErr>>> tasks, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = Result.CreateList<Task<Result<T, TErr>>, Task<Result<T, TErr>>>(tasks);
        foreach (var task in tasks)
        {
            if (task is null) throw new InvalidOperationException("The sequence contained a null task.");
            pending.Add(task);
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return Validation.Sequence(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    internal static async Task<Validation<IReadOnlyList<TOut>, TErr>> TraverseAsync<T, TOut, TErr>(
        IEnumerable<T> source, Func<T, CancellationToken, Task<Result<TOut, TErr>>> mapper, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = Result.CreateList<T, Task<Result<TOut, TErr>>>(source);
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(AsyncGuards.Require(mapper(element, cancellationToken), "mapper"));
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return Validation.Sequence(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }
}
