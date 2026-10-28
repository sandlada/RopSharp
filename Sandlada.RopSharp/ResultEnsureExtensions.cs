namespace Sandlada.RopSharp;

/// <summary>
/// Additional guards and sequenced side-effect checks for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// <see cref="ResultExtensions.Ensure{TOk,TErr}(Result{TOk,TErr},Func{TOk,bool},TErr)"/> fails when a predicate is
/// <see langword="false"/>. <c>FailIf</c> expresses the opposite, more natural reading for rejection rules, while
/// <c>ThenEnsure</c> runs a result-returning check and keeps the original success value.
/// </remarks>
public static class ResultEnsureExtensions
{
    /// <summary>
    /// Turns a success into a failure when a predicate holds.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">The rejection rule. Not evaluated on the failure track.</param>
    /// <param name="error">The error used when the predicate holds. Must not be <see langword="null"/>.</param>
    /// <returns>The original success when the predicate does not hold; otherwise <c>Err(error)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<TOk, TErr> FailIf<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, bool> predicate,
        TErr error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        return result.TryGetOk(out var value) && predicate(value)
            ? Rop.Err<TOk, TErr>(error)
            : result;
    }

    /// <summary>
    /// Turns a success into a failure when a predicate holds, building the error lazily.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">The rejection rule. Not evaluated on the failure track.</param>
    /// <param name="errorFactory">Builds the error from the value. Evaluated only when the predicate holds.</param>
    /// <returns>The original success when the predicate does not hold; otherwise <c>Err(errorFactory(value))</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    public static Result<TOk, TErr> FailIf<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, bool> predicate,
        Func<TOk, TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        return result.TryGetOk(out var value) && predicate(value)
            ? Rop.Err<TOk, TErr>(Rop.RequireError(errorFactory(value)))
            : result;
    }

    /// <summary>
    /// Runs a result-returning check on a success and keeps the original value when the check succeeds.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="ensure">The check. Its success payload is ignored; its error becomes the result error.</param>
    /// <returns>The original success when <paramref name="ensure"/> succeeds; otherwise its error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ensure"/> is <see langword="null"/>.</exception>
    public static Result<TOk, TErr> ThenEnsure<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<Unit, TErr>> ensure)
    {
        ArgumentNullException.ThrowIfNull(ensure);
        if (!result.TryGetOk(out var value))
        {
            return result;
        }

        var checkedResult = ensure(value);
        return checkedResult.IsOk ? result : Rop.Err<TOk, TErr>(checkedResult.ErrUnsafe);
    }
}
