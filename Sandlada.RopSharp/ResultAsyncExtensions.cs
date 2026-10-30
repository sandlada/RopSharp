using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sandlada.RopSharp;

/// <summary>
/// Cancellation-aware asynchronous combinators for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// <para>
/// The map/bind/tee combinators have <c>...Async</c> counterparts, and the naming symmetry of the synchronous
/// surface is preserved (<c>MapOkAsync</c> vs <c>MapErrAsync</c>). Guards such as <c>Ensure</c>/<c>FailIf</c> are
/// synchronous because they do not perform I/O.
/// </para>
/// <para>
/// The cancellation token is always optional and always last. Cancellation is reported by throwing
/// <see cref="OperationCanceledException"/>; it is never turned into an error, because errors describe domain
/// failures. On the success track the token is checked before invoking the mapper or binder.
/// </para>
/// <para>
/// The library always uses <c>ConfigureAwait(false)</c>; callers choose their own synchronization context.
/// </para>
/// <para>
/// These methods are asynchronous: argument validation errors surface through the returned task rather than being
/// thrown synchronously. A callback that returns a <see cref="Task"/> must not return <see langword="null"/>; a null
/// task faults the returned task with <see cref="InvalidOperationException"/> naming the callback role
/// (<c>mapper</c>, <c>binder</c>, <c>fallback</c>, <c>action</c> or <c>predicate</c>). An error mapper that returns a
/// null error value faults with the same exception instead of producing an error-less failure.
/// </para>
/// </remarks>
/// <remarks>
/// <code><![CDATA[
/// var receiptId = await Rop.TryAsync(
///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
///         "E_CHARGE")
///     .MapOkAsync(static payment => payment.Id);
///
/// // Ok("pay-ord-1001") when the provider answered,
/// // Err(E_CHARGE) when the task faulted - the async surface mirrors the synchronous one.
/// ]]></code>
/// </remarks>
public static class ResultAsyncExtensions
{
    /// <summary>
    /// Wraps a synchronous result in a completed task.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>A task that completes with <paramref name="result"/>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .AsTask()
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - AsTask only re-wraps the result.
    /// ]]></code>
    /// </remarks>
    public static Task<Result<T, TErr>> AsTask<T, TErr>(this Result<T, TErr> result) =>
        Task.FromResult(result);

