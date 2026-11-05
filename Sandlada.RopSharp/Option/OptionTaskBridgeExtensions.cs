namespace Sandlada.RopSharp;

/// <summary>
/// Bridges synchronous <see cref="Option{T}"/> values into <see cref="OptionTask{T}"/> pipelines.
/// </summary>
/// <remarks>
/// <code><![CDATA[
/// var city = await Option.FromNullable(OrderFixtures.Sample.Customer.Address)
///     .ToOptionTask()
///     .MapSomeAsync(static (shipping, _) => Task.FromResult(shipping.City));
///
/// // Some("London") when the address is present,
/// // None when it is missing - ToOptionTask lifts a completed option.
/// ]]></code>
/// </remarks>
public static class OptionTaskBridgeExtensions
{
    /// <summary>Wraps an option as a completed computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .ToOptionTask()
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the source option is Some,
    /// // None when it is empty.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> ToOptionTask<T>(this Option<T> option) => OptionTask<T>.From(option);

    /// <summary>Adopts a task that already carries an option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// Task<Option<Address>> lookup = Task.FromResult(
    ///     Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var city = await lookup.ToOptionTask()
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the task completes with Some,
    /// // None when it completes empty.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> ToOptionTask<T>(this Task<Option<T>> task) => OptionTask<T>.From(task);

    /// <summary>Maps the some track with an asynchronous mapper.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSomeAsync(static (shipping, _) => Task.FromResult(shipping.City));
    ///
    /// // Some("London") when present, None when missing.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> MapSomeAsync<T, TOut>(
        this Option<T> option,
        Func<T, CancellationToken, Task<TOut>> mapper) =>
        OptionTask<T>.From(option).MapSomeAsync(mapper);

    /// <summary>Chains an asynchronous binder on the some track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .BindSomeAsync(static (shipping, _) => Task.FromResult(Option.Some(shipping.City)));
    ///
    /// // Some("London") when the binder completes with Some,
    /// // None when the input or binder result is empty.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> BindSomeAsync<T, TOut>(
        this Option<T> option,
        Func<T, CancellationToken, Task<Option<TOut>>> binder) =>
        OptionTask<T>.From(option).BindSomeAsync(binder);

    /// <summary>Recovers on the empty track with an asynchronous fallback.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Option.None<string>()
    ///     .BindNoneAsync(static _ => Task.FromResult(Option.Some("unknown")));
    ///
    /// // Some("unknown") when empty,
    /// // None when the fallback also produces None.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> BindNoneAsync<T>(
        this Option<T> option,
        Func<CancellationToken, Task<Option<T>>> fallback) =>
        OptionTask<T>.From(option).BindNoneAsync(fallback);

    /// <summary>Filters the option with an asynchronous predicate.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var line = await Option.Some(OrderFixtures.Sample.Lines[0])
    ///     .FilterAsync(static (sku, _) => Task.FromResult(sku.Quantity > 0));
    ///
    /// // Some(line) when the predicate passes,
    /// // None when the predicate fails.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> FilterAsync<T>(
        this Option<T> option,
        Func<T, CancellationToken, Task<bool>> predicate) =>
        OptionTask<T>.From(option).FilterAsync(predicate);

    /// <summary>Runs an asynchronous side effect on the some track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Option.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .TeeSomeAsync(static (shipping, _) => Task.Run(() => Console.WriteLine(shipping.City)));
    ///
    /// // Some(address) when present,
    /// // None when missing - TeeSomeAsync only observes Some.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> TeeSomeAsync<T>(
        this Option<T> option,
        Func<T, CancellationToken, Task> action) =>
        OptionTask<T>.From(option).TeeSomeAsync(action);

    /// <summary>Runs an asynchronous side effect on the empty track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var empty = await Option.None<string>()
    ///     .TeeNoneAsync(static _ => Task.Run(() => Console.WriteLine("empty")));
    ///
    /// // None when missing,
    /// // Some("value") when present - TeeNoneAsync only observes None.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> TeeNoneAsync<T>(
        this Option<T> option,
        Func<CancellationToken, Task> action) =>
        OptionTask<T>.From(option).TeeNoneAsync(action);
}
