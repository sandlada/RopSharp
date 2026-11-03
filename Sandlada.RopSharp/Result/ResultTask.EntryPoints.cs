using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>Static aggregation entry points for <see cref="ResultTask{TOk,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var placed = await ResultTask.Zip(
///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample), "E_ORDER"),
///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.Total), "E_AMOUNT"),
///     static (order, amount) => $"{order.Id}:{amount}");
///
/// // Ok("ord-1001:44") when both computations succeed,
/// // Err(E_ORDER) with the first error - Zip short-circuits on the error track.
/// ]]></code>
/// </remarks>
[SuppressMessage(
    "ApiDesign",
    "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "The 2- and 3-input Zip overloads differ by arity; the optional token follows the ct-last convention.")]
public static class ResultTask
{
    /// <summary>Wraps a completed result in a computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await ResultTask.From(
    ///         Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the lifted result is Ok,
    /// // Err(E_CHARGE) when it is Err - From only wraps a completed value.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr> From<TOk, TErr>(Result<TOk, TErr> result) => ResultTask<TOk, TErr>.From(result);

    /// <summary>Adopts a task that already carries a result.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// Task<Result<Payment, RopError>> charge = Task.FromResult(
    ///     Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var receiptId = await ResultTask.From(charge)
    ///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the adopted task completes with Ok,
    /// // Err(E_CHARGE) when it completes with Err.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr> From<TOk, TErr>(Task<Result<TOk, TErr>> task) => ResultTask<TOk, TErr>.From(task);

    /// <summary>Awaits two computations and merges successes, returning the first error in argument order.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await ResultTask.Zip(
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample), "E_ORDER"),
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.Total), "E_AMOUNT"),
    ///     static (order, amount) => $"{order.Id}:{amount}");
    ///
    /// // Ok("ord-1001:44") when both succeed,
    /// // Err(E_ORDER) or Err(E_AMOUNT) with the first failure - the merger never runs after Err.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> Zip<T1, T2, TOut, TErr>(
        ResultTask<T1, TErr> first,
        ResultTask<T2, TErr> second,
        Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        cancellationToken.ThrowIfCancellationRequested();

        return ResultTask<TOut, TErr>.From(
                ResultTaskCore.CombineAsync(first.TaskUnsafe, second.TaskUnsafe, merger, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits three computations and merges successes, returning the first error in argument order.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = await ResultTask.Zip(
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample), "E_ORDER"),
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.Total), "E_AMOUNT"),
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.ItemCount), "E_COUNT"),
    ///     static (order, amount, count) => $"{order.Id}:{amount}:{count}");
    ///
    /// // Ok("ord-1001:44:3") when all three succeed,
    /// // Err(E_ORDER) with the first failure in argument order.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOut, TErr> Zip<T1, T2, T3, TOut, TErr>(
        ResultTask<T1, TErr> first,
        ResultTask<T2, TErr> second,
        ResultTask<T3, TErr> third,
        Func<T1, T2, T3, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        third.ThrowIfUninitialized();
        cancellationToken.ThrowIfCancellationRequested();

        return ResultTask<TOut, TErr>.From(
                ResultTaskCore.CombineAsync(
                    first.TaskUnsafe,
                    second.TaskUnsafe,
                    third.TaskUnsafe,
                    merger,
                    cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits two computations and pairs their success values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var pair = await ResultTask.Zip(
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample), "E_ORDER"),
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.Total), "E_AMOUNT"));
    ///
    /// // Ok((order, 44)) when both succeed,
    /// // Err(E_ORDER) with the first failure - the tuple merger is built in.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<(T1, T2), TErr> Zip<T1, T2, TErr>(
        ResultTask<T1, TErr> first,
        ResultTask<T2, TErr> second,
        CancellationToken cancellationToken = default) =>
        Zip(first, second, static (a, b) => (a, b), cancellationToken);

    /// <summary>Awaits three computations and tuples their success values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var triple = await ResultTask.Zip(
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample), "E_ORDER"),
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.Total), "E_AMOUNT"),
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.ItemCount), "E_COUNT"));
    ///
    /// // Ok((order, 44, 3)) when all three succeed,
    /// // Err(E_ORDER) with the first failure.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<(T1, T2, T3), TErr> Zip<T1, T2, T3, TErr>(
        ResultTask<T1, TErr> first,
        ResultTask<T2, TErr> second,
        ResultTask<T3, TErr> third,
        CancellationToken cancellationToken = default) =>
        Zip(first, second, third, static (a, b, c) => (a, b, c), cancellationToken);

    /// <summary>Awaits a sequence of computations in order, short-circuiting on the first error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var lines = await ResultTask.Sequence(new[]
    /// {
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.Lines[0]), "E_LINE"),
    ///     Result.TryAsync(() => Task.FromResult(OrderFixtures.Sample.Lines[1]), "E_LINE"),
    /// });
    ///
    /// // Ok([line0, line1]) when every element is Ok,
    /// // Err(E_LINE) on the first failure - Sequence short-circuits.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        IEnumerable<ResultTask<T, TErr>> computations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(computations);
        cancellationToken.ThrowIfCancellationRequested();

        return ResultTask<IReadOnlyList<T>, TErr>.From(ResultTaskCore.SequenceAsync(computations, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Maps each element to a computation and awaits in order, short-circuiting on the first error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var skus = await ResultTask.Traverse(
    ///     OrderFixtures.Sample.Lines,
    ///     static (line, _) => Task.FromResult(Result.Ok<OrderLine, RopError>(line)));
    ///
    /// // Ok([line0, line1]) when every mapper returns Ok,
    /// // Err(E_LINE) on the first mapper failure.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<IReadOnlyList<TOut>, TErr> Traverse<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, CancellationToken, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        return ResultTask<IReadOnlyList<TOut>, TErr>.From(
                ResultTaskCore.TraverseAsync(source, mapper, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits every computation and splits successes from errors without failing.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var split = await ResultTask.Partition(new[]
    /// {
    ///     Result.TryAsync(() => OrderFixtures.ChargeAsync(OrderFixtures.Sample), "E_CHARGE"),
    ///     Result.TryAsync(() => OrderFixtures.ChargeAsync(OrderFixtures.Declined), "E_CHARGE"),
    /// });
    ///
    /// // Ok(([payment], [E_CHARGE])) when one charge succeeds and one fails,
    /// // Err(E_CHARGE) is collected without failing the whole Partition call.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<(IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errs), TErr> Partition<T, TErr>(
        IEnumerable<ResultTask<T, TErr>> computations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(computations);
        cancellationToken.ThrowIfCancellationRequested();

        return ResultTask<(IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errs), TErr>.From(
                ResultTaskCore.PartitionAsync(computations, cancellationToken))
            .WithCancellation(cancellationToken);
    }
}
