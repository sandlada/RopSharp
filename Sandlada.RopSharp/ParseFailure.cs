namespace Sandlada.RopSharp;

/// <summary>
/// The complete diagnostic payload produced when <see cref="Result.Parse{T,TErr}(string?,System.Func{ParseFailure,TErr},string)"/>
/// cannot parse its input.
/// </summary>
/// <remarks>
/// <para>
/// The payload mirrors the information the <see cref="RopError"/> overload stores in
/// <see cref="RopError.Code"/>, <see cref="RopError.Message"/> and <see cref="RopError.Details"/>, so callers with
/// their own error type can map a parse failure without losing diagnostics.
/// </para>
/// <para>
/// <see cref="Input"/> is the raw caller text and may be long or contain control characters. Use
/// <see cref="Details"/> when the text is written to a log or returned to a client: it is bounded, escaped and
/// quoted, and it renders a missing value as the literal <c>null</c> instead of an empty pair of quotes.
/// </para>
/// <para>
/// The parameterless <see langword="default"/> value is uninitialized. Reading <see cref="Code"/>,
/// <see cref="Message"/>, <see cref="Input"/>, <see cref="TypeName"/>, <see cref="Details"/> or calling
/// <see cref="ToString"/> throws <see cref="InvalidOperationException"/>.
/// </para>
/// <code><![CDATA[
/// var parsed = Result.Parse<decimal, string>("19.50", static failure => failure.Code)
///     .MapOk(static amount => amount.ToString("0.00", CultureInfo.InvariantCulture));
///
/// // Ok("19.50") when the text is a number,
/// // Err(E_PARSE) when it is not - the failure carries the diagnostics of the attempt.
/// ]]></code>
/// </remarks>
public readonly struct ParseFailure : IEquatable<ParseFailure>
{
    private const byte TagUninitialized = 0;
    private const byte TagInitialized = 1;

    private readonly byte _tag;
    private readonly string _code;
    private readonly string _message;
    private readonly string? _input;
    private readonly string _typeName;

    /// <summary>
    /// Initializes a parse failure payload.
    /// </summary>
    /// <param name="code">The stable, machine-readable error code. Must not be blank.</param>
    /// <param name="message">The human-readable diagnostic message. Must not be blank.</param>
    /// <param name="input">The raw input that failed to parse; <see langword="null"/> means the input was missing.</param>
    /// <param name="typeName">The target type name. Must not be blank.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="code"/>, <paramref name="message"/> or <paramref name="typeName"/> is empty or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="code"/>, <paramref name="message"/> or <paramref name="typeName"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var placed = Result.Err<decimal>(RopError.Create(failure.Code, failure.Message));
    ///
    /// // Err(E_AMOUNT_PARSE) is the stable code of the failure,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public ParseFailure(string code, string message, string? input, string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        _tag = TagInitialized;
        _code = code;
        _message = message;
        _input = input;
        _typeName = typeName;
    }

    /// <summary>Gets the stable, machine-readable error code (for example <c>E_PARSE</c>).</summary>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var code = failure.Code;
    ///
    /// // Err(E_AMOUNT_PARSE) is the stable key to branch on,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string Code
    {
        get
        {
            ThrowIfUninitialized();
            return _code;
        }
    }

    /// <summary>Gets the human-readable diagnostic message. Display only; branch on <see cref="Code"/>.</summary>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var message = failure.Message;
    ///
    /// // Err(E_AMOUNT_PARSE) explains what went wrong,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string Message
    {
        get
        {
            ThrowIfUninitialized();
            return _message;
        }
    }

    /// <summary>Gets the raw input that failed to parse, or <see langword="null"/> when the input was missing.</summary>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var input = failure.Input;
    ///
    /// // Err(E_AMOUNT_PARSE) keeps the raw caller text,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string? Input
    {
        get
        {
            ThrowIfUninitialized();
            return _input;
        }
    }

    /// <summary>Gets the name of the target type that could not be parsed.</summary>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var typeName = failure.TypeName;
    ///
    /// // Err(E_AMOUNT_PARSE) names the target type that failed,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string TypeName
    {
        get
        {
            ThrowIfUninitialized();
            return _typeName;
        }
    }

    /// <summary>
    /// Gets the bounded, escaped <c>input=&lt;quoted&gt;</c> diagnostic, mirroring <see cref="RopError.Details"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message, failure.Details);
    /// var details = failure.Details;
    ///
    /// // Err(E_AMOUNT_PARSE) renders as "input=\"19,50\"",
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string Details
    {
        get
        {
            ThrowIfUninitialized();
            return $"input={DiagnosticText.Quote(_input)}";
        }
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message, failure.Details);
    /// var text = failure.ToString();
    ///
    /// // Err(E_AMOUNT_PARSE) renders as "E_AMOUNT_PARSE: the amount is not a number (input=\"19,50\")",
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public override string ToString()
    {
        ThrowIfUninitialized();
        return $"{_code}: {_message} ({Details})";
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Two parse failures are equal when every diagnostic field matches. This is what lets a failure be used as a
    /// dictionary key or compared inside a test.
    /// </para>
    /// <code><![CDATA[
    /// var first = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var second = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var placed = Result.Err<decimal>(RopError.Create(first.Code, first.Message, first.Details));
    /// var same = first.Equals(second);
    ///
    /// // Err(E_AMOUNT_PARSE) compares equal when every field matches,
    /// // Ok(19.50) is the other track of the same parse result.
    /// ]]></code>
    /// </remarks>
    public bool Equals(ParseFailure other) =>
        _tag == other._tag
        && (_tag == TagUninitialized
            || (_code == other._code
                && _message == other._message
                && _input == other._input
                && _typeName == other._typeName));

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The boxed overload defers to the strongly typed comparison, so a failure stored in an
    /// <see cref="object"/> still compares by value.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var placed = Result.Err<decimal>(RopError.Create(failure.Code, failure.Message, failure.Details));
    /// var same = failure.Equals((object)failure);
    ///
    /// // Err(E_AMOUNT_PARSE) equals its own boxed copy,
    /// // Ok(19.50) is the other track of the same parse result.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is ParseFailure other && Equals(other);

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The hash code is derived from the diagnostic fields, so equal failures always share a hash code.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var placed = Result.Err<decimal>(RopError.Create(failure.Code, failure.Message, failure.Details));
    /// var bucket = failure.GetHashCode() % 16;
    ///
    /// // Err(E_AMOUNT_PARSE) hashes by its diagnostic fields,
    /// // Ok(19.50) is the other track of the same parse result.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() =>
        _tag == TagUninitialized
            ? HashCode.Combine(TagUninitialized)
            : HashCode.Combine(TagInitialized, _code, _message, _input, _typeName);

    /// <summary>Determines whether two parse failures are equal.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true"/> when every diagnostic field matches.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var first = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var second = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var placed = Result.Err<decimal>(RopError.Create(first.Code, first.Message, first.Details));
    /// var same = first == second;
    ///
    /// // Err(E_AMOUNT_PARSE) == Err(E_AMOUNT_PARSE) when every field matches,
    /// // Ok(19.50) is the other track of the same parse result.
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(ParseFailure left, ParseFailure right) => left.Equals(right);

    /// <summary>Determines whether two parse failures differ.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true"/> when they differ.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var first = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var second = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "20,00", "decimal");
    /// var placed = Result.Err<decimal>(RopError.Create(first.Code, first.Message, first.Details));
    /// var differs = first != second;
    ///
    /// // Err(E_AMOUNT_PARSE) != Err(E_AMOUNT_PARSE) when input differs,
    /// // Ok(19.50) is the other track of the same parse result.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(ParseFailure left, ParseFailure right) => !left.Equals(right);

    private void ThrowIfUninitialized()
    {
        if (_tag == TagUninitialized)
        {
            throw new InvalidOperationException(
                "The parse failure is an uninitialized default value; construct values with the ParseFailure constructor.");
        }
    }
}
