namespace Sandlada.RopSharp;

/// <summary>
/// Nullable-boundary helpers for reference-typed options.
/// </summary>
/// <remarks>
/// C# does not allow two overloads that differ only by generic constraints, so the reference and value-type
/// variants live under distinct names (<c>ToNullable</c> and <c>ToNullableValue</c>).
/// <code><![CDATA[
/// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
///     .MapSome(static shipping => shipping.City)
///     .ToNullable();
///
/// // "London" for Some(city),
/// // null for None - this is the only place the library produces null.
/// ]]></code>
/// </remarks>
public static class OptionNullableExtensions
{
    /// <summary>
    /// Converts a reference-typed option to a nullable reference, the only place the library produces null.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <returns>The contained value, or <see langword="null"/> when empty.</returns>
    /// <remarks>Cross back immediately with <see cref="Opt.FromNullable{T}(T)"/> at the consuming boundary.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSome(static shipping => shipping.City)
    ///     .ToNullable();
    ///
    /// // "London" for Some(city),
    /// // null for None - cross back with Opt.FromNullable right away.
    /// ]]></code>
    /// </remarks>
    public static T? ToNullable<T>(this Option<T> option)
        where T : class => option.TryGetValue(out var value) ? value : null;

    /// <summary>
    /// Maps a value through a binder that returns a nullable reference, treating null as <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">Maps the value to a possibly-null reference. Not evaluated for <c>None</c>.</param>
    /// <returns><c>Some</c> when the binder returns a non-null value; otherwise <c>None</c>.</returns>
    /// <remarks>
    /// This is the way into an API that only knows about nullable references: the null the binder returns is
    /// turned back into an empty option instead of leaking out.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .BindSomeNullable(static shipping => shipping.City);
    ///
    /// // Some("London") when the binder returns a city,
    /// // None when it returns null - the null never leaves the option.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> BindSomeNullable<T, TOut>(this Option<T> option, Func<T, TOut?> binder)
        where TOut : class
    {
        ArgumentNullException.ThrowIfNull(binder);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        var mapped = binder(value);
        return mapped is null ? Opt.None<TOut>() : Opt.Some(mapped);
    }
}
