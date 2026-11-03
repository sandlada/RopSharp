namespace Sandlada.RopSharp;

/// <summary>
/// LINQ query-syntax support for <see cref="Option{T}"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="Result{TOk,TErr}"/>, options support <c>where</c>: filtering only needs to produce <c>None</c>,
/// so no error value is required.
/// <code><![CDATA[
/// var city = Option.FromNullable(OrderFixtures.Sample.Customer.Address)
///     .Select(static shipping => shipping.City);
///
/// // Some("London") on the present track,
/// // None on the empty track - Select is the query-syntax form of MapSome.
/// ]]></code>
/// </remarks>
public static class OptionLinqExtensions
{
    /// <summary>
    /// Projects a present value. Query-syntax equivalent of <c>select</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="selector">The projection.</param>
    /// <returns>The projected value, or <c>None</c>.</returns>
    /// <remarks>
    /// <c>Select</c> is the query-syntax spelling of <see cref="OptionExtensions.MapSome{T,TOut}"/>. It exists so
    /// that an option can be used inside a LINQ query expression.
    /// <code><![CDATA[
    /// var city = Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .Select(static shipping => shipping.City);
    ///
    /// // Some("London") on the present track,
    /// // None on the empty track - Select is the query-syntax form of MapSome.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> Select<T, TOut>(this Option<T> option, Func<T, TOut> selector) =>
        option.MapSome(selector);

    /// <summary>
    /// Binds a present value to another option. Query-syntax equivalent of a second <c>from</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The bound value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">The binding function.</param>
    /// <returns>The bound option, or <c>None</c>.</returns>
    /// <remarks>
    /// <c>SelectMany</c> is the query-syntax spelling of <see cref="OptionExtensions.BindSome{T,TOut}"/>: it
    /// expands the present track into another option.
    /// <code><![CDATA[
    /// var city = Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .SelectMany(static shipping => Option.Some(shipping.City));
    ///
    /// // Some("London") when the address is present and the binding succeeded,
    /// // None when it is missing - the binder never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> SelectMany<T, TOut>(
        this Option<T> option,
        Func<T, Option<TOut>> binder) => option.BindSome(binder);

    /// <summary>
    /// Binds a present value and projects the pair. Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TCol">The intermediate value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="collectionSelector">The intermediate binding.</param>
    /// <param name="resultSelector">Projects both values.</param>
    /// <returns>The projected value, or <c>None</c>.</returns>
    /// <remarks>
    /// This overload carries both values into the projection, which is what a query with two <c>from</c> clauses
    /// needs.
    /// <code><![CDATA[
    /// var label = Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .SelectMany(
    ///         static shipping => Option.Some(shipping.City),
    ///         static (shipping, city) => $"{shipping.Country}/{city}");
    ///
    /// // Some("GB/London") when both steps succeeded,
    /// // None when the address is missing - the first empty track wins.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> SelectMany<T, TCol, TOut>(
        this Option<T> option,
        Func<T, Option<TCol>> collectionSelector,
        Func<T, TCol, TOut> resultSelector) =>
        option.BindSome(first =>
            collectionSelector(first).MapSome(second => resultSelector(first, second)));

    /// <summary>
    /// Filters a present value. Query-syntax equivalent of <c>where</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    /// <remarks>
    /// <c>Where</c> is the query-syntax spelling of <see cref="OptionExtensions.Filter{T}"/>. Unlike a result,
    /// an option can filter without producing an error value.
    /// <code><![CDATA[
    /// var city = Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .Where(static shipping => shipping.Country is "GB")
    ///     .Select(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is in the UK,
    /// // None when it is not - filtering never produces an error.
    /// ]]></code>
    /// </remarks>
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Filter(predicate);
}