    /// <summary>
    /// Maps the success value of a task-wrapped result synchronously.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The mapped success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the success value. Not evaluated on the failure track.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(mapper(value))</c> or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the mapper never runs on the error track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut, TErr>> MapOkAsync<TOk, TOut, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, TOut> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Rop.OkUnchecked<TOut, TErr>(mapper(value));
    }

    /// <summary>
    /// Maps the success value of a task-wrapped result with an asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The mapped success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the success value asynchronously. Not evaluated on the failure track.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(mapped)</c> or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .MapOkAsync(static payment => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the asynchronous mapper never runs on the error track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut, TErr>> MapOkAsync<TOk, TOut, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var mapped = await AsyncGuards.Require(mapper(value), "mapper").ConfigureAwait(false);
        return Rop.OkUnchecked<TOut, TErr>(mapped);
    }

    /// <summary>
    /// Maps the success value of a task-wrapped result with a cancellation-aware asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The mapped success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the success value asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(mapped)</c> or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .MapOkAsync(static (payment, token) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the mapper receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(2)]
    public static async Task<Result<TOut, TErr>> MapOkAsync<TOk, TOut, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, CancellationToken, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var mapped = await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false);
        return Rop.OkUnchecked<TOut, TErr>(mapped);
    }

    /// <summary>
    /// Maps the success value of a synchronous result with an asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The mapped success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="mapper">Maps the success value asynchronously.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(mapped)</c> or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .MapOkAsync(static payment => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the asynchronous mapper never runs on the error track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(2)]
    public static async Task<Result<TOut, TErr>> MapOkAsync<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var mapped = await AsyncGuards.Require(mapper(value), "mapper").ConfigureAwait(false);
        return Rop.OkUnchecked<TOut, TErr>(mapped);
    }

    /// <summary>
    /// Maps the success value of a synchronous result with a cancellation-aware asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The mapped success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="mapper">Maps the success value asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(mapped)</c> or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .MapOkAsync(static (payment, token) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the mapper receives the same token.
    /// ]]></code>
    /// </remarks>
    [SuppressMessage(
        "ApiDesign",
        "RS0026:Do not add multiple overloads with optional parameters",
        Justification = "The overload differs by delegate arity, so existing one-argument lambda calls cannot become ambiguous; the optional token follows the library-wide ct-last convention.")]
    [OverloadResolutionPriority(3)]
    public static async Task<Result<TOut, TErr>> MapOkAsync<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, CancellationToken, Task<TOut>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var mapped = await AsyncGuards.Require(mapper(value, cancellationToken), "mapper").ConfigureAwait(false);
        return Rop.OkUnchecked<TOut, TErr>(mapped);
    }

    /// <summary>
    /// Maps the error of a task-wrapped result synchronously.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The current error type.</typeparam>
    /// <typeparam name="TErr2">The mapped error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the error. Not evaluated on the success track.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success or the mapped error.</returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the error mapper was invoked. The success track is passed through without
    /// observing the token.
    /// </exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"))
    ///     .MapErrAsync(static error => error.WithCode("E_PAYMENT_DECLINED"));
    ///
    /// // Err(E_PAYMENT_DECLINED) when the provider refused the order,
    /// // Ok(payment) keeps the original success - the mapper never runs on the success track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOk, TErr2>> MapErrAsync<TOk, TErr, TErr2>(
        this Task<Result<TOk, TErr>> task,
        Func<TErr, TErr2> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return Rop.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Rop.Err<TOk, TErr2>(Rop.RequireError(mapper(error)));
    }

    /// <summary>
    /// Maps the error of a task-wrapped result with an asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The current error type.</typeparam>
    /// <typeparam name="TErr2">The mapped error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the error asynchronously. Not evaluated on the success track.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success or the mapped error.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"))
    ///     .MapErrAsync(static error => Task.FromResult(error.WithCode("E_PAYMENT_DECLINED")));
    ///
    /// // Err(E_PAYMENT_DECLINED) when the provider refused the order,
    /// // Ok(payment) keeps the original success - the asynchronous mapper never runs on the success track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOk, TErr2>> MapErrAsync<TOk, TErr, TErr2>(
        this Task<Result<TOk, TErr>> task,
        Func<TErr, Task<TErr2>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return Rop.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Rop.Err<TOk, TErr2>(Rop.RequireError(await AsyncGuards.Require(mapper(error), "mapper").ConfigureAwait(false)));
    }

    /// <summary>
    /// Maps the error of a task-wrapped result with a cancellation-aware asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The current error type.</typeparam>
    /// <typeparam name="TErr2">The mapped error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="mapper">Maps the error asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success or the mapped error.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"))
    ///     .MapErrAsync(static (error, token) => Task.FromResult(error.WithCode("E_PAYMENT_DECLINED")));
    ///
    /// // Err(E_PAYMENT_DECLINED) when the provider refused the order,
    /// // Ok(payment) keeps the original success - the mapper receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(2)]
    public static async Task<Result<TOk, TErr2>> MapErrAsync<TOk, TErr, TErr2>(
        this Task<Result<TOk, TErr>> task,
        Func<TErr, CancellationToken, Task<TErr2>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(mapper);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return Rop.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Rop.Err<TOk, TErr2>(
            Rop.RequireError(
                await AsyncGuards.Require(mapper(error, cancellationToken), "mapper").ConfigureAwait(false)));
    }

    /// <summary>
    /// Maps the error of a synchronous result with an asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The current error type.</typeparam>
    /// <typeparam name="TErr2">The mapped error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="mapper">Maps the error asynchronously. Not evaluated on the success track.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success or the mapped error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .MapErrAsync(static error => Task.FromResult(error.WithCode("E_PAYMENT_DECLINED")));
    ///
    /// // Err(E_PAYMENT_DECLINED) when the provider refused the order,
    /// // Ok(payment) keeps the original success - the asynchronous mapper never runs on the success track.
    /// ]]></code>
    /// </remarks>
    [SuppressMessage(
        "ApiDesign",
        "RS0026:Do not add multiple overloads with optional parameters",
        Justification = "The overload differs by delegate arity, so existing one-argument lambda calls cannot become ambiguous; the optional token follows the library-wide ct-last convention.")]
    public static async Task<Result<TOk, TErr2>> MapErrAsync<TOk, TErr, TErr2>(
        this Result<TOk, TErr> result,
        Func<TErr, Task<TErr2>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (!result.TryGetErr(out var error))
        {
            return Rop.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Rop.Err<TOk, TErr2>(Rop.RequireError(await AsyncGuards.Require(mapper(error), "mapper").ConfigureAwait(false)));
    }

    /// <summary>
    /// Maps the error of a synchronous result with a cancellation-aware asynchronous mapper.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The current error type.</typeparam>
    /// <typeparam name="TErr2">The mapped error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="mapper">Maps the error asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success or the mapped error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .MapErrAsync(static (error, token) => Task.FromResult(error.WithCode("E_PAYMENT_DECLINED")));
    ///
    /// // Err(E_PAYMENT_DECLINED) when the provider refused the order,
    /// // Ok(payment) keeps the original success - the mapper receives the same token.
    /// ]]></code>
    /// </remarks>
    [SuppressMessage(
        "ApiDesign",
        "RS0026:Do not add multiple overloads with optional parameters",
        Justification = "The overload differs by delegate arity, so existing one-argument lambda calls cannot become ambiguous; the optional token follows the library-wide ct-last convention.")]
    public static async Task<Result<TOk, TErr2>> MapErrAsync<TOk, TErr, TErr2>(
        this Result<TOk, TErr> result,
        Func<TErr, CancellationToken, Task<TErr2>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (!result.TryGetErr(out var error))
        {
            return Rop.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Rop.Err<TOk, TErr2>(
            Rop.RequireError(
                await AsyncGuards.Require(mapper(error, cancellationToken), "mapper").ConfigureAwait(false)));
    }

    /// <summary>
    /// Expands the success value of a task-wrapped result with an asynchronous binder.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The bound success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="binder">Maps the success value to a result asynchronously. Not evaluated on the failure track.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound result, or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .BindOkAsync(static payment => Task.FromResult(Rop.Ok(payment.Id)));
    ///
    /// // Ok("pay-ord-1001") when the charge and the binding succeeded,
    /// // Err(E_CHARGE) when the charge failed - the binder never runs on the error track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut, TErr>> BindOkAsync<TOk, TOut, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, Task<Result<TOut, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binder);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await InvokeAsync(binder, value).ConfigureAwait(false);
    }

    /// <summary>
    /// Expands the success value of a task-wrapped result with a cancellation-aware asynchronous binder.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The bound success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="binder">Maps the success value to a result asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound result, or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .BindOkAsync(static (payment, token) => Task.FromResult(Rop.Ok(payment.Id)));
    ///
    /// // Ok("pay-ord-1001") when the charge and the binding succeeded,
    /// // Err(E_CHARGE) when the charge failed - the binder receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut, TErr>> BindOkAsync<TOk, TOut, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, CancellationToken, Task<Result<TOut, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(binder);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(binder(value, cancellationToken), "binder").ConfigureAwait(false);
    }

    /// <summary>
    /// Expands the success value of a synchronous result with an asynchronous binder.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The bound success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="binder">Maps the success value to a result asynchronously.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound result, or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .BindOkAsync(static payment => Task.FromResult(Rop.Ok(payment.Id)));
    ///
    /// // Ok("pay-ord-1001") when the charge and the binding succeeded,
    /// // Err(E_CHARGE) when the charge failed - the binder never runs on the error track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut, TErr>> BindOkAsync<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Task<Result<TOut, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(binder);

        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await InvokeAsync(binder, value).ConfigureAwait(false);
    }

    /// <summary>
    /// Expands the success value of a synchronous result with a cancellation-aware asynchronous binder.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The bound success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="binder">Maps the success value to a result asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The bound result, or the original error.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .BindOkAsync(static (payment, token) => Task.FromResult(Rop.Ok(payment.Id)));
    ///
    /// // Ok("pay-ord-1001") when the charge and the binding succeeded,
    /// // Err(E_CHARGE) when the charge failed - the binder receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut, TErr>> BindOkAsync<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, CancellationToken, Task<Result<TOut, TErr>>> binder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(binder);

        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(result.ErrUnsafe);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(binder(value, cancellationToken), "binder").ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from an error of a task-wrapped result with an asynchronous fallback.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="fallback">Maps the error to a result asynchronously. Not evaluated on the success track.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success, or the fallback result.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"))
    ///     .BindErrAsync(static error => Task.FromResult(Rop.Ok<Payment, RopError>(OrderFixtures.Charge(OrderFixtures.Sample))))
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the fallback succeeded,
    /// // Err(E_CHARGE) when the fallback failed too - the fallback never runs on the success track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOk, TErr>> BindErrAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TErr, Task<Result<TOk, TErr>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(fallback);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await InvokeFallbackAsync(fallback, error).ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from an error of a task-wrapped result with a cancellation-aware asynchronous fallback.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="fallback">Maps the error to a result asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success, or the fallback result.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"))
    ///     .BindErrAsync(static (error, token) => Task.FromResult(Rop.Ok<Payment, RopError>(OrderFixtures.Charge(OrderFixtures.Sample))))
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the fallback succeeded,
    /// // Err(E_CHARGE) when the fallback failed too - the fallback receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOk, TErr>> BindErrAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TErr, CancellationToken, Task<Result<TOk, TErr>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(fallback);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!result.TryGetErr(out var error))
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(fallback(error, cancellationToken), "fallback").ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from an error of a synchronous result with an asynchronous fallback.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="fallback">Maps the error to a result asynchronously.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success, or the fallback result.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .BindErrAsync(static error => Task.FromResult(Rop.Ok<Payment, RopError>(OrderFixtures.Charge(OrderFixtures.Sample))))
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the fallback succeeded,
    /// // Err(E_CHARGE) when the fallback failed too - the fallback never runs on the success track.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOk, TErr>> BindErrAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, Task<Result<TOk, TErr>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        if (!result.TryGetErr(out var error))
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await InvokeFallbackAsync(fallback, error).ConfigureAwait(false);
    }

    /// <summary>
    /// Recovers from an error of a synchronous result with a cancellation-aware asynchronous fallback.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="fallback">Maps the error to a result asynchronously, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The original success, or the fallback result.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .BindErrAsync(static (error, token) => Task.FromResult(Rop.Ok<Payment, RopError>(OrderFixtures.Charge(OrderFixtures.Sample))))
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the fallback succeeded,
    /// // Err(E_CHARGE) when the fallback failed too - the fallback receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOk, TErr>> BindErrAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, CancellationToken, Task<Result<TOk, TErr>>> fallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        if (!result.TryGetErr(out var error))
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await AsyncGuards.Require(fallback(error, cancellationToken), "fallback").ConfigureAwait(false);
    }

    /// <summary>
    /// Observes the success value of a task-wrapped result asynchronously.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect. Exceptions propagate.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same result value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .TeeOkAsync(static payment => Task.CompletedTask);
    ///
    /// // Ok(payment) runs the side effect and keeps the result,
    /// // Err(E_CHARGE) skips it - the result itself is never changed.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOk, TErr>> TeeOkAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetOk(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value), "action").ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Observes the success value of a task-wrapped result with a cancellation-aware asynchronous action.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same result value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .TeeOkAsync(static (payment, token) => Task.CompletedTask);
    ///
    /// // Ok(payment) runs the side effect and keeps the result,
    /// // Err(E_CHARGE) skips it - the action receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOk, TErr>> TeeOkAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetOk(out var value))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(value, cancellationToken), "action").ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Observes the error of a task-wrapped result asynchronously.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect. Exceptions propagate.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same result value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"))
    ///     .TeeErrAsync(static error => Task.CompletedTask);
    ///
    /// // Err(E_CHARGE) runs the side effect and keeps the result,
    /// // Ok(payment) skips it - the result itself is never changed.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOk, TErr>> TeeErrAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TErr, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetErr(out var error))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(error), "action").ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Observes the error of a task-wrapped result with a cancellation-aware asynchronous action.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="action">The asynchronous side effect, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>The same result value.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// await Task.FromResult(Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"))
    ///     .TeeErrAsync(static (error, token) => Task.CompletedTask);
    ///
    /// // Err(E_CHARGE) runs the side effect and keeps the result,
    /// // Ok(payment) skips it - the action receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOk, TErr>> TeeErrAsync<TOk, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TErr, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(action);

        var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (result.TryGetErr(out var error))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AsyncGuards.Require(action(error, cancellationToken), "action").ConfigureAwait(false);
        }

        return result;
    }

    private static Task<Result<TOut, TErr>> InvokeAsync<TOk, TOut, TErr>(
        Func<TOk, Task<Result<TOut, TErr>>> binder,
        TOk value) =>
        AsyncGuards.Require(binder(value), "binder");

    private static Task<Result<TOk, TErr>> InvokeFallbackAsync<TOk, TErr>(
        Func<TErr, Task<Result<TOk, TErr>>> fallback,
        TErr error) =>
        AsyncGuards.Require(fallback(error), "fallback");
}
