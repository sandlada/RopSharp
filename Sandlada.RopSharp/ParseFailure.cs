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
    public string Code { get; }

    /// <summary>Gets the human-readable diagnostic message. Display only; branch on <see cref="Code"/>.</summary>
    public string Message { get; }

    /// <summary>Gets the raw input that failed to parse, or <see langword="null"/> when the input was missing.</summary>
    /// <remarks>
    /// This value is not escaped or truncated. Prefer <see cref="Details"/> for logs and transport payloads.
    /// </remarks>
    public string? Input { get; }

    /// <summary>Gets the name of the target type that could not be parsed.</summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the bounded, escaped <c>input=&lt;quoted&gt;</c> diagnostic, mirroring <see cref="RopError.Details"/>.
    /// </summary>
    public string Details => $"input={DiagnosticText.Quote(Input)}";

    /// <inheritdoc />
    public override string ToString() => $"{Code}: {Message} ({Details})";
}
