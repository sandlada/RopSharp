namespace Sandlada.RopSharp;

/// <summary>
/// Receiver-based combinators and terminals for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Every combinator is data-last (the mapper, binder, predicate or action is the last parameter) and returns a
/// new value; results are immutable, so chains are always safe.
/// </para>
/// <para>
/// Naming is fully symmetric: <c>MapOk</c>/<c>MapErr</c>, <c>BindOk</c>/<c>BindErr</c>, <c>TeeOk</c>/<c>TeeErr</c>.
/// There are deliberately no shortened aliases (<c>Map</c>, <c>Bind</c>, <c>Tee</c>).
/// </para>
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Maps the success value, leaving an error untouched.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The mapped success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="mapper">Maps the success value. Not evaluated on the failure track.</param>
    /// <returns><c>Ok(mapper(value))</c> or the original error.</returns>
    /// <remarks>
    /// If the mapper returns <see langword="null"/> for a nullable success type, the success track carries that null
    /// (the compiler annotation is preserved). Enforce non-nullness with <see cref="RequireNotNull{T}"/>, or use
    /// <c>Option</c> when absence is expected.
    /// </remarks>
    public static Result<TOut, TErr> MapOk<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return result.TryGetOk(out var value)
            ? Rop.OkUnchecked<TOut, TErr>(mapper(value))
            : Rop.Err<TOut, TErr>(result.ErrUnsafe);
    }

    /// <summary>
    /// Maps the error, leaving a success untouched.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The current error type.</typeparam>
    /// <typeparam name="TErr2">The mapped error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="mapper">Maps the error. Not evaluated on the success track.</param>
    /// <returns>The original success or <c>Err(mapper(error))</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The mapper returned <see langword="null"/>.</exception>
    /// <remarks>
    /// The success track is passed through unchanged, including a success that carries <see langword="null"/>
    /// after a nullable mapping (see <see cref="MapOk{TOk,TOut,TErr}"/>); the payload is never re-validated.
    /// </remarks>
    public static Result<TOk, TErr2> MapErr<TOk, TErr, TErr2>(
        this Result<TOk, TErr> result,
        Func<TErr, TErr2> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return result.TryGetErr(out var error)
            ? Rop.Err<TOk, TErr2>(Rop.RequireError(mapper(error)))
            : Rop.OkUnchecked<TOk, TErr2>(result.OkUnsafe);
    }

    /// <summary>
    /// Observes the success value without changing the track.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="action">The side effect. Exceptions propagate (programming errors are not swallowed).</param>
    /// <returns>The same result value.</returns>
    /// <remarks>Only for logging and metrics; the execution order of side effects is not part of the contract.</remarks>
    public static Result<TOk, TErr> TeeOk<TOk, TErr>(this Result<TOk, TErr> result, Action<TOk> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.TryGetOk(out var value))
        {
            action(value);
        }

        return result;
    }

    /// <summary>
    /// Observes the error without changing the track.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="action">The side effect. Exceptions propagate (programming errors are not swallowed).</param>
    /// <returns>The same result value.</returns>
    public static Result<TOk, TErr> TeeErr<TOk, TErr>(this Result<TOk, TErr> result, Action<TErr> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (result.TryGetErr(out var error))
        {
            action(error);
        }

        return result;
    }

    /// <summary>
    /// Turns a success into a failure when a predicate does not hold.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">The invariant to check. Not evaluated on the failure track.</param>
    /// <param name="error">The error used when the predicate returns <see langword="false"/>. Must not be <see langword="null"/>.</param>
    /// <returns>The original success when the predicate holds; otherwise <c>Err(error)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<TOk, TErr> Ensure<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, bool> predicate,
        TErr error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        return result.TryGetOk(out var value) && !predicate(value)
            ? Rop.Err<TOk, TErr>(error)
            : result;
    }

    /// <summary>
    /// Turns a success into a failure when a predicate does not hold, building the error lazily.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">The invariant to check. Not evaluated on the failure track.</param>
    /// <param name="errorFactory">Builds the error from the value. Evaluated only when the predicate fails.</param>
    /// <returns>The original success when the predicate holds; otherwise <c>Err(errorFactory(value))</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> or <paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    /// <remarks>Prefer this overload on hot paths: the message is only formatted on the failing branch.</remarks>
    public static Result<TOk, TErr> Ensure<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, bool> predicate,
        Func<TOk, TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        return result.TryGetOk(out var value) && !predicate(value)
            ? Rop.Err<TOk, TErr>(Rop.RequireError(errorFactory(value)))
            : result;
    }

    /// <summary>
    /// Replaces the success value with a constant, leaving an error untouched.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The replacement type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="value">The replacement value for the success track. Must not be <see langword="null"/>.</param>
    /// <returns><c>Ok(value)</c> or the original error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>To discard a value and keep only success/failure, use <c>MapOkValue(Unit.Value)</c>.</remarks>
    public static Result<TOut, TErr> MapOkValue<TOk, TOut, TErr>(this Result<TOk, TErr> result, TOut value)
    {
        ArgumentGuards.ThrowIfNull(value, nameof(value));
        return result.TryGetOk(out _)
            ? Rop.Ok<TOut, TErr>(value)
            : Rop.Err<TOut, TErr>(result.ErrUnsafe);
    }

    /// <summary>
    /// Turns a success carrying <see langword="null"/> into a failure.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <param name="result">The source result whose success value may be null.</param>
    /// <param name="errorFactory">Builds the error when the success value is <see langword="null"/>.</param>
    /// <returns>The original success when non-null; otherwise <c>Err(errorFactory())</c>. Errors pass through.</returns>
    public static Result<T, RopError> RequireNotNull<T>(
        this Result<T?, RopError> result,
        Func<RopError> errorFactory)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<T, RopError>(result.ErrUnsafe);
        }

        return value is null
            ? Rop.Err<T, RopError>(Rop.MaterializeError(errorFactory))
            : Rop.Ok<T, RopError>(value);
    }

    /// <summary>
    /// Turns a success carrying <see langword="null"/> into a failure with a caller-defined error type.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="result">The source result whose success value may be null.</param>
    /// <param name="errorFactory">Builds the error when the success value is <see langword="null"/>.</param>
    /// <returns>The original success when non-null; otherwise <c>Err(errorFactory())</c>. Errors pass through.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    /// <remarks>
    /// Use this overload when the caller's error system is not <see cref="RopError"/>. The factory is evaluated only
    /// on the null-success branch.
    /// </remarks>
    public static Result<T, TErr> RequireNotNull<T, TErr>(
        this Result<T?, TErr> result,
        Func<TErr> errorFactory)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        if (!result.TryGetOk(out var value))
        {
            return Rop.Err<T, TErr>(result.ErrUnsafe);
        }

        return value is null
            ? Rop.Err<T, TErr>(Rop.MaterializeError(errorFactory))
            : Rop.Ok<T, TErr>(value);
    }

    /// <summary>
    /// Expands a success value into a new result, leaving an error untouched.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The new success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="binder">Maps the success value to a result. Not evaluated on the failure track.</param>
    /// <returns>The bound result, or the original error.</returns>
    public static Result<TOut, TErr> BindOk<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<TOut, TErr>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return result.TryGetOk(out var value)
            ? binder(value)
            : Rop.Err<TOut, TErr>(result.ErrUnsafe);
    }

    /// <summary>
    /// Recovers from an error by producing a new result, leaving a success untouched.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="fallback">Maps the error to a result. Not evaluated on the success track.</param>
    /// <returns>The original success, or the fallback result.</returns>
    public static Result<TOk, TErr> BindErr<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, Result<TOk, TErr>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return result.TryGetErr(out var error)
            ? fallback(error)
            : result;
    }

    /// <summary>
    /// Recovers from an error with a constant success value, leaving a success untouched.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="fallback">The success value used on the failure track. Must not be <see langword="null"/>.</param>
    /// <returns>The original success, or <c>Ok(fallback)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is <see langword="null"/>.</exception>
    public static Result<TOk, TErr> BindErrValue<TOk, TErr>(this Result<TOk, TErr> result, TOk fallback)
    {
        ArgumentGuards.ThrowIfNull(fallback, nameof(fallback));
        return result.TryGetErr(out _)
            ? Rop.Ok<TOk, TErr>(fallback)
            : result;
    }

    /// <summary>
    /// Recovers from an error with a lazily built success value, leaving a success untouched.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="fallbackFactory">Builds the fallback value. Evaluated only on the failure track.</param>
    /// <returns>The original success, or <c>Ok(fallbackFactory())</c>.</returns>
    public static Result<TOk, TErr> BindErrValue<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        return result.TryGetErr(out _)
            ? Rop.Ok<TOk, TErr>(fallbackFactory())
            : result;
    }

    /// <summary>
    /// Exits the result by projecting both tracks into a single value.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <typeparam name="T">The projected type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="ok">Projects a success.</param>
    /// <param name="err">Projects a failure.</param>
    /// <returns>The projected value. This is the primary production exit.</returns>
    public static T Match<TOk, TErr, T>(
        this Result<TOk, TErr> result,
        Func<TOk, T> ok,
        Func<TErr, T> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);
        return result.TryGetOk(out var value) ? ok(value) : err(result.ErrUnsafe);
    }

    /// <summary>
    /// Returns the success value or throws when the result is a failure.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">The result is an error.</exception>
    /// <remarks>Intended for tests and boundaries only; production code should use <see cref="Match{TOk,TErr,T}"/>.</remarks>
    public static TOk OrThrow<TOk, TErr>(this Result<TOk, TErr> result) => result.TryGetOk(out var value)
        ? value
        : throw new InvalidOperationException($"Expected Ok but was {result}.");

    /// <summary>
    /// Returns the success value or throws when the result is a failure.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">The result is an error.</exception>
    public static TOk OkOrThrow<TOk, TErr>(this Result<TOk, TErr> result) => result.OrThrow();

    /// <summary>
    /// Returns the error value or throws when the result is a success.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The error value.</returns>
    /// <exception cref="InvalidOperationException">The result is a success.</exception>
    public static TErr ErrOrThrow<TOk, TErr>(this Result<TOk, TErr> result) => result.TryGetErr(out var error)
        ? error
        : throw new InvalidOperationException($"Expected Err but was {result}.");
}
