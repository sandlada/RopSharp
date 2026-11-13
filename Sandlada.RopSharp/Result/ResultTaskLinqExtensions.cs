namespace Sandlada.RopSharp;

/// <summary>LINQ query-syntax support for <see cref="ResultTask{TOk,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var receiptId = await Result.TryAsync(
///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
///         "E_CHARGE")
///     .Select(static payment => payment.Id);
///
/// // Ok("pay-ord-1001") when the charge succeeds,
/// // Err(E_CHARGE) when it failed - query syntax desugars to Select and SelectMany.
/// ]]></code>
/// </remarks>
public static class ResultTaskLinqExtensions
{
    /// <summary>Projects a success value after the computation completes.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .Select(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the charge succeeded,
    /// // Err(E_CHARGE) when it failed - Select is the LINQ spelling of MapOk.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> Select<TOk, TOut, TErr>(
        this ResultTask<TOk, TErr> computation,
        Func<TOk, TOut> selector) => computation.MapOk(selector);

    /// <summary>Binds the success track to another result-producing step.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var shipment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .SelectMany(static payment =>
    ///         Result.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, payment), "E_SHIP"));
    ///
    /// // Ok(shipment) when both steps succeed,
    /// // Err(E_CHARGE) or Err(E_SHIP) - SelectMany is the LINQ spelling of BindOk.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> SelectMany<TOk, TOut, TErr>(
        this ResultTask<TOk, TErr> computation,
        Func<TOk, Result<TOut, TErr>> binder) => computation.BindOk(binder);

    /// <summary>Binds and projects both success values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .SelectMany(
    ///         static payment => Result.Ok<Shipment, RopError>(OrderFixtures.Ship(OrderFixtures.Sample, payment)),
    ///         static (payment, shipment) => $"{payment.Id}:{shipment.TrackingNumber}");
    ///
    /// // Ok("pay-ord-1001:TRK-...") when both steps succeed,
    /// // Err(E_CHARGE) or Err(E_SHIP) on the first failure - the result selector sees both success values.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> SelectMany<TOk, TCol, TOut, TErr>(
        this ResultTask<TOk, TErr> computation,
        Func<TOk, Result<TCol, TErr>> collectionSelector,
        Func<TOk, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return computation.BindOk(first => collectionSelector(first).MapOk(second => resultSelector(first, second)));
    }

    /// <summary>Binds the success track to another asynchronous result computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var shipment = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .SelectMany(static payment =>
    ///         Result.TryAsync(() => OrderFixtures.ShipAsync(OrderFixtures.Sample, payment), "E_SHIP"));
    ///
    /// // Ok(shipment) when both steps succeed,
    /// // Err(E_CHARGE) or Err(E_SHIP) with the first failure.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> SelectMany<TOk, TOut, TErr>(
        this ResultTask<TOk, TErr> computation,
        Func<TOk, ResultTask<TOut, TErr>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        computation.ThrowIfUninitialized();

        return computation.BindOkAsync(async (first, ct) =>
        {
            var next = binder(first);
            return await next.WithCancellation(ct);
        });
    }

    /// <summary>Binds and projects success values from an asynchronous result computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .SelectMany(
    ///         static payment => Result.TryAsync(() => OrderFixtures.ShipAsync(OrderFixtures.Sample, payment), "E_SHIP"),
    ///         static (payment, shipment) => $"{payment.Id}:{shipment.TrackingNumber}");
    ///
    /// // Ok("pay-ord-1001:TRK-...") when both steps succeed,
    /// // Err(E_CHARGE) or Err(E_SHIP) on the first failure.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> SelectMany<TOk, TCol, TOut, TErr>(
        this ResultTask<TOk, TErr> computation,
        Func<TOk, ResultTask<TCol, TErr>> collectionSelector,
        Func<TOk, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        computation.ThrowIfUninitialized();

        return computation.BindOkAsync(async (first, ct) =>
        {
            var next = collectionSelector(first);
            var colResult = await next.WithCancellation(ct);
            return colResult.MapOk(second => resultSelector(first, second));
        });
    }
}
