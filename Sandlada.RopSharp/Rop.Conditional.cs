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
        return condition ? Ok<T>(value) : Err<T>(MaterializeError(errorFactory));
    }

    /// <summary>
    /// Creates a success when <paramref name="condition"/> is <see langword="true"/>; otherwise the supplied
    /// caller-defined error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="condition">The condition that selects the success track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition holds.</param>
    /// <param name="error">The error used when the condition does not hold. Must not be <see langword="null"/>.</param>
    /// <returns><c>Ok(value)</c> or <c>Err(error)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<T, TErr> SuccessIf<T, TErr>(bool condition, T value, TErr error)
    {
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        return condition ? Ok<T, TErr>(value) : Err<T, TErr>(error);
    }

    /// <summary>
    /// Creates a success when <paramref name="condition"/> is <see langword="true"/>; otherwise a lazily built
    /// caller-defined error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="condition">The condition that selects the success track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition holds.</param>
    /// <param name="errorFactory">Builds the error. Evaluated only when the condition does not hold.</param>
    /// <returns><c>Ok(value)</c> or <c>Err(errorFactory())</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    public static Result<T, TErr> SuccessIf<T, TErr>(bool condition, T value, Func<TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return condition ? Ok<T, TErr>(value) : Err<T, TErr>(MaterializeError(errorFactory));
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
        return condition ? Err<T>(MaterializeError(errorFactory)) : Ok<T>(value);
    }

    /// <summary>
    /// Creates a failure when <paramref name="condition"/> is <see langword="true"/>; otherwise the supplied value
    /// with a caller-defined error type.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="condition">The condition that selects the failure track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition does not hold.</param>
    /// <param name="error">The error used when the condition holds. Must not be <see langword="null"/>.</param>
    /// <returns><c>Err(error)</c> or <c>Ok(value)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<T, TErr> FailureIf<T, TErr>(bool condition, T value, TErr error)
    {
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        return condition ? Err<T, TErr>(error) : Ok<T, TErr>(value);
    }

    /// <summary>
    /// Creates a failure when <paramref name="condition"/> is <see langword="true"/>; otherwise the supplied value,
    /// using a lazily built caller-defined error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="condition">The condition that selects the failure track.</param>
    /// <param name="value">The success value. Must not be <see langword="null"/> when the condition does not hold.</param>
    /// <param name="errorFactory">Builds the error. Evaluated only when the condition holds.</param>
    /// <returns><c>Err(errorFactory())</c> or <c>Ok(value)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    public static Result<T, TErr> FailureIf<T, TErr>(bool condition, T value, Func<TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return condition ? Err<T, TErr>(MaterializeError(errorFactory)) : Ok<T, TErr>(value);
    }

    /// <summary>
    /// Parses a string into a value using the invariant culture.
    /// </summary>
    /// <typeparam name="T">The parsable value type.</typeparam>
    /// <param name="value">The text to parse.</param>
    /// <param name="code">The error code used when parsing fails. Defaults to <c>E_PARSE</c>.</param>
    /// <returns><c>Ok(parsed)</c> or <c>Err</c> with the supplied code.</returns>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    /// <remarks>
    /// A failure echoes the offending input in the message and in <see cref="RopError.Details"/> as
    /// <c>input=&lt;quoted&gt;</c>, so callers can correlate the error without logging the raw payload themselves. The
    /// echo is bounded and escapes control characters; <see langword="null"/> renders as the literal <c>null</c>
    /// instead of an empty pair of quotes.
    /// </remarks>
    public static Result<T, RopError> Parse<T>(string? value, string code = "E_PARSE")
        where T : IParsable<T>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (TryParse<T>(value).TryGetValue(out var parsed))
        {
            return Ok<T>(parsed);
        }

        var message = value is null
            ? $"The value is null, which is not a valid {typeof(T).Name}."
            : $"'{DiagnosticText.Escape(value)}' is not a valid {typeof(T).Name}.";
        return Err<T>(RopError.Create(code, message, $"input={DiagnosticText.Quote(value)}"));
    }

    /// <summary>
    /// Parses a string into a value using the invariant culture and maps a failure into a caller-defined error.
    /// </summary>
    /// <typeparam name="T">The parsable value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="value">The text to parse.</param>
    /// <param name="errorFactory">
    /// Builds the error from the complete failure payload. Evaluated only when parsing fails.
    /// </param>
    /// <param name="code">The code carried by the failure payload. Defaults to <c>E_PARSE</c>.</param>
    /// <returns><c>Ok(parsed)</c> or <c>Err(errorFactory(failure))</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <see cref="ParseFailure"/> payload carries the same bounded, escaped message and <c>input=&lt;quoted&gt;</c>
    /// details as the <see cref="RopError"/> overload, so callers with their own error type do not lose diagnostic
    /// information. <see cref="ParseFailure.Input"/> is the raw input; use <see cref="ParseFailure.Details"/> when the
    /// text is written to a log.
    /// </para>
    /// <para>
    /// Use this overload when the caller's error system is not <see cref="RopError"/>; the <see cref="RopError"/>
    /// overload remains the default.
    /// </para>
    /// </remarks>
    public static Result<T, TErr> Parse<T, TErr>(
        string? value,
        Func<ParseFailure, TErr> errorFactory,
        string code = "E_PARSE")
        where T : IParsable<T>
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        if (TryParse<T>(value).TryGetValue(out var parsed))
        {
            return Ok<T, TErr>(parsed);
        }

        var typeName = typeof(T).Name;
        var message = value is null
            ? $"The value is null, which is not a valid {typeName}."
            : $"'{DiagnosticText.Escape(value)}' is not a valid {typeName}.";

        return Err<T, TErr>(RequireError(errorFactory(new ParseFailure(code, message, value, typeName))));
    }

    /// <summary>
    /// Attempts to parse a string into a value using the invariant culture.
    /// </summary>
    /// <typeparam name="T">The parsable value type.</typeparam>
    /// <param name="value">The text to parse.</param>
    /// <returns><c>Some(parsed)</c> when parsing succeeds; otherwise <c>None</c>.</returns>
    /// <remarks>
    /// A null <typeparamref name="T"/> result is treated as a failed parse rather than being lifted into
    /// <c>Some(null)</c>: an option never contains null, so a third-party <see cref="IParsable{T}"/> that reports
    /// success with a null value yields <c>None</c>.
    /// </remarks>
    public static Option<T> TryParse<T>(string? value)
        where T : IParsable<T>
    {
        if (!T.TryParse(value, CultureInfo.InvariantCulture, out var parsed) || parsed is null)
        {
            return Opt.None<T>();
        }

        return Opt.Some(parsed);
    }
}
