namespace Sandlada.RopSharp;

/// <summary>
/// Classifies an expected failure so transport and presentation layers can map errors without parsing
/// <see cref="RopError.Code"/> strings.
/// </summary>
/// <remarks>
/// The kind is intentionally small and transport-neutral. Domain code should still branch on
/// <see cref="RopError.Code"/>; the kind is a coarse category for cross-cutting concerns such as HTTP status
/// mapping, logging levels and UI grouping.
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
}
