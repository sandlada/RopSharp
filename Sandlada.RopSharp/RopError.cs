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
/// <para>
/// An error is the payload of the failure track. It travels through a pipeline untouched until a combinator
/// reshapes it, and it is finally read by a terminal that decides what the caller sees.
/// </para>
/// <code><![CDATA[
/// var placed = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
///     .MapErr(static error => error.WithCode("E_PAYMENT_DECLINED"));
///
/// // Err(E_PAYMENT_DECLINED) when the provider refused the order,
/// // Ok(payment) when it succeeded - the code is the stable part to branch on.
/// ]]></code>
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
    /// <remarks>
    /// <para>
    /// The code is the only part of an error that may be branched on. It is stable across releases, which makes
    /// it the key a transport layer maps to a status code.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist").WithCode("E_ORDER_404");
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_ORDER_404) is the stable key to branch on,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable message. Never use it for branching.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The message is written for a human reading a log or a notification. It may be reworded at any time, so
    /// only <see cref="Code"/> may be branched on.
    /// </para>
    /// <code><![CDATA[/// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE");
/// var message = charged.Match(
///     ok: static payment => payment.Id,
///     err: static error => error.Message);
///
/// // Err(E_CHARGE) yields the provider message,
/// // Ok(payment) yields the receipt id - the message is display only.]]></code>
    /// </remarks>
    public string Message { get; }

    /// <summary>
    /// Gets optional machine-readable or diagnostic details, or <see langword="null"/> when absent.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The details carry the payload that does not fit into the message, such as the offending identifier. They
    /// are part of the error's equality, so two errors that differ only in their details are different errors.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist", "id=42")
///     .WithDetails("id=42,archived=true");
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_ORDER_NOT_FOUND) carries the diagnostic payload,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public string? Details { get; }

    /// <summary>
    /// Gets the captured exception when the error originated from <see cref="FromException(Exception,string?)"/>; otherwise <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The cause exists only at a boundary. It is excluded from equality on purpose, so two errors captured from
    /// different stack traces still compare equal.
    /// </para>
    /// <code><![CDATA[/// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE");
/// Exception? cause = null;
/// if (charged.TryGetErr(out var error))
/// {
///     cause = error.Cause;
/// }
///
/// // Err(E_CHARGE) carries the captured exception as its Cause,
/// // Ok(payment) has no Cause at all - only boundaries produce one.]]></code>
    /// </remarks>
    public Exception? Cause { get; }

    /// <summary>
    /// Gets the coarse category of the failure. Use <see cref="Code"/> for precise domain branching.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The kind is a coarse bucket that transport and presentation layers map without knowing the domain. It is
    /// deliberately small and append-only.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_AMOUNT_RANGE", "amount must be positive", RopErrorKind.Validation);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_AMOUNT_RANGE) is categorized as RopErrorKind.Validation,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public RopErrorKind Kind { get; }

    /// <summary>
    /// Gets the errors this error was aggregated from, in the order supplied to <see cref="Combine"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The list is never <see langword="null"/>; an error that was not produced by <see cref="Combine"/> exposes an
    /// empty list. Aggregation preserves the supplied elements as-is, including nested aggregates, so callers can walk
    /// the tree when they need the individual failures rather than the collapsed <see cref="Message"/> and
    /// <see cref="Details"/>.
    /// </para>
    /// <code><![CDATA[/// var aggregate = RopError.Combine(
/// [
///     RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist"),
///     RopError.Create("E_STOCK_MISSING", "SKU-1 is out of stock"),
/// ]);
/// var leaves = aggregate.InnerErrors;
///
/// // Err(E_MULTIPLE) keeps both leaves in InnerErrors,
/// // Err(E_ORDER_NOT_FOUND) is one of them - a plain error has an empty list.]]></code>
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
    /// <remarks>
    /// <para>
    /// This is the default factory for a domain failure: it produces an error of kind
    /// <see cref="RopErrorKind.Failure"/> with no cause and no inner errors.
    /// </para>
    /// <code><![CDATA[/// var placed = Rop.Err<Payment>(RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist", "id=42"));
///
/// // Err(E_ORDER_NOT_FOUND) carries the code, the message and the details,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// Use this overload when a transport layer has to react to the category of the failure. The details stay
    /// empty.
    /// </para>
    /// <code><![CDATA[/// var placed = Rop.Err<Payment>(
///     RopError.Create("E_AMOUNT_RANGE", "amount must be positive", RopErrorKind.Validation));
///
/// // Err(E_AMOUNT_RANGE) is a validation failure,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// This is the most explicit factory: it sets the code, the message, the diagnostic payload and the category
    /// in one call.
    /// </para>
    /// <code><![CDATA[/// var placed = Rop.Err<Payment>(
///     RopError.Create("E_AMOUNT_RANGE", "amount must be positive", "amount=0", RopErrorKind.Validation));
///
/// // Err(E_AMOUNT_RANGE) carries the details as well,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// This is the only place where an exception becomes a domain value. The exception is kept as
    /// <see cref="Cause"/> so the stack trace is not lost, but it is excluded from equality.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.FromException(new InvalidOperationException("provider unavailable"), "E_PROVIDER");
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_PROVIDER) captures the exception as its Cause,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public static RopError FromException(Exception exception, string? code = null) =>
        FromException(exception, code, RopErrorKind.Unexpected);

    /// <summary>
    /// Creates an error from an exception with a category and the default code <c>E_EXN</c>.
    /// </summary>
    /// <param name="exception">The captured exception.</param>
    /// <param name="kind">The coarse failure category.</param>
    /// <returns>A new <see cref="RopError"/> whose <see cref="Cause"/> is <paramref name="exception"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// Use this overload when the category carries the meaning and the code is not needed, for example a timeout
    /// that a transport layer maps to <c>504</c>.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.FromException(new TimeoutException("carrier did not answer"), RopErrorKind.Timeout);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_EXN) is categorized as RopErrorKind.Timeout,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// This overload sets both the stable code and the coarse category of a captured exception, which is what a
    /// boundary that knows what went wrong needs.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.FromException(
