namespace Sandlada.RopSharp;

/// <summary>
/// Bridges synchronous <see cref="Result{TOk,TErr}"/> values into <see cref="ResultTask{TOk,TErr}"/> pipelines.
/// </summary>
/// <remarks>
/// <code><![CDATA[
/// var receiptId = await Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
///     .ToResultTask()
///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
///
/// // Ok("pay-ord-1001") when the lifted result is Ok,
/// // Err(E_CHARGE) when it is Err - ToResultTask wraps a completed result.
/// ]]></code>
/// </remarks>
public static class ResultTaskBridgeExtensions
{
    /// <summary>Wraps a result as a completed computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .ToResultTask()
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the source result is Ok,
    /// // Err(E_CHARGE) when it is Err.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr> ToResultTask<TOk, TErr>(this Result<TOk, TErr> result) => ResultTask.From(result);

    /// <summary>Adopts a task that already carries a result.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// Task<Result<Payment, RopError>> charge = Task.FromResult(
    ///     Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var receiptId = await charge.ToResultTask()
    ///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the task completes with Ok,
    /// // Err(E_CHARGE) when it completes with Err.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr> ToResultTask<TOk, TErr>(this Task<Result<TOk, TErr>> task) => ResultTask.From(task);

    /// <summary>Maps the success value with an asynchronous mapper.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - the bridge delegates to MapOkAsync on a completed computation.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> MapOkAsync<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, CancellationToken, Task<TOut>> mapper) =>
        ResultTask.From(result).MapOkAsync(mapper);

    /// <summary>Maps the error with an asynchronous mapper.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .MapErrAsync(static (error, _) => Task.FromResult(error.WithCode("E_PAYMENT_DECLINED")))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001-declined") when the charge succeeds,
    /// // Err(E_PAYMENT_DECLINED) when it failed.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr2> MapErrAsync<TOk, TErr, TErr2>(
        this Result<TOk, TErr> result,
        Func<TErr, CancellationToken, Task<TErr2>> mapper) =>
        ResultTask.From(result).MapErrAsync(mapper);

    /// <summary>Chains an asynchronous binder on the success track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var shipment = await Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .BindOkAsync(static (payment, _) =>
    ///         Task.FromResult(Result.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, payment), "E_SHIP")));
    ///
    /// // Ok(shipment) when both steps succeed,
    /// // Err(E_CHARGE) or Err(E_SHIP) with the first failure.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> BindOkAsync<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, CancellationToken, Task<Result<TOut, TErr>>> binder) =>
        ResultTask.From(result).BindOkAsync(binder);

    /// <summary>Recovers on the failure track with an asynchronous binder.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = await Result.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE")
    ///     .BindErrAsync(static (_, _) =>
    ///         Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")));
    ///
    /// // Ok(payment) when the fallback succeeds,
    /// // Err(E_CHARGE) when both attempts fail.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr> BindErrAsync<TOk, TErr>(
        this Result<TOk, TErr> result,
        Func<TErr, CancellationToken, Task<Result<TOk, TErr>>> binder) =>
        ResultTask.From(result).BindErrAsync(binder);
}
