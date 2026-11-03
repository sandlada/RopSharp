using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// Argument guards that are safe for unconstrained generic parameters.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ArgumentNullException.ThrowIfNull(object?,string?)"/> takes <see cref="object"/>, so passing an
/// unconstrained generic argument boxes value types on every call. These combinators sit on hot paths, therefore
/// the null test is only reached for type arguments that can actually be null. The per-type decision is computed
/// once by the runtime and constant-folded into the calling code.
/// </para>
/// <para>
/// Guards are evaluated at method entry, before any track is selected, so a programming error is reported
/// deterministically instead of depending on the shape of the input value.
/// </para>
/// </remarks>
internal static class ArgumentGuards
{
    /// <summary>
    /// Determines whether <paramref name="value"/> is null without boxing a value type.
    /// </summary>
    /// <typeparam name="T">The argument type.</typeparam>
    /// <param name="value">The argument value.</param>
    /// <returns><see langword="true"/> when the value is null; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The <c>value is null</c> pattern on an unconstrained generic parameter compiles to a box followed by a
    /// null comparison. Optimized code folds the box away, but unoptimized code (a debug build, or tier-0 code
    /// before the runtime re-jits it) allocates 24 bytes per call for every value-type payload. The factories of
    /// the library sit on the hot path of every pipeline, so the check is written to avoid the box instead.
    /// </para>
    /// <para>
    /// The per-type decision is computed once by the runtime and constant-folded into the calling code. A
    /// <see cref="Nullable{T}"/> payload is compared without boxing as well, because boxing it would allocate
    /// for every non-null value.
    /// </para>
    /// </remarks>
    internal static bool IsNull<T>([NotNullWhen(false)] T value)
    {
        if (!Nullability<T>.CanBeNull)
        {
            return false;
        }

        if (Nullability<T>.IsNullableValueType)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }

        return value is null;
    }

    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> when <paramref name="value"/> is null.
    /// </summary>
    /// <typeparam name="T">The argument type.</typeparam>
    /// <param name="value">The argument value.</param>
    /// <param name="paramName">The parameter name reported by the exception.</param>
    internal static void ThrowIfNull<T>(T value, string paramName)
    {
        if (IsNull(value))
        {
            throw new ArgumentNullException(paramName);
        }
    }

    private static class Nullability<T>
    {
        internal static readonly bool CanBeNull = default(T) is null;

        internal static readonly bool IsNullableValueType = Nullable.GetUnderlyingType(typeof(T)) is not null;
    }
}
