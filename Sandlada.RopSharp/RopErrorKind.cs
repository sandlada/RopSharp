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
/// </remarks>
public enum RopErrorKind
{
    /// <summary>A generic domain failure. This is the default.</summary>
    Failure = 0,

    /// <summary>Input or domain validation failed.</summary>
    Validation = 1,

    /// <summary>A requested resource does not exist.</summary>
    NotFound = 2,

    /// <summary>The operation conflicts with the current state.</summary>
    Conflict = 3,

    /// <summary>The caller is not authenticated.</summary>
    Unauthorized = 4,

    /// <summary>The caller is authenticated but not allowed to perform the operation.</summary>
    Forbidden = 5,

    /// <summary>An unexpected boundary failure was captured from an exception.</summary>
    Unexpected = 6,

    /// <summary>The operation did not complete within its deadline.</summary>
    Timeout = 7,

    /// <summary>A required dependency is temporarily unavailable, so the operation may be retried later.</summary>
    Unavailable = 8,

    /// <summary>The caller exceeded a quota or rate limit.</summary>
    RateLimited = 9,
}
