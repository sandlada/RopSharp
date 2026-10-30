using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sandlada.RopSharp;

/// <summary>
/// Cancellation-aware asynchronous combinators for <see cref="Option{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Rules mirror the result surface: the map/bind/tee/filter combinators have <c>...Async</c> counterparts, the
/// token is optional and last, and cancellation always throws <see cref="OperationCanceledException"/> (it never
/// becomes <c>None</c>).
/// </para>
/// <para>
/// The <c>Some</c> side functions are not invoked on the empty track. Terminal unwrapping is not paired: await first,
/// then call the synchronous conversion.
/// </para>
/// <para>
/// These methods are asynchronous: argument validation errors surface through the returned task rather than being
/// thrown synchronously. A callback that returns a <see cref="Task"/> must not return <see langword="null"/>; a null
/// task faults the returned task with <see cref="InvalidOperationException"/> naming the callback role
/// (<c>mapper</c>, <c>binder</c>, <c>fallback</c>, <c>action</c> or <c>predicate</c>).
/// </para>
/// </remarks>
/// <remarks>
/// <code><![CDATA[
/// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
///     .MapSomeAsync(static shipping => Task.FromResult(shipping.City));
///
/// // Some("London") when the customer has a shipping address,
/// // None when the address is missing - the async surface mirrors the synchronous one.
/// ]]></code>
/// </remarks>
[SuppressMessage(
    "ApiDesign",
    "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "The class exposes ct-last overloads for synchronous, asynchronous and cancellation-aware callback shapes; overloads differ by receiver or delegate arity, so existing calls cannot become ambiguous.")]
public static class OptionAsyncExtensions
{
    /// <summary>
    /// Wraps a synchronous option in a completed task.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <returns>A task that completes with <paramref name="option"/>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .AsTask()
    ///     .MapSomeAsync(static shipping => shipping.City);
    ///
    /// // Some("London") when the customer has a shipping address,
    /// // None when the address is missing - AsTask only re-wraps the option.
    /// ]]></code>
    /// </remarks>
    public static Task<Option<T>> AsTask<T>(this Option<T> option) => Task.FromResult(option);

    /// <summary>
    /// Maps the contained value of a task-wrapped option synchronously.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the value. Not evaluated for <c>None</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some(mapper(value))</c> or <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSomeAsync(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the mapper never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<TOut>> MapSomeAsync<T, TOut>(
        this Task<Option<T>> task,
        Func<T, TOut> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Opt.Some(mapper(value));
    }

    /// <summary>
    /// Maps the contained value of a task-wrapped option with an asynchronous mapper.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the value asynchronously. Not evaluated for <c>None</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some(mapped)</c> or <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSomeAsync(static shipping => Task.FromResult(shipping.City));
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the asynchronous mapper never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<TOut>> MapSomeAsync<T, TOut>(
        this Task<Option<T>> task,
        Func<T, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Opt.Some(await AsyncGuards.Require(mapper(value), "mapper").ConfigureAwait(false));
    }

    /// <summary>
    /// Maps the contained value of a task-wrapped option with a cancellation-aware asynchronous mapper.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the value asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some(mapped)</c> or <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSomeAsync(static (shipping, token) => Task.FromResult(shipping.City));
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the mapper receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(2)]
    public static async Task<Option<TOut>> MapSomeAsync<T, TOut>(
        this Task<Option<T>> task,
        Func<T, CancellationToken, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Opt.Some(await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false));
    }

    /// <summary>
    /// Expands the contained value of a task-wrapped option with an asynchronous binder.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The bound value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="binder">Maps the value to an option asynchronously. Not evaluated for <c>None</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound option, or <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .BindSomeAsync(static shipping => Task.FromResult(Opt.Some(shipping.City)));
    ///
    /// // Some("London") when the address is present and the binding succeeded,
    /// // None when it is missing - the binder never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<TOut>> BindSomeAsync<T, TOut>(
        this Task<Option<T>> task,
        Func<T, Task<Option<TOut>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binder);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await InvokeAsync(binder, value).ConfigureAwait(false);
    }

    /// <summary>
    /// Expands the contained value of a task-wrapped option with a cancellation-aware asynchronous binder.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The bound value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="binder">Maps the value to an option asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound option, or <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .BindSomeAsync(static (shipping, token) => Task.FromResult(Opt.Some(shipping.City)));
    ///
    /// // Some("London") when the address is present and the binding succeeded,
    /// // None when it is missing - the binder receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<TOut>> BindSomeAsync<T, TOut>(
        this Task<Option<T>> task,
        Func<T, CancellationToken, Task<Option<TOut>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binder);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(binder(value, cancellationToken), "binder").ConfigureAwait(false);
    }

    /// <summary>
    /// Expands the contained value of a synchronous option with an asynchronous binder.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The bound value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">Maps the value to an option asynchronously.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound option, or <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .BindSomeAsync(static shipping => Task.FromResult(Opt.Some(shipping.City)));
    ///
    /// // Some("London") when the address is present and the binding succeeded,
    /// // None when it is missing - the binder never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<TOut>> BindSomeAsync<T, TOut>(
        this Option<T> option,
        Func<T, Task<Option<TOut>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(binder);

        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await InvokeAsync(binder, value).ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from <c>None</c> on a task-wrapped option with an asynchronous fallback.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="fallback">Produces the fallback option asynchronously. Not evaluated for <c>Some</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option, or the fallback option.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .BindNoneAsync(static () => Task.FromResult(Opt.Some(OrderFixtures.Sample.Customer.Address!)));
    ///
    /// // Some(address) when the address is missing,
    /// // Some(city) when it is present - the fallback never runs on the present track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<T>> BindNoneAsync<T>(
        this Task<Option<T>> task,
        Func<Task<Option<T>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(fallback);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.IsSome)
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(fallback(), "fallback").ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from <c>None</c> on a task-wrapped option with a cancellation-aware asynchronous fallback.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="fallback">Produces the fallback option asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option, or the fallback option.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .BindNoneAsync(static (token) => Task.FromResult(Opt.Some(OrderFixtures.Sample.Customer.Address!)));
    ///
    /// // Some(address) when the address is missing,
    /// // Some(city) when it is present - the fallback receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> BindNoneAsync<T>(
        this Task<Option<T>> task,
        Func<CancellationToken, Task<Option<T>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(fallback);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.IsSome)
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(fallback(cancellationToken), "fallback").ConfigureAwait(false);
    }

    /// <summary>
    /// Observes the contained value of a task-wrapped option asynchronously.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect. Exceptions propagate.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .TeeSomeAsync(static shipping => Task.CompletedTask);
    ///
    /// // Some(address) runs the side effect and keeps the option,
    /// // None skips it - the option itself is never changed.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<T>> TeeSomeAsync<T>(
        this Task<Option<T>> task,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.TryGetValue(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Observes the contained value of a task-wrapped option with a cancellation-aware asynchronous action.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .TeeSomeAsync(static (shipping, token) => Task.CompletedTask);
    ///
    /// // Some(address) runs the side effect and keeps the option,
    /// // None skips it - the action receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> TeeSomeAsync<T>(
        this Task<Option<T>> task,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.TryGetValue(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value, cancellationToken), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Observes the empty track of a task-wrapped option asynchronously.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect. Exceptions propagate.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .TeeNoneAsync(static () => Task.CompletedTask);
    ///
    /// // None runs the side effect and keeps the option,
    /// // Some(address) skips it - the option itself is never changed.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<T>> TeeNoneAsync<T>(
        this Task<Option<T>> task,
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Observes the empty track of a task-wrapped option with a cancellation-aware asynchronous action.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address))
    ///     .TeeNoneAsync(static (token) => Task.CompletedTask);
    ///
    /// // None runs the side effect and keeps the option,
    /// // Some(address) skips it - the action receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> TeeNoneAsync<T>(
        this Task<Option<T>> task,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(cancellationToken), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Filters a task-wrapped option with a synchronous predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="predicate">The predicate. Not evaluated for <c>None</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .FilterAsync(static shipping => shipping.Country is "GB")
    ///     .MapSomeAsync(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is in the UK,
    /// // None when it is not - filtering never produces an error.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> task,
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(predicate);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return predicate(value) ? option : Opt.None<T>();
    }

    /// <summary>
    /// Filters a task-wrapped option with an asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="predicate">The asynchronous predicate. Not evaluated for <c>None</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .FilterAsync(static shipping => Task.FromResult(shipping.Country is "GB"))
    ///     .MapSomeAsync(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is in the UK,
    /// // None when it is not - the asynchronous predicate never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> task,
        Func<T, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(predicate);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(predicate(value), "predicate").ConfigureAwait(false)
            ? option
            : Opt.None<T>();
    }

    /// <summary>
    /// Maps the contained value of a synchronous option with an asynchronous mapper.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="mapper">Maps the value asynchronously. Not evaluated for <c>None</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some(mapped)</c> or <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the mapper was invoked.</exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSomeAsync(static shipping => Task.FromResult(shipping.City));
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the asynchronous mapper never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(2)]
    public static async Task<Option<TOut>> MapSomeAsync<T, TOut>(
        this Option<T> option,
        Func<T, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Opt.Some(await AsyncGuards.Require(mapper(value), "mapper").ConfigureAwait(false));
    }

    /// <summary>
    /// Maps the contained value of a synchronous option with a cancellation-aware asynchronous mapper.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="mapper">Maps the value asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some(mapped)</c> or <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the mapper was invoked.</exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSomeAsync(static (shipping, token) => Task.FromResult(shipping.City));
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the mapper receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(3)]
    public static async Task<Option<TOut>> MapSomeAsync<T, TOut>(
        this Option<T> option,
        Func<T, CancellationToken, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Opt.Some(
            await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false));
    }

    /// <summary>
    /// Expands the contained value of a synchronous option with a cancellation-aware asynchronous binder.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The bound value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">Maps the value to an option asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound option, or <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the binder was invoked.</exception>
    /// <exception cref="InvalidOperationException">The binder returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .BindSomeAsync(static (shipping, token) => Task.FromResult(Opt.Some(shipping.City)));
    ///
    /// // Some("London") when the address is present and the binding succeeded,
    /// // None when it is missing - the binder receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<TOut>> BindSomeAsync<T, TOut>(
        this Option<T> option,
        Func<T, CancellationToken, Task<Option<TOut>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(binder);

        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(binder(value, cancellationToken), "binder").ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from <c>None</c> on a synchronous option with an asynchronous fallback.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="fallback">Produces the fallback option asynchronously. Not evaluated for <c>Some</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option, or the fallback option.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the fallback was invoked.</exception>
    /// <exception cref="InvalidOperationException">The fallback returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .BindNoneAsync(static () => Task.FromResult(Opt.Some(OrderFixtures.Sample.Customer.Address!)));
    ///
    /// // Some(address) when the address is missing,
    /// // Some(city) when it is present - the fallback never runs on the present track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<T>> BindNoneAsync<T>(
        this Option<T> option,
        Func<Task<Option<T>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        if (option.IsSome)
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(fallback(), "fallback").ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from <c>None</c> on a synchronous option with a cancellation-aware asynchronous fallback.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="fallback">Produces the fallback option asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option, or the fallback option.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the fallback was invoked.</exception>
    /// <exception cref="InvalidOperationException">The fallback returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .BindNoneAsync(static (token) => Task.FromResult(Opt.Some(OrderFixtures.Sample.Customer.Address!)));
    ///
    /// // Some(address) when the address is missing,
    /// // Some(city) when it is present - the fallback receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> BindNoneAsync<T>(
        this Option<T> option,
        Func<CancellationToken, Task<Option<T>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        if (option.IsSome)
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(fallback(cancellationToken), "fallback").ConfigureAwait(false);
    }

    /// <summary>
    /// Observes the contained value of a synchronous option asynchronously.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The asynchronous side effect. Exceptions propagate.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the action was invoked.</exception>
    /// <exception cref="InvalidOperationException">The action returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .TeeSomeAsync(static shipping => Task.CompletedTask);
    ///
    /// // Some(address) runs the side effect and keeps the option,
    /// // None skips it - the option itself is never changed.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<T>> TeeSomeAsync<T>(
        this Option<T> option,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (option.TryGetValue(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Observes the contained value of a synchronous option with a cancellation-aware asynchronous action.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The asynchronous side effect, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the action was invoked.</exception>
    /// <exception cref="InvalidOperationException">The action returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .TeeSomeAsync(static (shipping, token) => Task.CompletedTask);
    ///
    /// // Some(address) runs the side effect and keeps the option,
    /// // None skips it - the action receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> TeeSomeAsync<T>(
        this Option<T> option,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (option.TryGetValue(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value, cancellationToken), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Observes the empty track of a synchronous option asynchronously.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The asynchronous side effect. Exceptions propagate.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the action was invoked.</exception>
    /// <exception cref="InvalidOperationException">The action returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// await Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .TeeNoneAsync(static () => Task.CompletedTask);
    ///
    /// // None runs the side effect and keeps the option,
    /// // Some(address) skips it - the option itself is never changed.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<T>> TeeNoneAsync<T>(
        this Option<T> option,
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Observes the empty track of a synchronous option with a cancellation-aware asynchronous action.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The asynchronous side effect, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same option value.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the action was invoked.</exception>
    /// <exception cref="InvalidOperationException">The action returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// await Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .TeeNoneAsync(static (token) => Task.CompletedTask);
    ///
    /// // None runs the side effect and keeps the option,
    /// // Some(address) skips it - the action receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> TeeNoneAsync<T>(
        this Option<T> option,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (option.IsNone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(cancellationToken), "action").ConfigureAwait(false);
        }

        return option;
    }

    /// <summary>
    /// Filters a synchronous option with an asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="predicate">The asynchronous predicate. Not evaluated for <c>None</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the predicate was invoked.</exception>
    /// <exception cref="InvalidOperationException">The predicate returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .FilterAsync(static shipping => Task.FromResult(shipping.Country is "GB"))
    ///     .MapSomeAsync(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is in the UK,
    /// // None when it is not - the asynchronous predicate never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (!option.TryGetValue(out var value))
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(predicate(value), "predicate").ConfigureAwait(false)
            ? option
            : Opt.None<T>();
    }

    /// <summary>
    /// Filters a synchronous option with a cancellation-aware asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="predicate">The asynchronous predicate, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the predicate was invoked.</exception>
    /// <exception cref="InvalidOperationException">The predicate returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .FilterAsync(static (shipping, token) => Task.FromResult(shipping.Country is "GB"))
    ///     .MapSomeAsync(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is in the UK,
    /// // None when it is not - the predicate receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(2)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Option<T> option,
        Func<T, CancellationToken, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (!option.TryGetValue(out var value))
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(predicate(value, cancellationToken), "predicate").ConfigureAwait(false)
            ? option
            : Opt.None<T>();
    }

    /// <summary>
    /// Filters a task-wrapped option with a cancellation-aware asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="predicate">The asynchronous predicate, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the predicate was invoked.</exception>
    /// <exception cref="InvalidOperationException">The predicate returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .FilterAsync(static (shipping, token) => Task.FromResult(shipping.Country is "GB"))
    ///     .MapSomeAsync(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is in the UK,
    /// // None when it is not - the predicate receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(2)]
    public static async Task<Option<T>> FilterAsync<T>(
        this Task<Option<T>> task,
        Func<T, CancellationToken, Task<bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(predicate);

        var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return option;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(predicate(value, cancellationToken), "predicate").ConfigureAwait(false)
            ? option
            : Opt.None<T>();
    }

    private static Task<Option<TOut>> InvokeAsync<T, TOut>(
        Func<T, Task<Option<TOut>>> binder,
        T value) =>
        AsyncGuards.Require(binder(value), "binder");
}
