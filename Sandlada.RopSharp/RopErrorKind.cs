namespace Sandlada.RopSharp;

/// <summary>
/// Classifies an expected failure so transport and presentation layers can map errors without parsing
/// <see cref="RopError.Code"/> strings.
/// </summary>
/// <remarks>
/// <para>
/// The kind is intentionally small and transport-neutral. Domain code should still branch on
/// <see cref="RopError.Code"/>; the kind is a coarse category for cross-cutting concerns such as HTTP status
/// mapping, logging levels and UI grouping.
/// </para>
/// <para>
/// Values are append-only for binary compatibility. Consumers that switch over the enum without a fallback arm
/// should add one, because new categories are added when they describe a distinct, common failure mode.
/// </para>
/// <code><![CDATA[/// var error = RopError.Create("E_AMOUNT_RANGE", "amount must be positive", RopErrorKind.Validation);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_AMOUNT_RANGE", "amount must be positive", RopErrorKind.Validation) is categorized as RopErrorKind.Validation,
/// // Ok(payment) is the other track of the same result.]]></code>
/// </remarks>
public enum RopErrorKind
{
    /// <summary>A generic domain failure. This is the default.</summary>
    /// <remarks>
    /// <para>
    /// The fallback category. It is used by every error that does not belong to a more specific bucket, so a
    /// transport layer that only knows this category still has a defined behaviour.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist", RopErrorKind.Failure);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_ORDER_NOT_FOUND", "order 42 does not exist", RopErrorKind.Failure) is categorized as RopErrorKind.Failure,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Failure = 0,

    /// <summary>Input or domain validation failed.</summary>
    /// <remarks>
    /// <para>
    /// The caller sent something the domain refuses. A validation failure is the caller's to fix, so a
    /// transport layer usually answers with a client error.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_AMOUNT_RANGE", "amount must be positive", RopErrorKind.Validation);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_AMOUNT_RANGE", "amount must be positive", RopErrorKind.Validation) is categorized as RopErrorKind.Validation,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Validation = 1,

    /// <summary>A requested resource does not exist.</summary>
    /// <remarks>
    /// <para>
    /// The resource was addressed but is not there. This is the category a lookup failure belongs to.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_NOT_FOUND", "order 42 does not exist", RopErrorKind.NotFound);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_ORDER_NOT_FOUND", "order 42 does not exist", RopErrorKind.NotFound) is categorized as RopErrorKind.NotFound,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    NotFound = 2,

    /// <summary>The operation conflicts with the current state.</summary>
    /// <remarks>
    /// <para>
    /// The resource exists but is not in a state that allows the operation, for example an order that has
    /// already shipped.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_ORDER_STATE", "the order is already shipped", RopErrorKind.Conflict);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_ORDER_STATE", "the order is already shipped", RopErrorKind.Conflict) is categorized as RopErrorKind.Conflict,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Conflict = 3,

    /// <summary>The caller is not authenticated.</summary>
    /// <remarks>
    /// <para>
    /// No usable identity was presented. The difference to <see cref="Forbidden"/> is that the caller may still
    /// succeed once it authenticates.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_AUTH", "the caller is not signed in", RopErrorKind.Unauthorized);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_AUTH", "the caller is not signed in", RopErrorKind.Unauthorized) is categorized as RopErrorKind.Unauthorized,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Unauthorized = 4,

    /// <summary>The caller is authenticated but not allowed to perform the operation.</summary>
    /// <remarks>
    /// <para>
    /// The identity is known but lacks the permission. Authentication would not help here.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_AUTH", "the caller may not refund this order", RopErrorKind.Forbidden);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_AUTH", "the caller may not refund this order", RopErrorKind.Forbidden) is categorized as RopErrorKind.Forbidden,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Forbidden = 5,

    /// <summary>An unexpected boundary failure was captured from an exception.</summary>
    /// <remarks>
    /// <para>
    /// The default category of every error produced by <see cref="RopError.FromException(Exception,string?)"/>.
    /// It signals that the failure was not anticipated by the domain.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_PROVIDER", "the provider failed unexpectedly", RopErrorKind.Unexpected);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_PROVIDER", "the provider failed unexpectedly", RopErrorKind.Unexpected) is categorized as RopErrorKind.Unexpected,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Unexpected = 6,

    /// <summary>The operation did not complete within its deadline.</summary>
    /// <remarks>
    /// <para>
    /// The operation ran out of time. Unlike <see cref="Unavailable"/> the operation may not have had any effect
    /// at all, so a retry has to be idempotent.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_CARRIER_TIMEOUT", "the carrier did not answer", RopErrorKind.Timeout);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_CARRIER_TIMEOUT", "the carrier did not answer", RopErrorKind.Timeout) is categorized as RopErrorKind.Timeout,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Timeout = 7,

    /// <summary>A required dependency is temporarily unavailable, so the operation may be retried later.</summary>
    /// <remarks>
    /// <para>
    /// A dependency is down but the request itself is fine. Retrying later is a sensible reaction, which is what
    /// separates this category from <see cref="Timeout"/>.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_CARRIER_DOWN", "the carrier is unavailable", RopErrorKind.Unavailable);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_CARRIER_DOWN", "the carrier is unavailable", RopErrorKind.Unavailable) is categorized as RopErrorKind.Unavailable,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    Unavailable = 8,

    /// <summary>The caller exceeded a quota or rate limit.</summary>
    /// <remarks>
    /// <para>
    /// The caller sent too much work. A transport layer usually answers with a rate-limit status and a retry
    /// hint.
    /// </para>
    /// <code><![CDATA[/// var error = RopError.Create("E_TOO_MANY_REQUESTS", "too many order updates", RopErrorKind.RateLimited);
/// var placed = Rop.Err<Payment>(error);
///
/// // Err("E_TOO_MANY_REQUESTS", "too many order updates", RopErrorKind.RateLimited) is categorized as RopErrorKind.RateLimited,
/// // Ok(payment) is the other track of the same result.]]></code>
    /// </remarks>
    RateLimited = 9,
}
