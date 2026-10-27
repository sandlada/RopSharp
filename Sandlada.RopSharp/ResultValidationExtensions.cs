namespace Sandlada.RopSharp;

/// <summary>
/// Conversions between the short-circuit track (<c>Result&lt;T, TErr&gt;</c>) and the aggregating track
/// (<c>Result&lt;T, IReadOnlyList&lt;TErr&gt;&gt;</c>).
/// </summary>
/// <remarks>
/// Conversions are always explicit: short-circuiting APIs never aggregate and aggregating APIs never
/// short-circuit, so the two models are never mixed implicitly.
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
    public static Result<T, RopError> ToSingleError<T>(this Result<T, IReadOnlyList<RopError>> result) =>
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
    /// <exception cref="InvalidOperationException">The error list is empty, which violates the aggregating invariant.</exception>
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

        return Rop.Err<T, TErr>(combine(errors));
    }
}
