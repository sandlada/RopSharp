using System.Globalization;

namespace Sandlada.RopSharp;

/// <summary>
/// Conditional construction and parsing helpers for the <see cref="Result{TOk,TErr}"/> and
/// <see cref="Option{T}"/> domains.
/// </summary>
public static partial class Rop
{
    /// <summary>
    /// Creates a success when <paramref name="condition"/> is <see langword="true"/>; otherwise the supplied error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="condition">The condition that selects the success track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition holds.</param>
    /// <param name="error">The error used when the condition does not hold. Must not be <see langword="null"/>.</param>
    /// <returns><c>Ok(value)</c> or <c>Err(error)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<T, RopError> SuccessIf<T>(bool condition, T value, RopError error)
    {
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        return condition ? Ok<T>(value) : Err<T>(error);
    }

    /// <summary>
    /// Creates a success when <paramref name="condition"/> is <see langword="true"/>; otherwise a lazily built error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="condition">The condition that selects the success track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition holds.</param>
    /// <param name="errorFactory">Builds the error. Evaluated only when the condition does not hold.</param>
    /// <returns><c>Ok(value)</c> or <c>Err(errorFactory())</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    public static Result<T, RopError> SuccessIf<T>(bool condition, T value, Func<RopError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return condition ? Ok<T>(value) : Err<T>(Materialize(errorFactory));
    }

    /// <summary>
    /// Creates a failure when <paramref name="condition"/> is <see langword="true"/>; otherwise the supplied value.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="condition">The condition that selects the failure track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition does not hold.</param>
    /// <param name="error">The error used when the condition holds. Must not be <see langword="null"/>.</param>
    /// <returns><c>Err(error)</c> or <c>Ok(value)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<T, RopError> FailureIf<T>(bool condition, T value, RopError error)
    {
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        return condition ? Err<T>(error) : Ok<T>(value);
    }

    /// <summary>
    /// Creates a failure when <paramref name="condition"/> is <see langword="true"/>; otherwise the supplied value.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="condition">The condition that selects the failure track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition does not hold.</param>
    /// <param name="errorFactory">Builds the error. Evaluated only when the condition holds.</param>
    /// <returns><c>Err(errorFactory())</c> or <c>Ok(value)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    public static Result<T, RopError> FailureIf<T>(bool condition, T value, Func<RopError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return condition ? Err<T>(Materialize(errorFactory)) : Ok<T>(value);
    }

    /// <summary>
    /// Parses a string into a value using the invariant culture.
    /// </summary>
    /// <typeparam name="T">The parsable value type.</typeparam>
    /// <param name="value">The text to parse.</param>
    /// <param name="code">The error code used when parsing fails. Defaults to <c>E_PARSE</c>.</param>
    /// <returns><c>Ok(parsed)</c> or <c>Err</c> with the supplied code.</returns>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    public static Result<T, RopError> Parse<T>(string? value, string code = "E_PARSE")
        where T : IParsable<T>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return TryParse<T>(value).TryGetValue(out var parsed)
            ? Ok<T>(parsed)
            : Err<T>(RopError.Create(code, $"'{value}' is not a valid {typeof(T).Name}."));
    }

    /// <summary>
    /// Attempts to parse a string into a value using the invariant culture.
    /// </summary>
    /// <typeparam name="T">The parsable value type.</typeparam>
    /// <param name="value">The text to parse.</param>
    /// <returns><c>Some(parsed)</c> when parsing succeeds; otherwise <c>None</c>.</returns>
    public static Option<T> TryParse<T>(string? value)
        where T : IParsable<T> =>
        T.TryParse(value, CultureInfo.InvariantCulture, out var parsed)
            ? Opt.Some(parsed)
            : Opt.None<T>();
}
