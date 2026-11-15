namespace Sandlada.RopSharp;

/// <summary>Combinators for <see cref="ValidationTask{T,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
///     .Map(static order => order.Id);
///
/// // Success("ord-1001") on the success track,
/// // Failure([E_X]) preserves every collected error.
/// ]]></code>
/// </remarks>
public readonly partial struct ValidationTask<T, TErr>
{
    /// <summary>Maps the success value when validation succeeds.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") when the validation succeeds,
    /// // Failure([E_X]) when it already failed - the mapper never runs on Failure.
    /// ]]></code>
    /// </remarks>
    public ValidationTask<TOut, TErr> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("Map");
        return new(ValidationTaskCore.Map(_task!, mapper, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Maps the success value with an asynchronous mapper that receives the computation token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .MapAsync(static (order, _) => Task.FromResult(order.Id));
    ///
    /// // Success("ord-1001") when the validation succeeds,
    /// // Failure([E_X]) when it already failed - the asynchronous mapper never runs on Failure.
    /// ]]></code>
    /// </remarks>
    public ValidationTask<TOut, TErr> MapAsync<TOut>(Func<T, CancellationToken, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();
        return new(ValidationTaskCore.MapAsync(_task!, mapper, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Merges all collected errors into a single error on completion and exits to <see cref="ResultTask{TOk,TErr}"/>.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .Collapse(RopError.Combine);
    ///
    /// // Ok(order) on success,
    /// // Err(E_MULTIPLE) when Failure errors are merged into one.
    /// ]]></code>
    /// </remarks>
    public ResultTask<T, TErr> Collapse(Func<IReadOnlyList<TErr>, TErr> merge)
    {
        ArgumentNullException.ThrowIfNull(merge);
        ThrowIfUninitialized();
        return ResultTask<T, TErr>.From(ValidationTaskCore.CollapseAsync(_task!, merge, _cancellationToken))
            .WithCancellation(_cancellationToken);
    }

    /// <summary>Asynchronously projects the success value or the full list of collected errors.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .MatchAsync(
    ///         ok: static (order, _) => Task.FromResult($"placed {order.Id}"),
    ///         errors: static (errs, _) => Task.FromResult($"failed {errs.Count}"));
    ///
    /// // "placed ord-1001" for Success(order),
    /// // "failed N" for Failure([E_X]).
    /// ]]></code>
    /// </remarks>
    public Task<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, Task<TResult>> ok,
        Func<IReadOnlyList<TErr>, CancellationToken, Task<TResult>> errors)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(errors);
        ThrowIfUninitialized();
        return ValidationTaskCore.MatchAsync(_task!, ok, errors, _cancellationToken);
    }

    /// <summary>Asynchronously projects the success value or only the first collected error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .MatchFirstAsync(
    ///         ok: static (order, _) => Task.FromResult($"placed {order.Id}"),
    ///         err: static (error, _) => Task.FromResult($"failed {error.Code}"));
    ///
    /// // "placed ord-1001" for Success(order),
    /// // "failed E_X" for Failure([E_X]) - passes only the first error.
    /// ]]></code>
    /// </remarks>
    public Task<TResult> MatchFirstAsync<TResult>(
        Func<T, CancellationToken, Task<TResult>> ok,
        Func<TErr, CancellationToken, Task<TResult>> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);
        ThrowIfUninitialized();
        return ValidationTaskCore.MatchFirstAsync(_task!, ok, err, _cancellationToken);
    }

    /// <summary>Runs one of two asynchronous actions for a success or the full list of collected errors.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// await ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .SwitchAsync(
    ///         ok: static (order, _) => Task.Run(() => Console.WriteLine(order.Id)),
    ///         errors: static (errs, _) => Task.Run(() => Console.WriteLine(errs.Count)));
    ///
    /// // Success(order) runs the ok handler,
    /// // Failure([E_X]) runs the errors handler once validation completes.
    /// ]]></code>
    /// </remarks>
    public Task SwitchAsync(
        Func<T, CancellationToken, Task> ok,
        Func<IReadOnlyList<TErr>, CancellationToken, Task> errors)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(errors);
        ThrowIfUninitialized();
        return ValidationTaskCore.SwitchAsync(_task!, ok, errors, _cancellationToken);
    }

    /// <summary>Runs one of two asynchronous actions for a success or only the first collected error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// await ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .SwitchFirstAsync(
    ///         ok: static (order, _) => Task.Run(() => Console.WriteLine(order.Id)),
    ///         err: static (error, _) => Task.Run(() => Console.WriteLine(error.Code)));
    ///
    /// // Success(order) runs ok,
    /// // Failure([E_X]) runs err with the first collected error.
    /// ]]></code>
    /// </remarks>
    public Task SwitchFirstAsync(
        Func<T, CancellationToken, Task> ok,
        Func<TErr, CancellationToken, Task> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);
        ThrowIfUninitialized();
        return ValidationTaskCore.SwitchFirstAsync(_task!, ok, err, _cancellationToken);
    }

    /// <summary>Awaits the validation and returns the success value, or throws on failure.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var order = await ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .ExpectAsync("order must pass all validation checks");
    ///
    /// // Success(order) returns order,
    /// // Failure([E_X]) throws InvalidOperationException with the reason.
    /// ]]></code>
    /// </remarks>
    public Task<T> ExpectAsync(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ThrowIfUninitialized();
        return ValidationTaskCore.ExpectAsync(_task!, reason, _cancellationToken);
    }
}
