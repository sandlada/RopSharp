namespace Sandlada.RopSharp;

/// <summary>
/// Nullable-boundary helpers for value-typed options.
/// </summary>
/// <remarks>
/// C# does not allow two overloads that differ only by generic constraints, so the reference and value-type
/// variants live under distinct names (<c>ToNullable</c> and <c>ToNullableValue</c>).
/// <code><![CDATA[
/// var count = Opt.FromNullableValue<int>(OrderFixtures.Sample.ItemCount)
///     .ToNullableValue();
///
/// // 3 for Some(count),
/// // null for None - this is the only place the library produces an empty value.
/// ]]></code>
/// </remarks>
public static class OptionNullableValueExtensions
{
    /// <summary>
    /// Converts a value-typed option to a <see cref="Nullable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <returns>The contained value, or <see langword="null"/> when empty.</returns>
    /// <remarks>Cross back immediately with <see cref="Opt.FromNullableValue{T}(T?)"/> at the consuming boundary.
    /// <code><![CDATA[
    /// int? quantity = OrderFixtures.Sample.ItemCount;
    /// var count = Opt.FromNullableValue(quantity)
    ///     .ToNullableValue();
    ///
    /// // 3 for Some(count),
    /// // null for None - cross back with Opt.FromNullableValue right away.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// This is the way into an API that only knows about nullable value types: the empty value the binder
    /// returns is turned back into an empty option instead of leaking out.
    /// <code><![CDATA[
    /// int? quantity = OrderFixtures.Sample.ItemCount;
    /// var count = Opt.FromNullableValue(quantity)
    ///     .BindSomeNullableValue(static value => (int?)(value + 1));
    ///
    /// // Some(4) when the binder returns a value,
    /// // None when it returns no value - the empty value never leaves the option.
    /// ]]></code>
    /// </remarks>
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
