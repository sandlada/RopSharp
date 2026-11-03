namespace Sandlada.RopSharp;

/// <summary>Combinators for <see cref="OptionTask{T}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
///     .MapSome(static shipping => shipping.City);
///
/// // Some("London") on the present track,
/// // None when the address is missing - combinators mirror OptionExtensions.
/// ]]></code>
/// </remarks>
public readonly partial struct OptionTask<T>
{
    /// <summary>Maps the contained value when the computation completes with <c>Some</c>.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the mapper never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<TOut> MapSome<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("MapSome");
        return OptionTask<TOut>.From(OptionTaskCore.MapSome(_task!, mapper, _cancellationToken));
    }

    /// <summary>Maps the contained value with an asynchronous mapper that receives the computation token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSomeAsync(static (shipping, _) => Task.FromResult(shipping.City));
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the asynchronous mapper never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<TOut> MapSomeAsync<TOut>(Func<T, CancellationToken, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();
        return OptionTask<TOut>.From(OptionTaskCore.MapSomeAsync(_task!, mapper, _cancellationToken));
    }

    /// <summary>Chains another option-producing step on the present track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .BindSome(static shipping => Option.Some(shipping.City));
    ///
    /// // Some("London") when the address is present,
    /// // None when the binder returns None or the input was None.
    /// ]]></code>
    /// </remarks>
    public OptionTask<TOut> BindSome<TOut>(Func<T, Option<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("BindSome");
        return OptionTask<TOut>.From(OptionTaskCore.BindSome(_task!, binder, _cancellationToken));
    }

    /// <summary>Chains an asynchronous binder on the present track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .BindSomeAsync(static (shipping, _) => Task.FromResult(Option.Some(shipping.City)));
    ///
    /// // Some("London") when the binder completes with Some,
    /// // None when the input or binder result is empty.
    /// ]]></code>
    /// </remarks>
    public OptionTask<TOut> BindSomeAsync<TOut>(Func<T, CancellationToken, Task<Option<TOut>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        ThrowIfUninitialized();
        return OptionTask<TOut>.From(OptionTaskCore.BindSomeAsync(_task!, binder, _cancellationToken));
    }

    /// <summary>Recovers on the empty track with an asynchronous fallback.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .MapSome(static shipping => shipping.City)
    ///     .BindNone(static _ => Task.FromResult(Option.Some("unknown")));
    ///
    /// // Some("unknown") when the address was missing,
    /// // Some(city) when it was present - the fallback never runs on the present track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> BindNone(Func<CancellationToken, Task<Option<T>>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        ThrowIfUninitialized();
        return From(OptionTaskCore.BindNone(_task!, fallback, _cancellationToken));
    }

    /// <summary>Keeps the value only when the predicate holds.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var line = await OptionTask.From(Option.Some(OrderFixtures.Sample.Lines[0]))
    ///     .Filter(static sku => sku.Quantity > 0);
    ///
    /// // Some(line) when the quantity is positive,
    /// // None when the predicate fails or the input was None.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> Filter(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ThrowIfUninitialized();
        return From(OptionTaskCore.Filter(_task!, predicate, _cancellationToken));
    }

    /// <summary>Keeps the value only when the asynchronous predicate holds.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var line = await OptionTask.From(Option.Some(OrderFixtures.Sample.Lines[0]))
    ///     .FilterAsync(static (sku, _) => Task.FromResult(sku.Quantity > 0));
    ///
    /// // Some(line) when the predicate completes true,
    /// // None when it completes false or the input was None.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> FilterAsync(Func<T, CancellationToken, Task<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ThrowIfUninitialized();
        return From(OptionTaskCore.FilterAsync(_task!, predicate, _cancellationToken));
    }

    /// <summary>Runs a side effect on the present track without changing the option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .TeeSome(static shipping => Console.WriteLine(shipping.City))
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") after logging the city,
    /// // None when the address is missing - TeeSome never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> TeeSome(Action<T> action) =>
        From(OptionTaskCore.TeeSome(_task!, action, _cancellationToken));

    /// <summary>Runs an asynchronous side effect on the present track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .TeeSomeAsync(static (shipping, _) => Task.Run(() => Console.WriteLine(shipping.City)))
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") after the asynchronous observation,
    /// // None when the address is missing.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> TeeSomeAsync(Func<T, CancellationToken, Task> action) =>
        From(OptionTaskCore.TeeSomeAsync(_task!, action, _cancellationToken));

    /// <summary>Runs a side effect on the empty track without changing the option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .MapSome(static shipping => shipping.City)
    ///     .TeeNone(static () => Console.WriteLine("no address"))
    ///     .BindNone(static _ => Task.FromResult(Option.Some("unknown")));
    ///
    /// // Some("unknown") when the address was missing,
    /// // None when a value was already present - the fallback never runs on the present track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> TeeNone(Action action) =>
        From(OptionTaskCore.TeeNone(_task!, action, _cancellationToken));

    /// <summary>Runs an asynchronous side effect on the empty track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .MapSome(static shipping => shipping.City)
    ///     .TeeNoneAsync(static _ => Task.Run(() => Console.WriteLine("no address")))
    ///     .BindNone(static _ => Task.FromResult(Option.Some("unknown")));
    ///
    /// // Some("unknown") after the asynchronous empty-track log,
    /// // None when a value was present - TeeNoneAsync never runs on the present track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> TeeNoneAsync(Func<CancellationToken, Task> action) =>
        From(OptionTaskCore.TeeNoneAsync(_task!, action, _cancellationToken));
}
