using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

[SuppressMessage(
    "ApiDesign",
    "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "The 2- and 3-input overloads differ by arity, and the Traverse mapper overloads differ by delegate shape; the optional token follows the library-wide ct-last convention.")]
public static partial class Validation
{
    /// <summary>Awaits two results and combines them applicatively, collecting every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await Validation.ZipAsync(
    ///     Task.FromResult(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     Task.FromResult(Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive"))),
    ///     static (order, amount) => order.Id);
    ///
    /// // Success("ord-1001") when both tasks complete with Ok,
    /// // Failure([E_AMOUNT]) when any task failed - every task is awaited first.
    /// ]]></code>
    /// </remarks>
    public static async Task<Validation<TOut, TErr>> ZipAsync<T1, T2, TOut, TErr>(
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
        return Zip(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            merger);
    }

    /// <summary>Awaits three results and combines them applicatively, collecting every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await Validation.ZipAsync(
    ///     Task.FromResult(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     Task.FromResult(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///     Task.FromResult(Result.Err<string>(RopError.Create("E_NOTE", "note required"))),
    ///     static (order, amount, note) => order.Id);
    ///
    /// // Success("ord-1001") when every task is Ok,
    /// // Failure([E_NOTE]) when any task failed - ZipAsync collects every error.
    /// ]]></code>
    /// </remarks>
    public static async Task<Validation<TOut, TErr>> ZipAsync<T1, T2, T3, TOut, TErr>(
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
        return Zip(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            await third.ConfigureAwait(false),
            merger);
    }

    /// <summary>Awaits every task and collects every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await Validation.SequenceAsync(new[]
    /// {
    ///     Task.FromResult(Result.Ok<int, RopError>(1)),
    ///     Task.FromResult(Result.Err<int>(RopError.Create("E_LINE", "invalid line"))),
    /// });
    ///
    /// // Success([1]) when every task completes with Ok,
    /// // Failure([E_LINE]) when any task failed - every task is awaited before deciding.
    /// ]]></code>
    /// </remarks>
    public static async Task<Validation<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        IEnumerable<Task<Result<T, TErr>>> tasks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        cancellationToken.ThrowIfCancellationRequested();

        var pending = Result.CreateList<Task<Result<T, TErr>>, Task<Result<T, TErr>>>(tasks);
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
        return Sequence(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Maps each element to a task-wrapped result and collects every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await Validation.TraverseAsync(
    ///     new[] { 1, 2 },
    ///     value => Task.FromResult(Result.Ok<int, RopError>(value)));
    ///
    /// // Success([1, 2]) when every mapper task returned Ok,
    /// // Failure([E_X]) when any mapper failed - every element is still mapped.
    /// ]]></code>
    /// </remarks>
    public static async Task<Validation<IReadOnlyList<TOut>, TErr>> TraverseAsync<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var pending = Result.CreateList<T, Task<Result<TOut, TErr>>>(source);
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(AsyncGuards.Require(mapper(element), "mapper"));
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return Sequence(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Maps each element with a cancellation-aware mapper and collects every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await Validation.TraverseAsync(
    ///     new[] { 1, 2 },
    ///     (value, _) => Task.FromResult(Result.Ok<int, RopError>(value)),
    ///     default);
    ///
    /// // Success([1, 2]) when every mapper task returned Ok,
    /// // Failure([E_X]) when any mapper failed - the token is passed to every mapper.
    /// ]]></code>
    /// </remarks>
    public static async Task<Validation<IReadOnlyList<TOut>, TErr>> TraverseAsync<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, CancellationToken, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var pending = Result.CreateList<T, Task<Result<TOut, TErr>>>(source);
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pending.Add(AsyncGuards.Require(mapper(element, cancellationToken), "mapper"));
        }

        var all = Task.WhenAll(pending);
        cancellationToken.ThrowIfCancellationRequested();
        return Sequence(await all.WaitAsync(cancellationToken).ConfigureAwait(false));
    }
}