///     new TimeoutException("carrier did not answer"),
///     "E_CARRIER_TIMEOUT",
///     RopErrorKind.Timeout);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_CARRIER_TIMEOUT) captures the exception as its Cause,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The error is immutable, so retagging produces a new value and leaves the original untouched. This is how
    /// a generic boundary error is turned into a domain error without losing the captured cause.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist").WithCode("E_ORDER_404");
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_ORDER_404) replaces the original code,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The message can be reworded for the audience that will read it, without touching the code other systems
    /// branch on.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist")
///     .WithMessage("the order was archived");
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_ORDER_NOT_FOUND) now explains what happened,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The details are replaced, not appended. Passing <see langword="null"/> removes them again.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist")
///     .WithDetails("id=42,archived=true");
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_ORDER_NOT_FOUND) carries the new diagnostic payload,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The category is corrected at the point where the domain knows better than the boundary that produced the
    /// error.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist")
///     .WithKind(RopErrorKind.NotFound);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err(E_ORDER_NOT_FOUND) is categorized as RopErrorKind.NotFound,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
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
    /// <code><![CDATA[/// var aggregate = RopError.Combine(
/// [
///     RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist"),
///     RopError.Create("E_STOCK_MISSING", "SKU-1 is out of stock"),
/// ]);
/// var placed = Rop.Err<Payment>(aggregate);
///
/// // Err(E_MULTIPLE) collapses the individual failures for display,
/// // Ok(payment) is the other track of the same result.]]></code>
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
    /// <remarks>
    /// <para>
    /// Two errors are equal when their kind, code, message and details match. The cause and the inner errors are
    /// deliberately ignored, so an error captured twice from different stack traces still compares equal.
    /// </para>
    /// <code><![CDATA[/// var first = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist");
/// var second = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist");
/// var placed = Rop.Err<Payment>(first);
/// var same = first.Equals(second);
///
/// // Err(E_ORDER_NOT_FOUND) equals Err(E_ORDER_NOT_FOUND),
/// // Ok(payment) is irrelevant - only kind, code, message and details are compared.]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The boxed overload defers to the strongly typed comparison, so an error stored in an
    /// <see cref="object"/> still compares by value.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist");
/// var placed = Rop.Err<Payment>(error);
/// var same = error.Equals((object)error);
///
/// // Err(E_ORDER_NOT_FOUND) equals its own boxed copy,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => Equals(obj as RopError);

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The hash code is derived from the same four fields that equality compares, so equal errors always share a
    /// hash code.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist");
/// var placed = Rop.Err<Payment>(error);
/// var bucket = error.GetHashCode() % 16;
///
/// // Err(E_ORDER_NOT_FOUND) hashes by kind, code, message and details,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(
        (int)Kind,
        StringComparer.Ordinal.GetHashCode(Code),
        StringComparer.Ordinal.GetHashCode(Message),
        Details is null ? 0 : StringComparer.Ordinal.GetHashCode(Details));

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The text is a debugging aid, not a wire format. Branch on <see cref="Code"/> and read the payload through
    /// the combinators instead of parsing this string.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist", "id=42");
/// var placed = Rop.Err<Payment>(error);
/// var text = error.ToString();
///
/// // Err(E_ORDER_NOT_FOUND) renders as "E_ORDER_NOT_FOUND: order 42 does not exist (id=42)",
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public override string ToString() =>
        Details is null ? $"{Code}: {Message}" : $"{Code}: {Message} ({Details})";

    /// <summary>Determines whether two errors are equal (see <see cref="Equals(RopError)"/>).</summary>
    /// <param name="left">The left error.</param>
    /// <param name="right">The right error.</param>
    /// <returns><see langword="true"/> when both are equal; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of <see cref="Equals(RopError)"/> and is what a test assertion or a
    /// cache lookup naturally reaches for.
    /// </para>
    /// <code><![CDATA[/// var first = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist");
/// var second = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist").WithCode("E_ORDER_NOT_FOUND");
/// var placed = Rop.Err<Payment>(first);
/// var same = first == second;
///
/// // Err(E_ORDER_NOT_FOUND) == Err(E_ORDER_NOT_FOUND),
/// // Ok(payment) is irrelevant - only kind, code, message and details are compared.]]></code>
    /// </remarks>
    public static bool operator ==(RopError? left, RopError? right) => Equals(left, right);

    /// <summary>Determines whether two errors are different.</summary>
    /// <param name="left">The left error.</param>
    /// <param name="right">The right error.</param>
    /// <returns><see langword="true"/> when they differ; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of the negated equality, and is how an error is compared against a
    /// known expected value.
    /// </para>
    /// <code><![CDATA[/// var first = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist");
/// var second = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist").WithCode("E_STOCK_MISSING");
/// var placed = Rop.Err<Payment>(first);
/// var differs = first != second;
///
/// // Err(E_ORDER_NOT_FOUND) != Err(E_STOCK_MISSING) because the codes differ,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    public static bool operator !=(RopError? left, RopError? right) => !Equals(left, right);
}
