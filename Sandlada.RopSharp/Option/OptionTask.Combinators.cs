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
        return new(OptionTaskCore.MapSome(_task!, mapper, _cancellationToken), TagReady, _cancellationToken);
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
        return new(OptionTaskCore.MapSomeAsync(_task!, mapper, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Replaces the contained value with a constant value on the present track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await OptionTask.From(Option.Some(OrderFixtures.Sample.Customer))
    ///     .MapSomeValue("preferred");
    ///
    /// // Some("preferred") when present,
    /// // None when empty - the replacement value is never null.
    /// ]]></code>
    /// </remarks>
    public OptionTask<TOut> MapSomeValue<TOut>(TOut value)
    {
        ArgumentGuards.ThrowIfNull(value, nameof(value));
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("MapSomeValue");
        return new(OptionTaskCore.MapSome(_task!, _ => value, _cancellationToken), TagReady, _cancellationToken);
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
        return new(OptionTaskCore.BindSome(_task!, binder, _cancellationToken), TagReady, _cancellationToken);
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
        return new(OptionTaskCore.BindSomeAsync(_task!, binder, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Recovers on the empty track with an asynchronous fallback.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .MapSome(static shipping => shipping.City)
    ///     .BindNoneAsync(static _ => Task.FromResult(Option.Some("unknown")));
    ///
    /// // Some("unknown") when the address was missing,
    /// // Some(city) when it was present - the fallback never runs on the present track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> BindNoneAsync(Func<CancellationToken, Task<Option<T>>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        ThrowIfUninitialized();
        return new(OptionTaskCore.BindNoneAsync(_task!, fallback, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Recovers on the empty track with a synchronous fallback option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.None<string>())
    ///     .BindNone(static () => Option.Some("unknown"));
    ///
    /// // Some("unknown") when empty, Some(city) when present.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> BindNone(Func<Option<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        ThrowIfUninitialized();
        return new(OptionTaskCore.BindNone(_task!, fallback, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Recovers on the empty track with a fallback value.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.None<string>())
    ///     .BindNoneValue("unknown");
    ///
    /// // Some("unknown") when empty, Some(city) when present.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> BindNoneValue(T fallback)
    {
        ArgumentGuards.ThrowIfNull(fallback, nameof(fallback));
        return BindNone(() => new Option<T>(fallback, isSome: true));
    }

    /// <summary>Recovers on the empty track with a fallback value factory.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.None<string>())
    ///     .BindNoneValue(static () => "unknown");
    ///
    /// // Some("unknown") when empty, Some(city) when present.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> BindNoneValue(Func<T> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        return BindNone(() => new Option<T>(
            ArgumentGuards.RequireNonNullReturn(fallbackFactory(), "fallback factory"),
            isSome: true));
    }

    /// <summary>Recovers on the empty track with an asynchronous fallback value factory.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.None<string>())
    ///     .BindNoneValueAsync(static _ => Task.FromResult("unknown"));
    ///
    /// // Some("unknown") when empty, Some(city) when present - the factory return value is never null.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> BindNoneValueAsync(Func<CancellationToken, Task<T>> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        ThrowIfUninitialized();
        return new(OptionTaskCore.BindNoneValueAsync(_task!, fallbackFactory, _cancellationToken), TagReady, _cancellationToken);
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
        return new(OptionTaskCore.Filter(_task!, predicate, _cancellationToken), TagReady, _cancellationToken);
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
        return new(OptionTaskCore.FilterAsync(_task!, predicate, _cancellationToken), TagReady, _cancellationToken);
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
    public OptionTask<T> TeeSome(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();
        return new(OptionTaskCore.TeeSome(_task!, action, _cancellationToken), TagReady, _cancellationToken);
    }

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
    public OptionTask<T> TeeSomeAsync(Func<T, CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();
        return new(OptionTaskCore.TeeSomeAsync(_task!, action, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Runs a side effect on the empty track without changing the option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .MapSome(static shipping => shipping.City)
    ///     .TeeNone(static () => Console.WriteLine("no address"))
    ///     .BindNoneAsync(static _ => Task.FromResult(Option.Some("unknown")));
    ///
    /// // Some("unknown") when the address was missing,
    /// // None when a value was already present - the fallback never runs on the present track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> TeeNone(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();
        return new(OptionTaskCore.TeeNone(_task!, action, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Runs an asynchronous side effect on the empty track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .MapSome(static shipping => shipping.City)
    ///     .TeeNoneAsync(static _ => Task.Run(() => Console.WriteLine("no address")))
    ///     .BindNoneAsync(static _ => Task.FromResult(Option.Some("unknown")));
    ///
    /// // Some("unknown") after the asynchronous empty-track log,
    /// // None when a value was present - TeeNoneAsync never runs on the present track.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> TeeNoneAsync(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();
        return new(OptionTaskCore.TeeNoneAsync(_task!, action, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Exits the computation asynchronously by projecting both tracks into a single value.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MatchAsync(
    ///         some: static (shipping, _) => Task.FromResult($"ships to {shipping.City}"),
    ///         none: static _ => Task.FromResult("no shipping address"));
    ///
    /// // "ships to London" for Some(address),
    /// // "no shipping address" for None.
    /// ]]></code>
    /// </remarks>
    public Task<TResult> MatchAsync<TResult>(
        Func<T, CancellationToken, Task<TResult>> some,
        Func<CancellationToken, Task<TResult>> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        ThrowIfUninitialized();
        return OptionTaskCore.MatchAsync(_task!, some, none, _cancellationToken);
    }

    /// <summary>Runs one of two asynchronous actions depending on the completed option track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .SwitchAsync(
    ///         some: static (shipping, _) => Task.Run(() => Console.WriteLine(shipping.City)),
    ///         none: static _ => Task.Run(() => Console.WriteLine("no shipping address")));
    ///
    /// // Some(address) runs the some handler,
    /// // None runs the none handler once the option task settles.
    /// ]]></code>
    /// </remarks>
    public Task SwitchAsync(
        Func<T, CancellationToken, Task> some,
        Func<CancellationToken, Task> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        ThrowIfUninitialized();
        return OptionTaskCore.SwitchAsync(_task!, some, none, _cancellationToken);
    }

    /// <summary>Awaits the computation and returns the contained value, or throws when <c>None</c>.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var address = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .ExpectAsync("checkout requires a saved address");
    ///
    /// // Returns the address for Some(address),
    /// // throws InvalidOperationException with the reason when None.
    /// ]]></code>
    /// </remarks>
    public Task<T> ExpectAsync(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ThrowIfUninitialized();
        return OptionTaskCore.ExpectAsync(_task!, reason, _cancellationToken);
    }
}
