using System.Globalization;

namespace Sandlada.RopSharp;

/// <summary>
/// Conditional construction and parsing helpers for the <see cref="Result{TOk,TErr}"/> and
/// <see cref="Option{T}"/> domains.
/// </summary>
/// <remarks>
/// <para>
/// A condition decides the track of a result before the pipeline starts. It is the shortest way to turn a
/// business rule such as "the order must have lines" into a value the rest of the code can compose with.
/// </para>
/// <code><![CDATA[
/// var placed = Rop.SuccessIf(
///         OrderFixtures.Sample.ItemCount > 0,
///         OrderFixtures.Sample,
///         RopError.Create("E_EMPTY_ORDER", "the order has no lines"))
///     .MapOk(static order => order.Id);
///
/// // Ok("ord-1001") when the order has lines,
/// // Err(E_EMPTY_ORDER) when it has none - the condition selects the track.
/// ]]></code>
/// </remarks>
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
    /// <remarks>
    /// The error is supplied eagerly, so use it when the error value is cheap. Use the factory overload when it
    /// is not.
    /// <code><![CDATA[
    /// var placed = Rop.SuccessIf(
    ///         OrderFixtures.Sample.ItemCount > 0,
    ///         OrderFixtures.Sample,
    ///         RopError.Create("E_EMPTY_ORDER", "the order has no lines"))
    ///     .MapOk(static order => order.Id);
    ///
    /// // Ok("ord-1001") when the order has lines,
    /// // Err(E_EMPTY_ORDER) when it has none - the condition selects the track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// The factory is lazy, so an expensive error is only built when the condition actually fails.
    /// <code><![CDATA[
    /// var placed = Rop.SuccessIf(
    ///         OrderFixtures.Sample.ItemCount > 0,
    ///         OrderFixtures.Sample,
    ///         static () => RopError.Create("E_EMPTY_ORDER", "the order has no lines"))
    ///     .MapOk(static order => order.Id);
    ///
    /// // Ok("ord-1001") when the order has lines,
    /// // Err(E_EMPTY_ORDER) when it has none - the factory runs only on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// Use this overload when the caller's error system is not <see cref="RopError"/>.
    /// <code><![CDATA[
    /// var placed = Rop.SuccessIf<string, string>(
    ///         OrderFixtures.Sample.ItemCount > 0,
    ///         OrderFixtures.Sample.Id,
    ///         "the order has no lines")
    ///     .MapOk(static id => id.ToUpperInvariant());
    ///
    /// // Ok("ORD-1001") when the order has lines,
    /// // Err(the order has no lines) when it has none - a custom error type keeps the reason verbatim.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// The factory is lazy, so an expensive error is only built when the condition actually fails.
    /// <code><![CDATA[
    /// var placed = Rop.SuccessIf<string, string>(
    ///         OrderFixtures.Sample.ItemCount > 0,
    ///         OrderFixtures.Sample.Id,
    ///         static () => "the order has no lines")
    ///     .MapOk(static id => id.ToUpperInvariant());
    ///
    /// // Ok("ORD-1001") when the order has lines,
    /// // Err(the order has no lines) when it has none - the factory runs only on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <c>FailureIf</c> inverts the usual success condition. It reads best when the failure is the exceptional
    /// case that has to be spelled out.
    /// <code><![CDATA[
    /// var charged = Rop.FailureIf(
    ///         OrderFixtures.Sample.Id.EndsWith("-declined", StringComparison.Ordinal),
    ///         OrderFixtures.Sample,
    ///         RopError.Create("E_PAYMENT_DECLINED", "the provider refused the order"))
    ///     .MapOk(static order => order.Id);
    ///
    /// // Err(E_PAYMENT_DECLINED) when the condition holds,
    /// // Ok("ord-1001") otherwise - FailureIf inverts the usual success condition.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// The factory is lazy, so an expensive error is only built when the condition actually holds.
    /// <code><![CDATA[
    /// var charged = Rop.FailureIf(
    ///         OrderFixtures.Sample.Id.EndsWith("-declined", StringComparison.Ordinal),
    ///         OrderFixtures.Sample,
    ///         static () => RopError.Create("E_PAYMENT_DECLINED", "the provider refused the order"))
    ///     .MapOk(static order => order.Id);
    ///
    /// // Err(E_PAYMENT_DECLINED) when the condition holds,
    /// // Ok("ord-1001") otherwise - the factory runs only on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// Use this overload when the caller's error system is not <see cref="RopError"/>.
    /// <code><![CDATA[
    /// var charged = Rop.FailureIf<string, string>(
    ///         OrderFixtures.Sample.Id.EndsWith("-declined", StringComparison.Ordinal),
    ///         OrderFixtures.Sample.Id,
    ///         "the provider refused the order")
    ///     .MapOk(static id => id.ToUpperInvariant());
    ///
    /// // Err(the provider refused the order) when the condition holds,
    /// // Ok("ORD-1001") otherwise - a custom error type keeps the reason verbatim.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// The factory is lazy, so an expensive error is only built when the condition actually holds.
    /// <code><![CDATA[
    /// var charged = Rop.FailureIf<string, string>(
    ///         OrderFixtures.Sample.Id.EndsWith("-declined", StringComparison.Ordinal),
    ///         OrderFixtures.Sample.Id,
    ///         static () => "the provider refused the order")
    ///     .MapOk(static id => id.ToUpperInvariant());
    ///
    /// // Err(the provider refused the order) when the condition holds,
    /// // Ok("ORD-1001") otherwise - the factory runs only on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <code><![CDATA[
    /// var amount = Rop.Parse<decimal>("19.50")
    ///     .MapOk(static value => value.ToString("0.00", CultureInfo.InvariantCulture));
    ///
    /// // Ok("19.50") when the text is a number,
    /// // Err(E_PARSE) when it is not - the failing input is echoed in the error details.
    /// ]]></code>
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
    /// <code><![CDATA[
    /// var amount = Rop.Parse<decimal, string>("19.50", static failure => failure.Code)
    ///     .MapOk(static value => value.ToString("0.00", CultureInfo.InvariantCulture));
    ///
    /// // Ok("19.50") when the text is a number,
    /// // Err(E_PARSE) when it is not - the failure payload carries the diagnostics of the attempt.
    /// ]]></code>
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
    /// <code><![CDATA[
    /// var amount = Rop.TryParse<decimal>("19.50")
    ///     .BindNoneValue(0m);
    ///
    /// // Some(19.50) when the text is a number,
    /// // None becomes 0m - a parse failure is not an error, it is an absent value.
    /// ]]></code>
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
