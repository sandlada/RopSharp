namespace Sandlada.RopSharp;

/// <summary>LINQ query-syntax support for <see cref="OptionTask{T}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
///     .Select(static shipping => shipping.City);
///
/// // Some("London") when the address is present,
/// // None when it is missing - query syntax desugars to Select, SelectMany, and Where.
/// ]]></code>
/// </remarks>
public static class OptionTaskLinqExtensions
{
    /// <summary>Projects a present value after the computation completes.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .Select(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> Select<T, TOut>(
        this OptionTask<T> computation,
        Func<T, TOut> selector) => computation.MapSome(selector);

    /// <summary>Binds the present track to another option-producing step.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer))
    ///     .SelectMany(static customer => Option.FromNullable(customer.Address));
    ///
    /// // Some(address) when customer and address are present,
    /// // None when either is missing.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> SelectMany<T, TOut>(
        this OptionTask<T> computation,
        Func<T, Option<TOut>> binder) => computation.BindSome(binder);

    /// <summary>Binds and projects both present values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer))
    ///     .SelectMany(
    ///         static customer => Option.FromNullable(customer.Address),
    ///         static (customer, address) => $"{customer.Name}:{address.City}");
    ///
    /// // Some("Alice:London") when both steps succeed,
    /// // None when either step is missing.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> SelectMany<T, TCol, TOut>(
        this OptionTask<T> computation,
        Func<T, Option<TCol>> collectionSelector,
        Func<T, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return computation.BindSome(first => collectionSelector(first).MapSome(second => resultSelector(first, second)));
    }

    /// <summary>Binds the present track to another asynchronous option computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer))
    ///     .SelectMany(static customer =>
    ///         OptionTask.From(Task.FromResult(Option.FromNullable(customer.Address))));
    ///
    /// // Some(address) when customer and address are present,
    /// // None when either is missing.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> SelectMany<T, TOut>(
        this OptionTask<T> computation,
        Func<T, OptionTask<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        computation.ThrowIfUninitialized();

        return computation.BindSomeAsync(async (first, ct) =>
        {
            var next = binder(first);
            return await next.WithCancellation(ct);
        });
    }

    /// <summary>Binds and projects present values from an asynchronous option computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer))
    ///     .SelectMany(
    ///         static customer => OptionTask.From(Task.FromResult(Option.FromNullable(customer.Address))),
    ///         static (customer, address) => $"{customer.Name}:{address.City}");
    ///
    /// // Some("Alice:London") when both steps succeed,
    /// // None when either step is missing.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> SelectMany<T, TCol, TOut>(
        this OptionTask<T> computation,
        Func<T, OptionTask<TCol>> collectionSelector,
        Func<T, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        computation.ThrowIfUninitialized();

        return computation.BindSomeAsync(async (first, ct) =>
        {
            var next = collectionSelector(first);
            var colOption = await next.WithCancellation(ct);
            return colOption.MapSome(second => resultSelector(first, second));
        });
    }

    /// <summary>Filters the present value after the computation completes.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var active = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer))
    ///     .Where(static customer => customer.Name.Length > 0);
    ///
    /// // Some(customer) when customer is present and has a non-empty name,
    /// // None when missing or unnamed.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> Where<T>(
        this OptionTask<T> computation,
        Func<T, bool> predicate) => computation.Filter(predicate);
}
