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
/// (<c>mapper</c>, <c>binder</c>, <c>fallback</c>, <c>action</c> or <c>predicate</c>).
/// </para>
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
        return Rop.Err<TOk, TErr2>(mapper(error));
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
        return Rop.Err<TOk, TErr2>(await AsyncGuards.Require(mapper(error), "mapper").ConfigureAwait(false));
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
            await AsyncGuards.Require(mapper(error, cancellationToken), "mapper").ConfigureAwait(false));
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
        return Rop.Err<TOk, TErr2>(await AsyncGuards.Require(mapper(error), "mapper").ConfigureAwait(false));
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
            await AsyncGuards.Require(mapper(error, cancellationToken), "mapper").ConfigureAwait(false));
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
