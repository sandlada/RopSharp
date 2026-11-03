namespace Sandlada.RopSharp;

/// <summary>
/// Action-based terminals for <see cref="Result{TOk,TErr}"/>. Use these when both tracks end in a side effect
/// instead of a projected value; use <see cref="ResultExtensions.Match{TOk,TErr,T}"/> when a value is required.
/// </summary>
/// <remarks>
/// <code><![CDATA[
/// Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
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
    /// Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
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
    /// await Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
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
}
