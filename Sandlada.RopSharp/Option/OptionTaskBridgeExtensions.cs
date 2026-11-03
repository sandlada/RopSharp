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
}
