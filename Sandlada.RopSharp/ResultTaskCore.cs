namespace Sandlada.RopSharp;

internal static class ResultTaskCore
{
    internal static async Task<Result<TOut, TErr>> MapOk<TOk, TOut, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TOk, TOut> mapper,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Result.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("MapOk");
        return Result.OkUnchecked<TOut, TErr>(mapper(value));
    }

    internal static async Task<Result<TOut, TErr>> MapOkAsync<TOk, TOut, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TOk, CancellationToken, Task<TOut>> mapper,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Result.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var mapped = await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false);
        return Result.OkUnchecked<TOut, TErr>(mapped);
    }

    internal static async Task<Result<TOut, TErr>> BindOk<TOk, TOut, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TOk, Result<TOut, TErr>> binder,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Result.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("BindOk");
        return binder(value);
    }

    internal static async Task<Result<TOut, TErr>> BindOkAsync<TOk, TOut, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TOk, CancellationToken, Task<Result<TOut, TErr>>> binder,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Result.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(binder(value, cancellationToken), "binder").ConfigureAwait(false);
    }

    internal static async Task<Result<TOk, TErr2>> MapErr<TOk, TErr, TErr2>(
        Task<Result<TOk, TErr>> source,
        Func<TErr, TErr2> mapper,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return Result.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Result.Err<TOk, TErr2>(Result.RequireError(mapper(error)));
    }

    internal static async Task<Result<TOk, TErr2>> MapErrAsync<TOk, TErr, TErr2>(
        Task<Result<TOk, TErr>> source,
        Func<TErr, CancellationToken, Task<TErr2>> mapper,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return Result.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var mapped = await AsyncGuards.Require(mapper(error, cancellationToken), "mapper").ConfigureAwait(false);
        return Result.Err<TOk, TErr2>(Result.RequireError(mapped));
    }

    internal static async Task<Result<TOk, TErr>> BindErr<TOk, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TErr, Result<TOk, TErr>> binder,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return binder(error);
    }

    internal static async Task<Result<TOk, TErr>> BindErrAsync<TOk, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TErr, CancellationToken, Task<Result<TOk, TErr>>> binder,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(binder(error, cancellationToken), "binder").ConfigureAwait(false);
    }

    internal static async Task<Result<TOk, TErr>> TeeOk<TOk, TErr>(
        Task<Result<TOk, TErr>> source,
        Action<TOk> action,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetOk(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(value);
        }

        return result;
    }

    internal static async Task<Result<TOk, TErr>> TeeOkAsync<TOk, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TOk, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetOk(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value, cancellationToken), "action").ConfigureAwait(false);
        }

        return result;
    }

    internal static async Task<Result<TOk, TErr>> TeeErr<TOk, TErr>(
        Task<Result<TOk, TErr>> source,
        Action<TErr> action,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetErr(out var error))
        {
            cancellationToken.ThrowIfCancellationRequested();
            action(error);
        }

        return result;
    }

    internal static async Task<Result<TOk, TErr>> TeeErrAsync<TOk, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TErr, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetErr(out var error))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(error, cancellationToken), "action").ConfigureAwait(false);
        }

        return result;
    }

    internal static async Task<Result<TOut, TErr>> CombineAsync<T1, T2, TOut, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(first, second).WaitAsync(cancellationToken).ConfigureAwait(false);
        var a = await first.ConfigureAwait(false);
        var b = await second.ConfigureAwait(false);
        return Result.Combine(a, b, merger);
    }

    internal static async Task<Result<TOut, TErr>> CombineAsync<T1, T2, T3, TOut, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Task<Result<T3, TErr>> third,
        Func<T1, T2, T3, TOut> merger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.WhenAll(first, second, third).WaitAsync(cancellationToken).ConfigureAwait(false);
        var a = await first.ConfigureAwait(false);
        var b = await second.ConfigureAwait(false);
        var c = await third.ConfigureAwait(false);
        return Result.Combine(a, b, c, merger);
    }

    internal static async Task<Result<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        IEnumerable<ResultTask<T, TErr>> computations,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var values = new List<T>();
        foreach (var computation in computations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            computation.ThrowIfUninitialized();
            var task = computation.TaskUnsafe;
            if (task is null)
            {
                throw new InvalidOperationException("The sequence contained a null task.");
            }

            var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!result.TryGetOk(out var value))
            {
                return Result.Err<IReadOnlyList<T>, TErr>(result.ErrUnsafe);
            }

            values.Add(value);
        }

        return Result.Ok<IReadOnlyList<T>, TErr>(Result.ToReadOnlyList(values));
    }

    internal static async Task<Result<IReadOnlyList<TOut>, TErr>> TraverseAsync<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, CancellationToken, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var values = new List<TOut>();
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pending = mapper(element, cancellationToken);
            if (pending is null)
            {
                throw new InvalidOperationException("The mapper returned a null task.");
            }

            var result = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!result.TryGetOk(out var value))
            {
                return Result.Err<IReadOnlyList<TOut>, TErr>(result.ErrUnsafe);
            }

            values.Add(value);
        }

        return Result.Ok<IReadOnlyList<TOut>, TErr>(Result.ToReadOnlyList(values));
    }

    internal static async Task SwitchAsync<TOk, TErr>(
        Task<Result<TOk, TErr>> source,
        Func<TOk, CancellationToken, Task> ok,
        Func<TErr, CancellationToken, Task> err,
        CancellationToken cancellationToken)
    {
        var result = await source.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (result.TryGetOk(out var value))
        {
            await AsyncGuards.Require(ok(value, cancellationToken), "ok").ConfigureAwait(false);
        }
        else
        {
            await AsyncGuards.Require(err(result.ErrUnsafe, cancellationToken), "err").ConfigureAwait(false);
        }
    }

    internal static async Task<Result<(IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errs), TErr>> PartitionAsync<T, TErr>(
        IEnumerable<ResultTask<T, TErr>> computations,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(computations);
        cancellationToken.ThrowIfCancellationRequested();

        var pending = new List<Task<Result<T, TErr>>>();
        foreach (var computation in computations)
        {
            computation.ThrowIfUninitialized();
            pending.Add(computation.TaskUnsafe);
        }

        var results = await Task.WhenAll(pending).WaitAsync(cancellationToken).ConfigureAwait(false);
        var split = Result.Partition(results);
        return Result.Ok<(IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errs), TErr>(split);
    }
}
