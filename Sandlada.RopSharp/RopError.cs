using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// A structured, immutable description of an expected failure.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Code"/> is a stable, machine-readable identifier intended for branching; <see cref="Message"/>
/// is human-readable and intended for display only. Use <see cref="Create(string,string,string?)"/> for domain
/// failures and <see cref="FromException(Exception,string?)"/> exclusively at boundaries where an exception must be
/// captured.
/// </para>
/// <para>
/// <see cref="Combine"/> collapses several errors into one <c>E_MULTIPLE</c> error whose <see cref="InnerErrors"/>
/// preserve the individual failures in the supplied order. An error that was not produced by <see cref="Combine"/>
/// exposes an empty <see cref="InnerErrors"/> list rather than <see langword="null"/>.
/// </para>
/// <para>
/// Equality compares <see cref="Kind"/>, <see cref="Code"/>, <see cref="Message"/> and <see cref="Details"/>.
/// <see cref="Cause"/> is deliberately excluded so that stack-trace differences do not make two logically equal
/// errors unequal, and <see cref="InnerErrors"/> is excluded for the same reason: the collapsed
/// code/message/details already describe the aggregate for branching and display. Compare
/// <see cref="InnerErrors"/> explicitly when the structure matters.
/// </para>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
[SuppressMessage(
    "ApiDesign",
    "RS0027:API with optional parameter(s) should have the most parameters amongst its public overloads",
    Justification = "The shipped 3-parameter overload is preserved; category-aware overloads add a fourth parameter rather than changing the existing signature.")]
public sealed class RopError : IEquatable<RopError>
{
    private RopError(
        string code,
        string message,
        string? details,
        Exception? cause,
        RopErrorKind kind,
        IReadOnlyList<RopError> innerErrors)
    {
        Code = code;
        Message = message;
        Details = details;
        Cause = cause;
        Kind = kind;
        InnerErrors = innerErrors;
    }

    /// <summary>
    /// Gets the stable, machine-readable error code (for example <c>E_USER_NOT_FOUND</c>).
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable message. Never use it for branching.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets optional machine-readable or diagnostic details, or <see langword="null"/> when absent.
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// Gets the captured exception when the error originated from <see cref="FromException(Exception,string?)"/>; otherwise <see langword="null"/>.
    /// </summary>
    public Exception? Cause { get; }

    /// <summary>
    /// Gets the coarse category of the failure. Use <see cref="Code"/> for precise domain branching.
    /// </summary>
    public RopErrorKind Kind { get; }

    /// <summary>
    /// Gets the errors this error was aggregated from, in the order supplied to <see cref="Combine"/>.
    /// </summary>
    /// <remarks>
    /// The list is never <see langword="null"/>; an error that was not produced by <see cref="Combine"/> exposes an
    /// empty list. Aggregation preserves the supplied elements as-is, including nested aggregates, so callers can walk
    /// the tree when they need the individual failures rather than the collapsed <see cref="Message"/> and
    /// <see cref="Details"/>.
    /// </remarks>
    public IReadOnlyList<RopError> InnerErrors { get; }

    /// <summary>
    /// Creates a structured error.
    /// </summary>
    /// <param name="code">The stable error code. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="message">The human-readable message. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="details">Optional details. When supplied it must not be an empty or whitespace string.</param>
    /// <returns>A new <see cref="RopError"/> with <see cref="RopErrorKind.Failure"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="code"/> or <paramref name="message"/> is empty or whitespace, or
    /// <paramref name="details"/> is an empty or whitespace string.
    /// </exception>
    public static RopError Create(string code, string message, string? details = null) =>
        Create(code, message, details, RopErrorKind.Failure);

    /// <summary>
    /// Creates a structured error with a category and no details.
    /// </summary>
    /// <param name="code">The stable error code. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="message">The human-readable message. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="kind">The coarse failure category.</param>
    /// <returns>A new <see cref="RopError"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="code"/> or <paramref name="message"/> is empty or whitespace.</exception>
    public static RopError Create(string code, string message, RopErrorKind kind) =>
        Create(code, message, details: null, kind);

