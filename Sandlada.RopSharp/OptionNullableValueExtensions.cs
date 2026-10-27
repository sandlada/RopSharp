namespace Sandlada.RopSharp;

/// <summary>
/// Nullable-boundary helpers for value-typed options.
/// </summary>
/// <remarks>
/// C# does not allow two overloads that differ only by generic constraints, so the reference and value-type
/// variants live under distinct names (<c>ToNullable</c> and <c>ToNullableValue</c>).
/// </remarks>
public static class OptionNullableValueExtensions
{
    /// <summary>
    /// Converts a value-typed option to a <see cref="Nullable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <returns>The contained value, or <see langword="null"/> when empty.</returns>
    public static T? ToNullableValue<T>(this Option<T> option)
        where T : struct => option.TryGetValue(out var value) ? value : null;

    /// <summary>
    /// Maps a value through a binder that returns a nullable value type, treating "no value" as <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">Maps the value to a possibly-empty value type. Not evaluated for <c>None</c>.</param>
    /// <returns><c>Some</c> when the binder returns a value; otherwise <c>None</c>.</returns>
    public static Option<TOut> BindSomeNullableValue<T, TOut>(this Option<T> option, Func<T, TOut?> binder)
        where TOut : struct
    {
        ArgumentNullException.ThrowIfNull(binder);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        var mapped = binder(value);
        return mapped is null ? Opt.None<TOut>() : Opt.Some(mapped.GetValueOrDefault());
    }
}
