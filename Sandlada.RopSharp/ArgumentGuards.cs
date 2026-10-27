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
    /// Throws <see cref="ArgumentNullException"/> when <paramref name="value"/> is null.
    /// </summary>
    /// <typeparam name="T">The argument type.</typeparam>
    /// <param name="value">The argument value.</param>
    /// <param name="paramName">The parameter name reported by the exception.</param>
    internal static void ThrowIfNull<T>(T value, string paramName)
    {
        if (CanBeNull<T>.Value && value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    private static class CanBeNull<T>
    {
        internal static readonly bool Value = default(T) is null;
    }
}
