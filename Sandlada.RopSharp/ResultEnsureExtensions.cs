namespace Sandlada.RopSharp;

/// <summary>
/// Additional guards and sequenced side-effect checks for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// <see cref="ResultExtensions.Ensure{TOk,TErr}(Result{TOk,TErr},Func{TOk,bool},TErr)"/> fails when a predicate is
/// <see langword="false"/>. <c>FailIf</c> expresses the opposite, more natural reading for rejection rules, while
/// <c>ThenEnsure</c> runs a result-returning check and keeps the original success value.
/// <code><![CDATA[
/// var placed = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
///     .FailIf(static payment => payment.Amount <= 0m, RopError.Create("E_AMOUNT", "the amount must be positive"));
///
/// // Ok(payment) when the amount is positive,
/// // Err(E_AMOUNT) when it is not - FailIf states the rejection rule.
/// ]]></code>
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
    /// <remarks>
    /// <c>FailIf</c> reads the way a rejection rule is usually written: "fail when the amount is not positive".
    /// The predicate never runs on the failure track.
    /// <code><![CDATA[
    /// var placed = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .FailIf(static payment => payment.Amount <= 0m, RopError.Create("E_AMOUNT", "the amount must be positive"));
    ///
    /// // Ok(payment) when the amount is positive,
    /// // Err(E_AMOUNT) when it is not - the predicate never runs on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// The factory is lazy, so an expensive error message is only built on the failing branch.
    /// <code><![CDATA[
    /// var placed = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .FailIf(
    ///         static payment => payment.Amount <= 0m,
    ///         static payment => RopError.Create("E_AMOUNT", $"the amount {payment.Amount} must be positive"));
    ///
    /// // Ok(payment) when the amount is positive,
    /// // Err(E_AMOUNT) when it is not - the factory runs only on the failing branch.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <c>ThenEnsure</c> is the sequenced form of the assertion: the check is itself a pipeline, so it can fail
    /// with its own error. The original success value survives a successful check.
    /// <code><![CDATA[
    /// var placed = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .ThenEnsure(static payment => Rop.Try(
    ///         () =>
    ///         {
    ///             if (payment.Amount <= 0m)
    ///             {
    ///                 throw new InvalidOperationException("the amount must be positive");
    ///             }
    ///         },
    ///         "E_AMOUNT"));
    ///
    /// // Ok(payment) when the amount is positive,
    /// // Err(E_AMOUNT) when it is not - the original success value is kept.
    /// ]]></code>
    /// </remarks>
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