    /// <summary>
    /// Creates a structured error with details and a category.
    /// </summary>
    /// <param name="code">The stable error code. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="message">The human-readable message. Must not be <see langword="null"/>, empty or whitespace.</param>
    /// <param name="details">Optional details. When supplied it must not be an empty or whitespace string.</param>
    /// <param name="kind">The coarse failure category.</param>
    /// <returns>A new <see cref="RopError"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="code"/> or <paramref name="message"/> is empty or whitespace, or
    /// <paramref name="details"/> is an empty or whitespace string.
    /// </exception>
    public static RopError Create(string code, string message, string? details, RopErrorKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (details is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(details);
        }

        return new RopError(code, message, details, cause: null, kind, Array.Empty<RopError>());
    }

    /// <summary>
    /// Creates an error from an exception. Intended for boundary code only (see <c>Rop.Try</c>).
    /// </summary>
    /// <param name="exception">The captured exception.</param>
    /// <param name="code">The error code; defaults to <c>E_EXN</c>.</param>
    /// <returns>A new <see cref="RopError"/> whose <see cref="Cause"/> is <paramref name="exception"/> and whose
    /// <see cref="Kind"/> is <see cref="RopErrorKind.Unexpected"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    public static RopError FromException(Exception exception, string? code = null) =>
        FromException(exception, code, RopErrorKind.Unexpected);

    /// <summary>
    /// Creates an error from an exception with a category and the default code <c>E_EXN</c>.
    /// </summary>
    /// <param name="exception">The captured exception.</param>
    /// <param name="kind">The coarse failure category.</param>
    /// <returns>A new <see cref="RopError"/> whose <see cref="Cause"/> is <paramref name="exception"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static RopError FromException(Exception exception, RopErrorKind kind) =>
        FromException(exception, code: null, kind);

