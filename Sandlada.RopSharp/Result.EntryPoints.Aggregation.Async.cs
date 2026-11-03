using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// Asynchronous aggregation helpers for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// <para>
/// All inputs are awaited before the result is decided, so faults from every supplied task are observed even when
/// an earlier result already failed. The merger is never invoked when any input failed. Error ordering and
/// short-circuit versus aggregating semantics match the synchronous <see cref="Result"/> counterparts.
/// </para>
/// <code><![CDATA[
/// var placed = await Result.CombineAsync(
///         Task.FromResult(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
///         Task.FromResult(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)),
///         static (order, amount) => $"{order.Id}:{amount}");
///
/// // Ok("ord-1001:44") when both tasks produced Ok,
/// // Err(E_ORDER) with the first error - every task is awaited before the result is decided.
/// ]]></code>
/// </remarks>
[SuppressMessage(
    "ApiDesign",
    "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "The 2- and 3-input overloads differ by arity, so existing calls cannot become ambiguous; the optional token follows the library-wide ct-last convention.")]
public static partial class Result
{
    /// <summary>
    /// Awaits two results and combines them with a merger, returning the first error in argument order.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TOut">The merged success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result task.</param>
    /// <param name="second">The second result task.</param>
    /// <param name="merger">Combines both success values. Evaluated only when both are <c>Ok</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(merger(a, b))</c> or the first <c>Err</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// Both tasks are awaited before the result is decided, so a fault in the second task is observed even when the
    /// first one already failed.
    /// <code><![CDATA[
    /// var placed = await Result.CombineAsync(
    ///         Task.FromResult(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///         Task.FromResult(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///         static (order, amount) => $"{order.Id}:{amount}");
    ///
    /// // Ok("ord-1001:44") when both tasks produced Ok,
    /// // Err(E_ORDER) with the first error - the merger never runs after a failure.
    /// ]]></code>
    /// </remarks>
    public static async Task<Result<TOut, TErr>> CombineAsync<T1, T2, TOut, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(merger);
        cancellationToken.ThrowIfCancellationRequested();

        await Task.WhenAll(first, second).WaitAsync(cancellationToken).ConfigureAwait(false);
        var a = await first.ConfigureAwait(false);
        var b = await second.ConfigureAwait(false);
        return Combine(a, b, merger);
    }

    /// <summary>
    /// Awaits three results and combines them with a merger, returning the first error in argument order.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="T3">The third success type.</typeparam>
    /// <typeparam name="TOut">The merged success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result task.</param>
    /// <param name="second">The second result task.</param>
    /// <param name="third">The third result task.</param>
    /// <param name="merger">Combines all success values. Evaluated only when all are <c>Ok</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(merger(a, b, c))</c> or the first <c>Err</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// The same short-circuiting semantics as the two-input overload, extended to three tasks.
    /// <code><![CDATA[
    /// var placed = await Result.CombineAsync(
    ///         Task.FromResult(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///         Task.FromResult(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///         Task.FromResult(Result.Ok<int, RopError>(OrderFixtures.Sample.ItemCount)),
    ///         static (order, amount, count) => $"{order.Id}:{amount}:{count}");
    ///
    /// // Ok("ord-1001:44:3") when all three tasks produced Ok,
    /// // Err(E_ORDER) with the first error - the merger never runs after a failure.
    /// ]]></code>
    /// </remarks>
    public static async Task<Result<TOut, TErr>> CombineAsync<T1, T2, T3, TOut, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Task<Result<T3, TErr>> third,
        Func<T1, T2, T3, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(third);
        ArgumentNullException.ThrowIfNull(merger);
        cancellationToken.ThrowIfCancellationRequested();

        await Task.WhenAll(first, second, third).WaitAsync(cancellationToken).ConfigureAwait(false);
        var a = await first.ConfigureAwait(false);
        var b = await second.ConfigureAwait(false);
        var c = await third.ConfigureAwait(false);
        return Combine(a, b, c, merger);
    }

    /// <summary>
    /// Awaits two results and pairs them into a tuple, returning the first error in argument order.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result task.</param>
    /// <param name="second">The second result task.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok((a, b))</c> or the first <c>Err</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// <c>ZipAsync</c> is <see cref="CombineAsync{T1,T2,TOut,TErr}"/> with a tuple merger.
    /// <code><![CDATA[
    /// var pair = await Result.ZipAsync(
    ///     Task.FromResult(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     Task.FromResult(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)));
    ///
    /// // Ok((order, 44)) when both tasks produced Ok,
    /// // Err(E_ORDER) with the first error - ZipAsync is CombineAsync with a tuple merger.
    /// ]]></code>
    /// </remarks>
    public static Task<Result<(T1, T2), TErr>> ZipAsync<T1, T2, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        CancellationToken cancellationToken = default) =>
        CombineAsync(first, second, static (a, b) => (a, b), cancellationToken);

    /// <summary>
    /// Awaits three results and pairs them into a tuple, returning the first error in argument order.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="T3">The third success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result task.</param>
    /// <param name="second">The second result task.</param>
    /// <param name="third">The third result task.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok((a, b, c))</c> or the first <c>Err</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// <c>ZipAsync</c> is <see cref="CombineAsync{T1,T2,T3,TOut,TErr}"/> with a tuple merger.
    /// <code><![CDATA[
    /// var triple = await Result.ZipAsync(
    ///     Task.FromResult(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     Task.FromResult(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///     Task.FromResult(Result.Ok<int, RopError>(OrderFixtures.Sample.ItemCount)));
    ///
    /// // Ok((order, 44, 3)) when all three tasks produced Ok,
    /// // Err(E_ORDER) with the first error - ZipAsync is CombineAsync with a tuple merger.
    /// ]]></code>
    /// </remarks>
    public static Task<Result<(T1, T2, T3), TErr>> ZipAsync<T1, T2, T3, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Task<Result<T3, TErr>> third,
        CancellationToken cancellationToken = default) =>
        CombineAsync(first, second, third, static (a, b, c) => (a, b, c), cancellationToken);

    /// <summary>
    /// Awaits a sequence of results and splits them into successes and errors.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="tasks">The result tasks to await. Enumerated exactly once.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>All success values and all errors, preserving source order.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before completion.</exception>
    /// <exception cref="InvalidOperationException">The sequence contained a null task.</exception>
    /// <remarks>
    /// Null tasks are rejected with <see cref="InvalidOperationException"/>, matching
    /// <see cref="Validation.SequenceAsync{T,TErr}(IEnumerable{Task{Result{T,TErr}}},CancellationToken)"/> and
    /// <see cref="Validation.TraverseAsync{T,TOut,TErr}(IEnumerable{T},Func{T,CancellationToken,Task{Result{TOut,TErr}}},CancellationToken)"/>,
    /// so every aggregation helper reports the same programming error.
    /// <code><![CDATA[
    /// var (oks, errs) = await Result.PartitionAsync(
    ///     new[]
    ///     {
    ///         Task.FromResult(Result.Ok<string, RopError>("SKU-1")),
    ///         Task.FromResult(Result.Err<string, RopError>(RopError.Create("E_LINE", "the line is invalid"))),
    ///     });
    ///
    /// // Ok(["SKU-1"]) is the success half,
    /// // Err([E_LINE]) is the error half - PartitionAsync never fails.
    /// ]]></code>
    /// </remarks>
    public static async Task<(IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errs)> PartitionAsync<T, TErr>(
        IEnumerable<Task<Result<T, TErr>>> tasks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        cancellationToken.ThrowIfCancellationRequested();

        var pending = CreateList<Task<Result<T, TErr>>, Task<Result<T, TErr>>>(tasks);
        foreach (var task in tasks)
        {
            if (task is null)
            {
                throw new InvalidOperationException("The sequence contained a null task.");
            }

            pending.Add(task);
        }

        var results = await Task.WhenAll(pending).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Partition(results);
    }
}
