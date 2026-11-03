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

        return ResultTask<TOut, TErr>.From(ResultTaskCore.MapOk<TOk, TOut, TErr>(_task!, mapper, _cancellationToken));
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

        return ResultTask<TOut, TErr>.From(
            ResultTaskCore.MapOkAsync<TOk, TOut, TErr>(_task!, mapper, _cancellationToken));
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

        return ResultTask<TOut, TErr>.From(ResultTaskCore.BindOk<TOk, TOut, TErr>(_task!, binder, _cancellationToken));
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

        return ResultTask<TOut, TErr>.From(
            ResultTaskCore.BindOkAsync<TOk, TOut, TErr>(_task!, binder, _cancellationToken));
    }
}