    /// <summary>
    /// Creates an error from an exception with a code and category.
    /// </summary>
    /// <param name="exception">The captured exception.</param>
    /// <param name="code">The error code; defaults to <c>E_EXN</c>.</param>
    /// <param name="kind">The coarse failure category.</param>
    /// <returns>A new <see cref="RopError"/> whose <see cref="Cause"/> is <paramref name="exception"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    public static RopError FromException(Exception exception, string? code, RopErrorKind kind)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var resolvedCode = code ?? "E_EXN";
        ArgumentException.ThrowIfNullOrWhiteSpace(resolvedCode);
        var message = string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message;
        return new RopError(resolvedCode, message, details: null, cause: exception, kind, Array.Empty<RopError>());
    }

    /// <summary>
    /// Returns a new error with the specified code, preserving message, details and cause.
    /// </summary>
    /// <param name="code">The replacement code.</param>
    /// <returns>A new <see cref="RopError"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    public RopError WithCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new RopError(code, Message, Details, Cause, Kind, InnerErrors);
    }

    /// <summary>
    /// Returns a new error with the specified message, preserving code, details and cause.
    /// </summary>
    /// <param name="message">The replacement message.</param>
    /// <returns>A new <see cref="RopError"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="message"/> is empty or whitespace.</exception>
    public RopError WithMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new RopError(Code, message, Details, Cause, Kind, InnerErrors);
    }

    /// <summary>
    /// Returns a new error with the specified details, preserving code, message and cause.
    /// </summary>
    /// <param name="details">The replacement details, or <see langword="null"/> to remove them.</param>
    /// <returns>A new <see cref="RopError"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="details"/> is an empty or whitespace string.</exception>
    public RopError WithDetails(string? details)
    {
        if (details is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(details);
        }

        return new RopError(Code, Message, details, Cause, Kind, InnerErrors);
    }

    /// <summary>
    /// Returns a new error with the specified category, preserving code, message, details and cause.
    /// </summary>
    /// <param name="kind">The replacement category.</param>
    /// <returns>A new <see cref="RopError"/> instance.</returns>
    public RopError WithKind(RopErrorKind kind) => new(Code, Message, Details, Cause, kind, InnerErrors);

    /// <summary>
    /// Combines a non-empty list of errors into a single error with code <c>E_MULTIPLE</c>.
    /// </summary>
    /// <param name="errors">The errors to combine. Must contain at least one element.</param>
    /// <returns>
    /// A <see cref="RopError"/> whose message joins the distinct messages, whose details list the distinct codes and
    /// whose <see cref="InnerErrors"/> preserve every supplied error in order. The returned error never carries a
    /// <see cref="Cause"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="errors"/> is <see langword="null"/>, or any element is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// The aggregate collapses messages and codes for display and branching, but no information is lost: the original
    /// errors remain available through <see cref="InnerErrors"/>. The supplied elements are preserved as-is, so a
    /// nested aggregate keeps its own <see cref="InnerErrors"/> instead of being flattened.
    /// </para>
    /// <para>
    /// A single-element call is an identity operation: it returns that error unchanged (including its
    /// <see cref="InnerErrors"/>), which is why the returned instance may not carry the <c>E_MULTIPLE</c> code.
    /// </para>
    /// <para>
    /// Message and code deduplication uses ordinal comparison and runs in linear time in the number of errors, so a
    /// large validation batch cannot degrade into a quadratic scan.
    /// </para>
    /// </remarks>
    public static RopError Combine(IReadOnlyList<RopError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }

        // Validate the whole list before the single-element shortcut so a null element is reported
        // deterministically instead of leaking out as a null error value.
        foreach (var error in errors)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(errors));
        }

        if (errors.Count == 1)
        {
            return errors[0];
        }

        var messages = new List<string>(errors.Count);
        var codes = new List<string>(errors.Count);
        var distinctMessages = new HashSet<string>(errors.Count, StringComparer.Ordinal);
        var distinctCodes = new HashSet<string>(errors.Count, StringComparer.Ordinal);
        var innerErrors = new RopError[errors.Count];
        var kind = errors[0].Kind;
        for (var index = 0; index < errors.Count; index++)
        {
            var error = errors[index];
            innerErrors[index] = error;
            if (distinctMessages.Add(error.Message))
            {
                messages.Add(error.Message);
            }

            if (distinctCodes.Add(error.Code))
            {
                codes.Add(error.Code);
            }

            if (error.Kind != kind)
            {
                kind = RopErrorKind.Failure;
            }
        }

        return new RopError(
            "E_MULTIPLE",
            string.Join("; ", messages),
            string.Join(", ", codes),
            cause: null,
            kind,
            Array.AsReadOnly(innerErrors));
    }

    /// <inheritdoc />
    public bool Equals(RopError? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is not null
            && Kind == other.Kind
            && string.Equals(Code, other.Code, StringComparison.Ordinal)
            && string.Equals(Message, other.Message, StringComparison.Ordinal)
            && string.Equals(Details, other.Details, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as RopError);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(
        (int)Kind,
        StringComparer.Ordinal.GetHashCode(Code),
        StringComparer.Ordinal.GetHashCode(Message),
        Details is null ? 0 : StringComparer.Ordinal.GetHashCode(Details));

    /// <inheritdoc />
    public override string ToString() =>
        Details is null ? $"{Code}: {Message}" : $"{Code}: {Message} ({Details})";

    /// <summary>Determines whether two errors are equal (see <see cref="Equals(RopError)"/>).</summary>
    /// <param name="left">The left error.</param>
    /// <param name="right">The right error.</param>
    /// <returns><see langword="true"/> when both are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(RopError? left, RopError? right) => Equals(left, right);

    /// <summary>Determines whether two errors are different.</summary>
    /// <param name="left">The left error.</param>
    /// <param name="right">The right error.</param>
    /// <returns><see langword="true"/> when they differ; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(RopError? left, RopError? right) => !Equals(left, right);
}
