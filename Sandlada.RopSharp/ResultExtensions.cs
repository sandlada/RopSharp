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
/// <para>
/// These combinators are the vocabulary of a pipeline. Each one works on exactly one track and leaves the
/// other untouched, which is what keeps a long chain readable: the shape of the data says where the chain is,
/// and the shape of the errors says where it can still go.
/// </para>
/// <code><![CDATA[
/// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
///     .MapOk(static payment => payment.Id)
///     .Ensure(static id => id.Length > 0, static id => RopError.Create("E_RECEIPT", "the receipt id is empty"));
///
/// // Ok(receiptId) when the charge succeeded and the receipt is well formed,
/// // Err(E_CHARGE) or Err(E_RECEIPT) when a step failed - the chain short-circuits.
/// ]]></code>
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
    /// <para>
    /// If the mapper returns <see langword="null"/> for a nullable success type, the success track carries that null
    /// (the compiler annotation is preserved). Enforce non-nullness with <see cref="RequireNotNull{T}"/>, or use
    /// <c>Option</c> when absence is expected.
    /// </para>
    /// <para>
    /// <c>MapOk</c> reshapes a payload without touching the error track, which makes it the step that turns a
    /// boundary result into the value the caller actually asked for. It never runs on the failure track, so the
    /// mapper is free to assume a non-null success value.
    /// </para>
    /// <code><![CDATA[
    /// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok(receiptId) when the payment provider answered,
    /// // Err(E_CHARGE) when the provider threw - the mapper is never evaluated on the error track.
    /// ]]></code>
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
    /// <para>
    /// The success track is passed through unchanged, including a success that carries <see langword="null"/>
    /// after a nullable mapping (see <see cref="MapOk{TOk,TOut,TErr}"/>); the payload is never re-validated.
    /// </para>
    /// <para>
    /// This is where an error is given its final shape: a retagged code, a translated message or an error of a
    /// different type for a caller that does not speak <see cref="RopError"/>.
    /// </para>
    /// <code><![CDATA[
    /// var placed = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .MapErr(static error => error.WithCode("E_PAYMENT_DECLINED"));
    ///
    /// // Err(E_PAYMENT_DECLINED) when the provider refused the order,
    /// // Ok(payment) keeps the original success - the mapper never runs there.
    /// ]]></code>
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
    /// <remarks>Only for logging and metrics; the execution order of side effects is not part of the contract.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .TeeOk(static payment => Console.WriteLine($"captured {payment.Id}"));
    ///
    /// // Ok(payment) runs the side effect and keeps the result,
    /// // Err(E_CHARGE) skips it - the result itself is never changed.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>Only for logging and metrics; the execution order of side effects is not part of the contract.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .TeeErr(static error => Console.WriteLine($"failed {error.Code}"));
    ///
    /// // Err(E_CHARGE) runs the side effect and keeps the result,
    /// // Ok(payment) skips it - the result itself is never changed.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <c>Ensure</c> is the assertion of a result pipeline: it states an invariant that the success payload has
    /// to satisfy. It is the shortest way to reject a value that would break a later step.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .Ensure(static payment => payment.Amount > 0m, RopError.Create("E_AMOUNT", "the amount must be positive"));
    ///
    /// // Ok(payment) when the amount is positive,
    /// // Err(E_AMOUNT) when it is not - the predicate never runs on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>Prefer this overload on hot paths: the message is only formatted on the failing branch.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .Ensure(
    ///         static payment => payment.Amount > 0m,
    ///         static payment => RopError.Create("E_AMOUNT", $"the amount {payment.Amount} must be positive"));
    ///
    /// // Ok(payment) when the amount is positive,
    /// // Err(E_AMOUNT) when it is not - the factory runs only on the failing branch.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>To discard a value and keep only success/failure, use <c>MapOkValue(Unit.Value)</c>.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .MapOkValue(Unit.Value);
    ///
    /// // Ok(()) when the charge succeeded,
    /// // Err(E_CHARGE) keeps the original error - the payload is replaced, not mapped.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// This is the bridge from a nullable payload back to a trustworthy one. It is the counterpart of
    /// <c>MapOk</c>, which may legitimately produce a null success value.
    /// <code><![CDATA[
    /// var placed = Rop.Ok<Order?, RopError>(OrderFixtures.Sample)
    ///     .RequireNotNull(() => RopError.Create("E_ORDER", "the order is missing"))
    ///     .MapOk(static order => order.Id);
    ///
    /// // Ok("ord-1001") when the success value is not null,
    /// // Err(E_ORDER) when it is null - errors pass through unchanged.
    /// ]]></code>
    /// </remarks>
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
    /// <code><![CDATA[
    /// var placed = Rop.Ok<Order?, string>(OrderFixtures.Sample)
    ///     .RequireNotNull(() => "the order is missing")
    ///     .MapOk(static order => order.Id);
    ///
    /// // Ok("ord-1001") when the success value is not null,
    /// // Err(the order is missing) when it is null - errors pass through unchanged.
    /// ]]></code>
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
    /// <remarks>
    /// <c>BindOk</c> is the expansion step: the binder returns a whole new result, so it may fail in its own
    /// right. Use it when the next step of the pipeline can fail, and <see cref="MapOk{TOk,TOut,TErr}"/> when it
    /// cannot.
    /// <code><![CDATA[
    /// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .BindOk(static payment => Rop.Ok(payment.Id));
    ///
    /// // Ok(receiptId) when the charge succeeded and the follow-up step succeeded,
    /// // Err(E_CHARGE) when the charge failed - the binder never runs on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <c>BindErr</c> is the recovery step: the fallback returns a whole new result, so it may fail again. It is
    /// how a pipeline handles a known failure without giving up the success track.
    /// <code><![CDATA[
    /// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .BindErr(static error => Rop.Ok<Payment, RopError>(OrderFixtures.Charge(OrderFixtures.Sample)))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok(receiptId) when the fallback succeeded,
    /// // Err(E_CHARGE) when the fallback failed too - the fallback never runs on the success track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// The constant fallback is the cheapest recovery: the pipeline continues with a known value instead of
    /// failing. Use it when the caller can live with a default.
    /// <code><![CDATA[
    /// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .BindErrValue(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok(receiptId) when the fallback produced a payment,
    /// // Err(E_CHARGE) when the charge failed - a constant fallback replaces the error.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// The factory is lazy, so an expensive fallback is only built when the pipeline actually failed.
    /// <code><![CDATA[
    /// var receiptId = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .BindErrValue(static () => OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok(receiptId) when the fallback produced a payment,
    /// // Err(E_CHARGE) when the charge failed - the factory runs only on the failure track.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <c>Match</c> is the total exit of a result: it handles both tracks, so nothing is ever left behind. This
    /// is the only exit production code should use.
    /// <code><![CDATA[
    /// var text = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .Match(
    ///         ok: static payment => $"captured {payment.Id}",
    ///         err: static error => $"failed {error.Code}");
    ///
    /// // "captured pay-ord-1001" for Ok(payment),
    /// // "failed E_CHARGE" for Err(E_CHARGE) - Match is the production exit.
    /// ]]></code>
    /// </remarks>
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
    /// <exception cref="InvalidOperationException">The result is an error or an uninitialized default.</exception>
    /// <remarks>Intended for tests and boundaries only; production code should use <see cref="Match{TOk,TErr,T}"/>.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE");
    /// var receipt = charged.OrThrow();
    ///
    /// // Ok(payment) yields the payment,
    /// // Err(E_CHARGE) throws - OrThrow is for tests and boundaries only.
    /// ]]></code>
    /// </remarks>
    public static TOk OrThrow<TOk, TErr>(this Result<TOk, TErr> result)
    {
        if (result.TryGetOk(out var value))
        {
            return value;
        }

        result.ThrowIfUninitialized();
        throw new InvalidOperationException($"Expected Ok but was {result}.");
    }

    /// <summary>
    /// Returns the success value or throws when the result is a failure.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">The result is an error or an uninitialized default.</exception>
    /// <remarks>This is the explicitly named form of <see cref="OrThrow{TOk,TErr}"/>.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE");
    /// var receipt = charged.OkOrThrow();
    ///
    /// // Ok(payment) yields the payment,
    /// // Err(E_CHARGE) throws - OkOrThrow is for tests and boundaries only.
    /// ]]></code>
    /// </remarks>
    public static TOk OkOrThrow<TOk, TErr>(this Result<TOk, TErr> result) => result.OrThrow();

    /// <summary>
    /// Returns the error value or throws when the result is a success.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns>The error value.</returns>
    /// <exception cref="InvalidOperationException">The result is a success or an uninitialized default.</exception>
    /// <remarks>Intended for tests and boundaries only; production code should use <see cref="Match{TOk,TErr,T}"/>.
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE");
    /// var error = charged.ErrOrThrow();
    ///
    /// // Err(E_CHARGE) yields the error,
    /// // Ok(payment) throws - ErrOrThrow is for tests and boundaries only.
    /// ]]></code>
    /// </remarks>
    public static TErr ErrOrThrow<TOk, TErr>(this Result<TOk, TErr> result) => result.TryGetErr(out var error)
        ? error
        : throw new InvalidOperationException($"Expected Err but was {result}.");
}
