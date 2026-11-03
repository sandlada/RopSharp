namespace Sandlada.RopSharp;

public readonly partial struct ResultTask<TOk, TErr>
{
    /// <summary>Maps the error when the computation completes on the failure track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Declined),
    ///         "E_CHARGE")
    ///     .MapErr(static error => error.WithCode("E_PAYMENT_DECLINED"))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001-declined") when the charge succeeds,
    /// // Err(E_PAYMENT_DECLINED) when it failed - MapErr reshapes the error track only.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr2> MapErr<TErr2>(Func<TErr, TErr2> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();

        return ResultTask<TOk, TErr2>.From(ResultTaskCore.MapErr<TOk, TErr, TErr2>(_task!, mapper, _cancellationToken));
    }

    /// <summary>Maps the error with an asynchronous mapper that receives the computation token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Declined),
    ///         "E_CHARGE")
    ///     .MapErrAsync(static (error, _) => Task.FromResult(error.WithCode("E_PAYMENT_DECLINED")))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001-declined") when the charge succeeds,
    /// // Err(E_PAYMENT_DECLINED) when it failed - the asynchronous mapper runs only on Err.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr2> MapErrAsync<TErr2>(Func<TErr, CancellationToken, Task<TErr2>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();

        return ResultTask<TOk, TErr2>.From(
            ResultTaskCore.MapErrAsync<TOk, TErr, TErr2>(_task!, mapper, _cancellationToken));
    }

    /// <summary>Recovers on the failure track with another result-producing step.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Declined),
    ///         "E_CHARGE")
    ///     .BindErr(static _ =>
    ///         Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    ///
    /// // Ok(payment) when the fallback charge succeeds,
    /// // Err(E_CHARGE) when both attempts fail - BindErr may fail again.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> BindErr(Func<TErr, Result<TOk, TErr>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        ThrowIfUninitialized();

        return From(ResultTaskCore.BindErr<TOk, TErr>(_task!, binder, _cancellationToken));
    }

    /// <summary>Recovers on the failure track with an asynchronous binder.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Declined),
    ///         "E_CHARGE")
    ///     .BindErrAsync(static (_, _) =>
    ///         Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")));
    ///
    /// // Ok(payment) when the asynchronous fallback succeeds,
    /// // Err(E_CHARGE) when both attempts fail.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> BindErrAsync(Func<TErr, CancellationToken, Task<Result<TOk, TErr>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        ThrowIfUninitialized();

        return From(ResultTaskCore.BindErrAsync<TOk, TErr>(_task!, binder, _cancellationToken));
    }

    /// <summary>Runs a side effect on the success track without changing the result.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .TeeOk(static captured => Console.WriteLine($"captured {captured.Id}"))
    ///     .MapOk(static captured => captured.Id);
    ///
    /// // Ok("pay-ord-1001") after logging the capture,
    /// // Err(E_CHARGE) when the charge failed - TeeOk never runs on the error track.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> TeeOk(Action<TOk> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();

        return From(ResultTaskCore.TeeOk<TOk, TErr>(_task!, action, _cancellationToken));
    }

    /// <summary>Runs an asynchronous side effect on the success track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .TeeOkAsync(static (captured, _) => Task.Run(() => Console.WriteLine($"captured {captured.Id}")))
    ///     .MapOk(static captured => captured.Id);
    ///
    /// // Ok("pay-ord-1001") after the asynchronous log,
    /// // Err(E_CHARGE) when the charge failed.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> TeeOkAsync(Func<TOk, CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();

        return From(ResultTaskCore.TeeOkAsync<TOk, TErr>(_task!, action, _cancellationToken));
    }

    /// <summary>Runs a side effect on the failure track without changing the result.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Declined),
    ///         "E_CHARGE")
    ///     .TeeErr(static error => Console.WriteLine(error.Code))
    ///     .BindErr(static _ =>
    ///         Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    ///
    /// // Ok(payment) when the fallback succeeds after logging the decline,
    /// // Err(E_CHARGE) when recovery fails - TeeErr never runs on Ok.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> TeeErr(Action<TErr> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();

        return From(ResultTaskCore.TeeErr<TOk, TErr>(_task!, action, _cancellationToken));
    }

    /// <summary>Runs an asynchronous side effect on the failure track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Declined),
    ///         "E_CHARGE")
    ///     .TeeErrAsync(static (error, _) => Task.Run(() => Console.WriteLine(error.Code)))
    ///     .BindErr(static _ =>
    ///         Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    ///
    /// // Ok(payment) when the fallback succeeds after the asynchronous log,
    /// // Err(E_CHARGE) when recovery fails.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> TeeErrAsync(Func<TErr, CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfUninitialized();

        return From(ResultTaskCore.TeeErrAsync<TOk, TErr>(_task!, action, _cancellationToken));
    }

    /// <summary>Runs the selected asynchronous action for the active track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .SwitchAsync(
    ///         static (payment, _) => OrderFixtures.ShipAsync(OrderFixtures.Sample, payment),
    ///         static (_, _) => Task.CompletedTask);
    ///
    /// // Ok(shipment) when the charge succeeded and ShipAsync runs,
    /// // Err(E_CHARGE) when the charge failed - SwitchAsync consumes the result without returning it.
    /// ]]></code>
    /// </remarks>
    public Task SwitchAsync(
        Func<TOk, CancellationToken, Task> ok,
        Func<TErr, CancellationToken, Task> err)
    {
        ThrowIfUninitialized();
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);
        return ResultTaskCore.SwitchAsync(_task!, ok, err, _cancellationToken);
    }
}
