namespace Sandlada.RopSharp;

/// <summary>
/// Extension helpers for <see cref="Validation{T,TErr}"/>.
/// </summary>
/// <remarks>
/// <code><![CDATA[
/// var count = Validation.Sequence(new[] { Result.Ok(1), Result.Ok(2) })
///     .Map(static lines => lines.Count)
///     .Expect("test asserts the validated count");
///
/// // Success(2) yields the mapped count,
/// // Failure([E_X]) throws when Expect is used on the failure track.
/// ]]></code>
/// </remarks>
public static class ValidationExtensions
{
    /// <summary>
    /// Returns the success value when this validation succeeded.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="validation">The validation.</param>
    /// <param name="reason">
    /// A non-empty explanation of why unwrapping is justified at this boundary. Included in the exception when the
    /// validation failed or is uninitialized.
    /// </param>
    /// <returns>The success value.</returns>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">
    /// The validation is a failure, uninitialized, or otherwise not on the success track.
    /// </exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var lines = Validation.Sequence(new[]
    ///     {
    ///         Result.Ok<int, RopError>(1),
    ///         Result.Ok<int, RopError>(2),
    ///     })
    ///     .Map(static lines => lines.Count)
    ///     .Expect("checkout asserts both lines validated");
    ///
    /// // Success(2) yields the count after Map on Success([1, 2]),
    /// // Failure([E_X]) throws and the message includes the reason and validation text.
    /// ]]></code>
    /// </remarks>
    public static T Expect<T, TErr>(this Validation<T, TErr> validation, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (validation.TryGetSuccess(out var value))
        {
            return value;
        }

        validation.ThrowIfUninitialized();
        throw new InvalidOperationException($"{reason}: {validation}");
    }
}
