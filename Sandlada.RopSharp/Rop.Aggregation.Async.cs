using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// Asynchronous aggregation helpers for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// All inputs are awaited before the result is decided, so faults from every supplied task are observed even when
/// an earlier result already failed. The merger is never invoked when any input failed. Error ordering and
/// short-circuit versus aggregating semantics match the synchronous <see cref="Rop"/> counterparts.
/// </remarks>
[SuppressMessage(
    "ApiDesign",
    "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "The 2- and 3-input overloads differ by arity, so existing calls cannot become ambiguous; the optional token follows the library-wide ct-last convention.")]
public static partial class Rop
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
    public static Task<Result<(T1, T2, T3), TErr>> ZipAsync<T1, T2, T3, TErr>(
        Task<Result<T1, TErr>> first,
        Task<Result<T2, TErr>> second,
        Task<Result<T3, TErr>> third,
        CancellationToken cancellationToken = default) =>
        CombineAsync(first, second, third, static (a, b, c) => (a, b, c), cancellationToken);

    /// <summary>
    /// Awaits two results and combines them applicatively, collecting every error.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TOut">The merged success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result task.</param>
    /// <param name="second">The second result task.</param>
    /// <param name="merger">Combines both success values. Evaluated only when both are <c>Ok</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(merger(a, b))</c> or <c>Err</c> with all errors in argument order.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    public static async Task<Result<TOut, IReadOnlyList<TErr>>> ValidateAsync<T1, T2, TOut, TErr>(
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
        return Validate(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            merger);
    }

    /// <summary>
    /// Awaits three results and combines them applicatively, collecting every error.
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
    /// <returns><c>Ok(merger(a, b, c))</c> or <c>Err</c> with all errors in argument order.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    public static async Task<Result<TOut, IReadOnlyList<TErr>>> ValidateAsync<T1, T2, T3, TOut, TErr>(
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
        return Validate(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            await third.ConfigureAwait(false),
            merger);
    }

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
    /// <see cref="SequenceValidationAsync{T,TErr}"/> and <see cref="TraverseValidationAsync{T,TOut,TErr}(IEnumerable{T},Func{T,Task{Result{TOut,TErr}}},CancellationToken)"/>,
    /// so every aggregation helper reports the same programming error.
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

    /// <summary>
    /// Awaits a sequence of task-wrapped results and collects every error, awaiting every task.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="tasks">The result tasks to await. Enumerated exactly once.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// <c>Ok</c> with all success values in source order, or <c>Err</c> with all errors in source order (never
    /// empty). An empty source yields <c>Ok([])</c>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before the sequence was started or completed.</exception>
    /// <exception cref="InvalidOperationException">The sequence contained a null task.</exception>
    /// <remarks>
    /// Aggregating semantics require every input, so all tasks are awaited instead of short-circuiting. All faults are
    /// observed; when more than one task faults, the first fault wins. Use
    /// <see cref="Rop.SequenceAsync{T,TErr}"/> when only the first error matters.
    /// </remarks>
    public static async Task<Result<IReadOnlyList<T>, IReadOnlyList<TErr>>> SequenceValidationAsync<T, TErr>(
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

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return SequenceValidation(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Maps each element to a task-wrapped result and collects every error, awaiting every task.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TOut">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="source">The source elements. Enumerated exactly once.</param>
    /// <param name="mapper">Maps an element to a task-wrapped result.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// <c>Ok</c> with all success values in source order, or <c>Err</c> with all errors in source order (never
    /// empty). An empty source yields <c>Ok([])</c>.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the mapper was invoked for the next element (the mapper is then not called) or
    /// before the produced tasks completed.
    /// </exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// The mapper is invoked for every element because aggregation never short-circuits. Use
    /// <see cref="Rop.TraverseAsync{T,TOut,TErr}"/> when only the first error matters.
    /// </remarks>
    public static async Task<Result<IReadOnlyList<TOut>, IReadOnlyList<TErr>>> TraverseValidationAsync<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var pending = CreateList<T, Task<Result<TOut, TErr>>>(source);
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(AsyncGuards.Require(mapper(element), "mapper"));
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return SequenceValidation(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Maps each element to a task-wrapped result with a cancellation-aware mapper and collects every error,
    /// awaiting every task.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TOut">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="source">The source elements. Enumerated exactly once.</param>
    /// <param name="mapper">Maps an element to a task-wrapped result, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// <c>Ok</c> with all success values in source order, or <c>Err</c> with all errors in source order (never
    /// empty). An empty source yields <c>Ok([])</c>.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the mapper was invoked for the next element (the mapper is then not called) or
    /// before the produced tasks completed.
    /// </exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// The mapper is invoked for every element because aggregation never short-circuits. Use
    /// <see cref="Rop.TraverseAsync{T,TOut,TErr}"/> when only the first error matters.
    /// </remarks>
    public static async Task<Result<IReadOnlyList<TOut>, IReadOnlyList<TErr>>> TraverseValidationAsync<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, CancellationToken, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var pending = CreateList<T, Task<Result<TOut, TErr>>>(source);
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(AsyncGuards.Require(mapper(element, cancellationToken), "mapper"));
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return SequenceValidation(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }
}
