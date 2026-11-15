namespace Sandlada.RopSharp;

public readonly partial struct ResultTask<TOk, TErr>
{
    /// <summary>Maps the success value when the computation completes successfully.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the mapper never runs on the error track.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOut, TErr> MapOk<TOut>(Func<TOk, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("MapOk");

        return new(ResultTaskCore.MapOk<TOk, TOut, TErr>(_task!, mapper, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Maps the success value with an asynchronous mapper that receives the computation token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the asynchronous mapper never runs on the error track.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOut, TErr> MapOkAsync<TOut>(Func<TOk, CancellationToken, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();

        return new(
            ResultTaskCore.MapOkAsync<TOk, TOut, TErr>(_task!, mapper, _cancellationToken),
            TagReady,
            _cancellationToken);
    }

    /// <summary>Chains another result-producing step on the success track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var shipment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .BindOk(static payment =>
    ///         Result.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, payment), "E_SHIP"));
    ///
    /// // Ok(shipment) when both steps succeed,
    /// // Err(E_CHARGE) or Err(E_SHIP) with the first failure - BindOk short-circuits on Err.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOut, TErr> BindOk<TOut>(Func<TOk, Result<TOut, TErr>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("BindOk");

        return new(ResultTaskCore.BindOk<TOk, TOut, TErr>(_task!, binder, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Chains an asynchronous binder on the success track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var shipment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .BindOkAsync(static (payment, _) =>
    ///         Task.FromResult(Result.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, payment), "E_SHIP")));
    ///
    /// // Ok(shipment) when both asynchronous steps succeed,
    /// // Err(E_CHARGE) or Err(E_SHIP) with the first failure.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOut, TErr> BindOkAsync<TOut>(
        Func<TOk, CancellationToken, Task<Result<TOut, TErr>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        ThrowIfUninitialized();

        return new(
            ResultTaskCore.BindOkAsync<TOk, TOut, TErr>(_task!, binder, _cancellationToken),
            TagReady,
            _cancellationToken);
    }

    /// <summary>Replaces the success value with a constant value, leaving an error untouched.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receipt = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .MapOkValue(Unit.Value);
    ///
    /// // Ok(()) when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the replacement value is never null.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOut, TErr> MapOkValue<TOut>(TOut value)
    {
        ArgumentGuards.ThrowIfNull(value, nameof(value));
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("MapOkValue");

        return new(ResultTaskCore.MapOk<TOk, TOut, TErr>(_task!, _ => value, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when a predicate does not hold.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .Ensure(static payment => payment.Amount > 0m, RopError.Create("E_AMOUNT", "amount must be positive"));
    ///
    /// // Ok(payment) when positive,
    /// // Err(E_AMOUNT) when non-positive - the predicate never runs on Err.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> Ensure(Func<TOk, bool> predicate, TErr error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        ThrowIfUninitialized();

        return new(ResultTaskCore.Ensure(_task!, predicate, error, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when a predicate does not hold, building the error lazily.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .Ensure(
    ///         static payment => payment.Amount > 0m,
    ///         static payment => RopError.Create("E_AMOUNT", $"invalid {payment.Amount}"));
    ///
    /// // Ok(payment) when positive,
    /// // Err(E_AMOUNT) when the factory runs on failure.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> Ensure(Func<TOk, bool> predicate, Func<TOk, TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        ThrowIfUninitialized();

        return new(ResultTaskCore.Ensure(_task!, predicate, errorFactory, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when an asynchronous predicate does not hold.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .EnsureAsync(
    ///         static (payment, _) => Task.FromResult(payment.Amount > 0m),
    ///         RopError.Create("E_AMOUNT", "amount must be positive"));
    ///
    /// // Ok(payment) when the asynchronous invariant holds,
    /// // Err(E_AMOUNT) when it returns false.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> EnsureAsync(Func<TOk, CancellationToken, Task<bool>> predicate, TErr error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        ThrowIfUninitialized();

        return new(ResultTaskCore.EnsureAsync(_task!, predicate, error, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when an asynchronous predicate does not hold, building the error asynchronously.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .EnsureAsync(
    ///         static (payment, _) => Task.FromResult(payment.Amount > 0m),
    ///         static (payment, _) => Task.FromResult(RopError.Create("E_AMOUNT", $"invalid {payment.Amount}")));
    ///
    /// // Ok(payment) when the asynchronous invariant holds,
    /// // Err(E_AMOUNT) when the asynchronous error factory runs.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> EnsureAsync(
        Func<TOk, CancellationToken, Task<bool>> predicate,
        Func<TOk, CancellationToken, Task<TErr>> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        ThrowIfUninitialized();

        return new(ResultTaskCore.EnsureAsync(_task!, predicate, errorFactory, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when a condition holds.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .FailIf(static payment => payment.Amount <= 0m, RopError.Create("E_AMOUNT", "amount must be positive"));
    ///
    /// // Ok(payment) when the rejection predicate is false,
    /// // Err(E_AMOUNT) when it holds.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> FailIf(Func<TOk, bool> predicate, TErr error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        ThrowIfUninitialized();

        return new(ResultTaskCore.FailIf(_task!, predicate, error, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when a condition holds, building the error lazily.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .FailIf(
    ///         static payment => payment.Amount <= 0m,
    ///         static payment => RopError.Create("E_AMOUNT", $"invalid {payment.Amount}"));
    ///
    /// // Ok(payment) when the rejection predicate is false,
    /// // Err(E_AMOUNT) when it holds.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> FailIf(Func<TOk, bool> predicate, Func<TOk, TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        ThrowIfUninitialized();

        return new(ResultTaskCore.FailIf(_task!, predicate, errorFactory, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when an asynchronous condition holds.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .FailIfAsync(
    ///         static (payment, _) => Task.FromResult(payment.Amount <= 0m),
    ///         RopError.Create("E_AMOUNT", "amount must be positive"));
    ///
    /// // Ok(payment) when the asynchronous predicate returns false,
    /// // Err(E_AMOUNT) when it returns true.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> FailIfAsync(Func<TOk, CancellationToken, Task<bool>> predicate, TErr error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        ThrowIfUninitialized();

        return new(ResultTaskCore.FailIfAsync(_task!, predicate, error, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Turns a success into a failure when an asynchronous condition holds, building the error asynchronously.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .FailIfAsync(
    ///         static (payment, _) => Task.FromResult(payment.Amount <= 0m),
    ///         static (payment, _) => Task.FromResult(RopError.Create("E_AMOUNT", $"invalid {payment.Amount}")));
    ///
    /// // Ok(payment) when the asynchronous predicate returns false,
    /// // Err(E_AMOUNT) when it returns true.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> FailIfAsync(
        Func<TOk, CancellationToken, Task<bool>> predicate,
        Func<TOk, CancellationToken, Task<TErr>> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);
        ThrowIfUninitialized();

        return new(ResultTaskCore.FailIfAsync(_task!, predicate, errorFactory, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Runs a validating <see cref="Unit"/> check on the success track without altering the payload.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .ThenEnsure(static payment => payment.Amount > 0m
    ///         ? Result.Ok<Unit, RopError>(Unit.Value)
    ///         : Result.Err<Unit, RopError>(RopError.Create("E_AMOUNT", "amount must be positive")));
    ///
    /// // Ok(payment) when the check succeeds,
    /// // Err(E_AMOUNT) when it fails.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> ThenEnsure(Func<TOk, Result<Unit, TErr>> ensure)
    {
        ArgumentNullException.ThrowIfNull(ensure);
        ThrowIfUninitialized();

        return new(ResultTaskCore.ThenEnsure(_task!, ensure, _cancellationToken), TagReady, _cancellationToken);
    }

    /// <summary>Runs an asynchronous validating <see cref="Unit"/> check on the success track without altering the payload.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var checkedPayment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .ThenEnsureAsync(static (payment, _) => Task.FromResult(payment.Amount > 0m
    ///         ? Result.Ok<Unit, RopError>(Unit.Value)
    ///         : Result.Err<Unit, RopError>(RopError.Create("E_AMOUNT", "amount must be positive"))));
    ///
    /// // Ok(payment) when the asynchronous check succeeds,
    /// // Err(E_AMOUNT) when it fails.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> ThenEnsureAsync(Func<TOk, CancellationToken, Task<Result<Unit, TErr>>> ensure)
    {
        ArgumentNullException.ThrowIfNull(ensure);
        ThrowIfUninitialized();

        return new(ResultTaskCore.ThenEnsureAsync(_task!, ensure, _cancellationToken), TagReady, _cancellationToken);
    }
}
