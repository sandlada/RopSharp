namespace Sandlada.RopSharp;

/// <summary>
/// The complete diagnostic payload produced when <see cref="Rop.Parse{T,TErr}(string?,System.Func{ParseFailure,TErr},string)"/>
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
/// The parameterless <see langword="default"/> value is undefined: <see cref="Code"/>, <see cref="Message"/> and
/// <see cref="TypeName"/> are <see langword="null"/>. Instances are created by the library.
/// </para>
/// <code><![CDATA[
/// var parsed = Rop.Parse<decimal, string>("19.50", static failure => failure.Code)
///     .MapOk(static amount => amount.ToString("0.00", CultureInfo.InvariantCulture));
///
/// // Ok("19.50") when the text is a number,
/// // Err(E_PARSE) when it is not - the failure carries the diagnostics of the attempt.
/// ]]></code>
/// </remarks>
public readonly record struct ParseFailure
{
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
    /// <para>
    /// The library creates these payloads itself. The constructor is public so that a caller with a custom error
    /// type can build one for its own parsing boundary and keep the same shape.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var placed = Rop.Err<decimal>(RopError.Create(failure.Code, failure.Message));
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

        Code = code;
        Message = message;
        Input = input;
        TypeName = typeName;
    }

    /// <summary>Gets the stable, machine-readable error code (for example <c>E_PARSE</c>).</summary>
    /// <remarks>
    /// <para>
    /// The code is the part of the payload that may be branched on. It mirrors
    /// <see cref="RopError.Code"/> so that a caller with its own error type can map a parse failure without
    /// losing the stable identifier.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var code = failure.Code;
    ///
    /// // Err(E_AMOUNT_PARSE) is the stable key to branch on,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string Code { get; }

    /// <summary>Gets the human-readable diagnostic message. Display only; branch on <see cref="Code"/>.</summary>
    /// <remarks>
    /// <para>
    /// The message explains what went wrong with the input. It mirrors <see cref="RopError.Message"/> and is meant
    /// for a human reading a log.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var message = failure.Message;
    ///
    /// // Err(E_AMOUNT_PARSE) explains what went wrong,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string Message { get; }

    /// <summary>Gets the raw input that failed to parse, or <see langword="null"/> when the input was missing.</summary>
    /// <remarks>
    /// <para>
    /// This value is not escaped or truncated. Prefer <see cref="Details"/> for logs and transport payloads.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var input = failure.Input;
    ///
    /// // Err(E_AMOUNT_PARSE) keeps the raw caller text,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string? Input { get; }

    /// <summary>Gets the name of the target type that could not be parsed.</summary>
    /// <remarks>
    /// <para>
    /// The type name tells the caller which conversion failed, which matters when one endpoint accepts several
    /// shapes.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message);
    /// var typeName = failure.TypeName;
    ///
    /// // Err(E_AMOUNT_PARSE) names the target type that failed,
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string TypeName { get; }

    /// <summary>
    /// Gets the bounded, escaped <c>input=&lt;quoted&gt;</c> diagnostic, mirroring <see cref="RopError.Details"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unlike <see cref="Input"/>, this value is safe to log or to return to a client: it is quoted, escaped and
    /// truncated, and it renders a missing input as the literal <c>null</c>.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message, failure.Details);
    /// var details = failure.Details;
    ///
    /// // Err(E_AMOUNT_PARSE) renders as "input=\"19,50\"",
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public string Details => $"input={DiagnosticText.Quote(Input)}";

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The text is a debugging aid, not a wire format. It combines the code, the message and the bounded
    /// details into a single line.
    /// </para>
    /// <code><![CDATA[
    /// var failure = new ParseFailure("E_AMOUNT_PARSE", "the amount is not a number", "19,50", "decimal");
    /// var error = RopError.Create(failure.Code, failure.Message, failure.Details);
    /// var text = failure.ToString();
    ///
    /// // Err(E_AMOUNT_PARSE) renders as "E_AMOUNT_PARSE: the amount is not a number (input=\"19,50\")",
    /// // Ok(19.50) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public override string ToString() => $"{Code}: {Message} ({Details})";
}
