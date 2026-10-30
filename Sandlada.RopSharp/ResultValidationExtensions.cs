namespace Sandlada.RopSharp;

/// <summary>
/// Conversions between the short-circuit track (<c>Result&lt;T, TErr&gt;</c>) and the aggregating track
/// (<c>Result&lt;T, IReadOnlyList&lt;TErr&gt;&gt;</c>).
/// </summary>
/// <remarks>
/// Conversions are always explicit: short-circuiting APIs never aggregate and aggregating APIs never
/// short-circuit, so the two models are never mixed implicitly.
/// <code><![CDATA[
/// var collapsed = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
///     .ToValidation()
///     .ToSingleError(RopError.Combine);
///
/// // Err(E_CHARGE) after the round trip through the aggregating track,
/// // Ok(payment) stays untouched - the conversion is explicit.
/// ]]></code>
/// </remarks>
public static class ResultValidationExtensions
{
    /// <summary>
    /// Lifts a single-error result onto the aggregating track.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns><c>Ok(value)</c> when successful; otherwise <c>Err([error])</c> (never empty).</returns>
    /// <remarks>
    /// A success that carries <see langword="null"/> after a nullable mapping is preserved as is; lifting a track
    /// never re-validates the payload.
    /// <code><![CDATA[
    /// var aggregated = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .ToValidation();
    ///
    /// // Ok(payment) stays on the success track,
    /// // Err([E_CHARGE]) wraps the single error in a list - the list is never empty.
    /// ]]></code>
    /// </remarks>
    public static Result<T, IReadOnlyList<TErr>> ToValidation<T, TErr>(this Result<T, TErr> result) =>
        result.TryGetOk(out var value)
            ? Rop.OkUnchecked<T, IReadOnlyList<TErr>>(value)
            : Rop.Err<T, IReadOnlyList<TErr>>([result.ErrUnsafe]);

    /// <summary>
    /// Collapses an aggregating result by keeping the first error.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The original success, or <c>Err</c> with the first error.</returns>
    /// <exception cref="InvalidOperationException">The error list is empty, which violates the aggregating invariant.</exception>
    /// <remarks>
    /// This is the default collapse: the first error is the one the caller cares about, and the rest is dropped.
    /// <code><![CDATA[
    /// var aggregated = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .ToValidation();
    /// var collapsed = aggregated.ToSingleError();
    ///
    /// // Ok(payment) stays untouched,
    /// // Err(E_CHARGE) keeps only the first error of the list.
    /// ]]></code>
    /// </remarks>
    public static Result<T, RopError> ToSingleError<T>(this Result<T, IReadOnlyList<RopError>> result) =>
        result.ToSingleError(static errors => errors[0]);

    /// <summary>
    /// Collapses an aggregating result with a caller-defined error type by keeping the first error.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The original success, or <c>Err</c> with the first error.</returns>
    /// <exception cref="InvalidOperationException">The error list is empty, which violates the aggregating invariant.</exception>
    /// <remarks>
    /// Use this overload when the caller's error system is not <see cref="RopError"/>. When a different collapse
    /// strategy is required, use <see cref="ToSingleError{T,TErr}(Result{T,IReadOnlyList{TErr}},Func{IReadOnlyList{TErr},TErr})"/>.
    /// <code><![CDATA[
    /// Result<Order, IReadOnlyList<string>> aggregated =
    ///     Rop.Err<Order, IReadOnlyList<string>>(["the order has no lines"]);
    /// var collapsed = aggregated.ToSingleError<Order, string>();
    ///
    /// // Ok(order) stays untouched,
    /// // Err(the order has no lines) keeps only the first error of the list.
    /// ]]></code>
    /// </remarks>
    public static Result<T, TErr> ToSingleError<T, TErr>(this Result<T, IReadOnlyList<TErr>> result) =>
        result.ToSingleError(static errors => errors[0]);

    /// <summary>
    /// Collapses an aggregating result with an explicit strategy.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="combine">Collapses the non-empty error list into a single error (for example <see cref="RopError.Combine"/>).</param>
    /// <returns>The original success, or <c>Err(combine(errors))</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="combine"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The error list is empty, which violates the aggregating invariant, or <paramref name="combine"/> returned <see langword="null"/>.</exception>
    /// <remarks>
    /// The strategy decides what the aggregated failures become. <see cref="RopError.Combine"/> keeps every
    /// individual failure reachable through <see cref="RopError.InnerErrors"/>.
    /// <code><![CDATA[
    /// var collapsed = Rop.Validate(
    ///         Rop.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Rop.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
    ///         static (order, amount) => order.Id)
    ///     .ToSingleError(RopError.Combine);
    ///
    /// // Ok(id) stays untouched,
    /// // Err(E_MULTIPLE) collapses the whole list into one error.
    /// ]]></code>
    /// </remarks>
    public static Result<T, TErr> ToSingleError<T, TErr>(
        this Result<T, IReadOnlyList<TErr>> result,
        Func<IReadOnlyList<TErr>, TErr> combine)
    {
        ArgumentNullException.ThrowIfNull(combine);
        if (result.TryGetOk(out var value))
        {
            return Rop.OkUnchecked<T, TErr>(value);
        }

        var errors = result.ErrUnsafe;
        if (errors is null || errors.Count == 0)
        {
            throw new InvalidOperationException(
                "An aggregating result must carry at least one error when it is on the failure track.");
        }

        return Rop.Err<T, TErr>(Rop.RequireError(combine(errors)));
    }
}
