namespace Sandlada.RopSharp;

/// <summary>
/// LINQ query-syntax support for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// Only <c>select</c> and <c>from</c> are supported. <c>where</c> is intentionally absent because a result query
/// has no error value to produce for a failing predicate; use <see cref="ResultExtensions.Ensure{TOk,TErr}(Result{TOk,TErr},Func{TOk,bool},TErr)"/> instead.
/// <code><![CDATA[
/// var receiptId = Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
///     .Select(static payment => payment.Id);
///
/// // Ok("pay-ord-1001") on the success track,
/// // Err(E_CHARGE) on the failure track - Select is the query-syntax form of MapOk.
/// ]]></code>
/// </remarks>
public static class ResultLinqExtensions
{
    /// <summary>
    /// Projects a success value. Query-syntax equivalent of <c>select</c>.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TOut">The projected type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="selector">The projection.</param>
    /// <returns>The projected success, or the original error.</returns>
    /// <remarks>
    /// <c>Select</c> is the query-syntax spelling of <see cref="ResultExtensions.MapOk{TOk,TOut,TErr}"/>. It exists
    /// so that a result can be used inside a LINQ query expression.
    /// <code><![CDATA[
    /// var receiptId = Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .Select(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") on the success track,
    /// // Err(E_CHARGE) on the failure track - Select is the query-syntax form of MapOk.
    /// ]]></code>
    /// </remarks>
    public static Result<TOut, TErr> Select<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, TOut> selector) => result.MapOk(selector);

    /// <summary>
    /// Binds a success value to another result. Query-syntax equivalent of a second <c>from</c>.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The bound success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="binder">The binding function.</param>
    /// <returns>The bound result, or the original error.</returns>
    /// <remarks>
    /// <c>SelectMany</c> is the query-syntax spelling of <see cref="ResultExtensions.BindOk{TOk,TOut,TErr}"/>: it
    /// expands the success track into another result.
    /// <code><![CDATA[
    /// var receiptId = Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .SelectMany(static payment => Result.Ok(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the charge and the binding succeeded,
    /// // Err(E_CHARGE) when the charge failed - the binder never runs on the failure track.
    /// ]]></code>
    /// </remarks>
    public static Result<TOut, TErr> SelectMany<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<TOut, TErr>> binder) => result.BindOk(binder);

    /// <summary>
    /// Binds a success value and projects the pair. Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TCol">The intermediate success type.</typeparam>
    /// <typeparam name="TOut">The projected type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="collectionSelector">The intermediate binding.</param>
    /// <param name="resultSelector">Projects both success values.</param>
    /// <returns>The projected success, or the first error.</returns>
    /// <remarks>
    /// This overload carries both success values into the projection, which is what a query with two
    /// <c>from</c> clauses needs.
    /// <code><![CDATA[
    /// var summary = Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .SelectMany(
    ///         static payment => Result.Ok(payment.Amount),
    ///         static (payment, amount) => $"{payment.Id}:{amount}");
    ///
    /// // Ok("pay-ord-1001:44") when both steps succeeded,
    /// // Err(E_CHARGE) when the charge failed - the first error wins.
    /// ]]></code>
    /// </remarks>
    public static Result<TOut, TErr> SelectMany<TOk, TCol, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<TCol, TErr>> collectionSelector,
        Func<TOk, TCol, TOut> resultSelector) =>
        result.BindOk(first => collectionSelector(first).MapOk(second => resultSelector(first, second)));
}
