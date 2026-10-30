namespace Sandlada.RopSharp;

/// <summary>
/// Action-based terminals for <see cref="Result{TOk,TErr}"/>. Use these when both tracks end in a side effect
/// instead of a projected value; use <see cref="ResultExtensions.Match{TOk,TErr,T}"/> when a value is required.
/// </summary>
/// <remarks>
/// <code><![CDATA[
/// Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
///     .Switch(
///         ok: static payment => Console.WriteLine($"captured {payment.Id}"),
///         err: static error => Console.WriteLine($"failed {error.Code}"));
///
/// // Ok(payment) runs the success action,
/// // Err(E_CHARGE) runs the error action - Switch ends the pipeline.
/// ]]></code>
/// </remarks>
public static class ResultTerminalExtensions
{
    /// <summary>
    /// Runs the action for the active track.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="ok">The action for a success. Exceptions propagate.</param>
    /// <param name="err">The action for an error. Exceptions propagate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="err"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Exactly one of the two actions runs. Use it when the pipeline ends in a side effect - a log line, a
    /// notification - and there is no value left to hand back.
    /// <code><![CDATA[
    /// Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .Switch(
    ///         ok: static payment => Console.WriteLine($"captured {payment.Id}"),
    ///         err: static error => Console.WriteLine($"failed {error.Code}"));
    ///
    /// // Ok(payment) runs the success action,
    /// // Err(E_CHARGE) runs the error action - exactly one of them runs.
    /// ]]></code>
    /// </remarks>
    public static void Switch<TOk, TErr>(
        this Result<TOk, TErr> result,
        Action<TOk> ok,
        Action<TErr> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);
        if (result.TryGetOk(out var value))
        {
            ok(value);
        }
        else
        {
            err(result.ErrUnsafe);
        }
    }

    /// <summary>
    /// Runs the asynchronous action for the active track of a task-wrapped result.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="ok">The action for a success. Exceptions propagate.</param>
    /// <param name="err">The action for an error. Exceptions propagate.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>A task that completes after the selected action.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the selected action ran.</exception>
    /// <remarks>
    /// The task is awaited before the action runs, so the action always sees a decided result. The token is
    /// observed both while waiting and right before the action starts.
    /// <code><![CDATA[
    /// await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .SwitchAsync(
    ///         ok: static payment => Task.CompletedTask,
    ///         err: static error => Task.CompletedTask);
    ///
    /// // Ok(payment) runs the success action,
    /// // Err(E_CHARGE) runs the error action - exactly one of them runs.
    /// ]]></code>
    /// </remarks>
    public static async Task SwitchAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, Task> ok,
        Func<TErr, Task> err,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (result.TryGetOk(out var value))
        {
            await AsyncGuards.Require(ok(value), "ok").ConfigureAwait(false);
        }
        else
        {
            await AsyncGuards.Require(err(result.ErrUnsafe), "err").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Projects the active track of an aggregating result, using only the first error on the failure track.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="result">The aggregating result.</param>
    /// <param name="ok">Projects a success.</param>
    /// <param name="err">Projects the first error.</param>
    /// <returns>The projected value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="err"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The failure track carries an empty error list.</exception>
    /// <remarks>
    /// <c>MatchFirst</c> is the exit for an aggregating result that has to be reduced to a single error. It is
    /// the counterpart of <see cref="ResultExtensions.Match{TOk,TErr,T}"/> for the collecting track.
    /// <code><![CDATA[
    /// var validated = Rop.Validate(
    ///         Rop.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Rop.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
    ///         static (order, amount) => order.Id);
    ///
    /// var label = validated.MatchFirst(
    ///     ok: static id => $"placed {id}",
    ///     err: static error => $"failed {error.Code}");
    ///
    /// // "placed ord-1001" for Ok(id),
    /// // "failed E_AMOUNT" for Err([E_AMOUNT]) - MatchFirst uses only the first error.
    /// ]]></code>
    /// </remarks>
    public static TResult MatchFirst<T, TErr, TResult>(
        this Result<T, IReadOnlyList<TErr>> result,
        Func<T, TResult> ok,
        Func<TErr, TResult> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);
        if (result.TryGetOk(out var value))
        {
            return ok(value);
        }

        return err(FirstError(result.ErrUnsafe));
    }

    /// <summary>
    /// Runs the action for the active track of an aggregating result, using only the first error.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The aggregating result.</param>
    /// <param name="ok">The action for a success. Exceptions propagate.</param>
    /// <param name="err">The action for the first error. Exceptions propagate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="err"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The failure track carries an empty error list.</exception>
    /// <remarks>
    /// <c>SwitchFirst</c> is the action-based counterpart of <see cref="MatchFirst{T,TErr,TResult}"/> for the
    /// collecting track.
    /// <code><![CDATA[
    /// var validated = Rop.Validate(
    ///         Rop.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Rop.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
    ///         static (order, amount) => order.Id);
    ///
    /// validated.SwitchFirst(
    ///     ok: static id => Console.WriteLine($"placed {id}"),
    ///     err: static error => Console.WriteLine($"failed {error.Code}"));
    ///
    /// // Ok(id) runs the success action,
    /// // Err([E_AMOUNT]) runs the error action for the first error only.
    /// ]]></code>
    /// </remarks>
    public static void SwitchFirst<T, TErr>(
        this Result<T, IReadOnlyList<TErr>> result,
        Action<T> ok,
        Action<TErr> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);
        if (result.TryGetOk(out var value))
        {
            ok(value);
        }
        else
        {
            err(FirstError(result.ErrUnsafe));
        }
    }

    private static TErr FirstError<TErr>(IReadOnlyList<TErr> errors)
    {
        if (errors is null || errors.Count == 0)
        {
            throw new InvalidOperationException(
                "An aggregating result must carry at least one error when it is on the failure track.");
        }

        return errors[0];
    }
}
