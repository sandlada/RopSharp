namespace Sandlada.RopSharp;

internal static class ValidationTaskCore
{
    internal static async Task<Validation<TOut, TErr>> Map<T, TOut, TErr>(
        Task<Validation<T, TErr>> source, Func<T, TOut> mapper, CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
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
        var mapped = ArgumentGuards.RequireNonNullReturn(
            await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false),
            "mapper");
        return new Validation<TOut, TErr>(mapped, Array.Empty<TErr>(), Validation<TOut, TErr>.TagSuccess);
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
        IEnumerable<ResultTask<T, TErr>> computations, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = Result.CreateList<ResultTask<T, TErr>, Task<Result<T, TErr>>>(computations);
        foreach (var computation in computations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(computation.ObservedTask);
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

    internal static async Task<Validation<TOut, TErr>> ZipAsync<T1, T2, TOut, TErr>(
        Task<Validation<T1, TErr>> first,
        Task<Validation<T2, TErr>> second,
        Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(first, second).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Validation.Zip(await first.ConfigureAwait(false), await second.ConfigureAwait(false), merger);
    }

    internal static async Task<Validation<TOut, TErr>> ZipAsync<T1, T2, T3, TOut, TErr>(
        Task<Validation<T1, TErr>> first,
        Task<Validation<T2, TErr>> second,
        Task<Validation<T3, TErr>> third,
        Func<T1, T2, T3, TOut> merger,
        CancellationToken cancellationToken)
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
        IEnumerable<ValidationTask<T, TErr>> computations,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = Result.CreateList<ValidationTask<T, TErr>, Task<Validation<T, TErr>>>(computations);
        foreach (var computation in computations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(computation.ObservedTask);
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return Validation.Sequence(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    internal static async Task<Validation<IReadOnlyList<TOut>, TErr>> TraverseAsync<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, CancellationToken, Task<Validation<TOut, TErr>>> mapper,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = Result.CreateList<T, Task<Validation<TOut, TErr>>>(source);
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(AsyncGuards.Require(mapper(element, cancellationToken), "mapper"));
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return Validation.Sequence(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    internal static async Task<Result<T, TErr>> CollapseAsync<T, TErr>(
        Task<Validation<T, TErr>> source,
        Func<IReadOnlyList<TErr>, TErr> merge,
        CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return validation.Collapse(merge);
    }

    internal static async Task<TResult> MatchAsync<T, TResult, TErr>(
        Task<Validation<T, TErr>> source,
        Func<T, CancellationToken, Task<TResult>> ok,
        Func<IReadOnlyList<TErr>, CancellationToken, Task<TResult>> errors,
        CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return validation.TryGetSuccess(out var value)
            ? await AsyncGuards.Require(ok(value, cancellationToken), "ok").ConfigureAwait(false)
            : await AsyncGuards.Require(errors(validation.ErrorsUnsafe, cancellationToken), "errors").ConfigureAwait(false);
    }

    internal static async Task<TResult> MatchFirstAsync<T, TResult, TErr>(
        Task<Validation<T, TErr>> source,
        Func<T, CancellationToken, Task<TResult>> ok,
        Func<TErr, CancellationToken, Task<TResult>> err,
        CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return validation.TryGetSuccess(out var value)
            ? await AsyncGuards.Require(ok(value, cancellationToken), "ok").ConfigureAwait(false)
            : await AsyncGuards.Require(err(GetFirstError(validation), cancellationToken), "err").ConfigureAwait(false);
    }

    internal static async Task SwitchAsync<T, TErr>(
        Task<Validation<T, TErr>> source,
        Func<T, CancellationToken, Task> ok,
        Func<IReadOnlyList<TErr>, CancellationToken, Task> errors,
        CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var pending = validation.TryGetSuccess(out var value)
            ? AsyncGuards.Require(ok(value, cancellationToken), "ok")
            : AsyncGuards.Require(errors(validation.ErrorsUnsafe, cancellationToken), "errors");
        await pending.ConfigureAwait(false);
    }

    internal static async Task SwitchFirstAsync<T, TErr>(
        Task<Validation<T, TErr>> source,
        Func<T, CancellationToken, Task> ok,
        Func<TErr, CancellationToken, Task> err,
        CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var pending = validation.TryGetSuccess(out var value)
            ? AsyncGuards.Require(ok(value, cancellationToken), "ok")
            : AsyncGuards.Require(err(GetFirstError(validation), cancellationToken), "err");
        await pending.ConfigureAwait(false);
    }

    internal static async Task<T> ExpectAsync<T, TErr>(
        Task<Validation<T, TErr>> source,
        string reason,
        CancellationToken cancellationToken)
    {
        var validation = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        return validation.Expect(reason);
    }

    private static TErr GetFirstError<T, TErr>(Validation<T, TErr> validation)
    {
        var errors = validation.ErrorsUnsafe;
        if (errors.Count == 0)
        {
            throw new InvalidOperationException(
                "A validation failure must carry at least one error when it is on the failure track.");
        }

        return errors[0];
    }
}
