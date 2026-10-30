using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sandlada.RopSharp;

/// <summary>
/// Asynchronous aggregation helpers for <see cref="Option{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// These helpers mirror the synchronous <see cref="Opt"/> combinators while preserving the option contract: a
/// missing value short-circuits the sequence, and cancellation is reported as
/// <see cref="OperationCanceledException"/> rather than <c>None</c>.
/// </para>
/// <para>
/// <c>CombineAsync</c> and <c>ZipAsync</c> await every supplied task before deciding, so faults from every input
/// are observed even when an earlier option is empty. <c>SequenceAsync</c> and <c>TraverseAsync</c> await in
/// enumeration order and stop at the first <c>None</c>; start tasks eagerly if they should run concurrently.
/// </para>
/// </remarks>
[SuppressMessage(
    "ApiDesign",
    "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "The 2- and 3-input overloads differ by arity, and the TraverseAsync overloads differ by delegate arity; existing calls cannot become ambiguous. The optional token follows the library-wide ct-last convention.")]
public static partial class Opt
{
    /// <summary>
    /// Awaits two options and combines them with a merger, yielding <c>None</c> when either is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <typeparam name="TOut">The merged value type.</typeparam>
    /// <param name="first">The first option task.</param>
    /// <param name="second">The second option task.</param>
    /// <param name="merger">Combines both values. Evaluated only when both are <c>Some</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some(merger(a, b))</c> or <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// Every input is awaited before the result is decided.
    /// <code><![CDATA[
    /// var summary = await Opt.CombineAsync(
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Name)),
    ///         static (shipping, name) => $"{name} in {shipping.City}")
    ///     .MapSomeAsync(static summary => summary.ToUpperInvariant());
    ///
    /// // Some("ADA LOVELACE IN LONDON") when both options carry a value,
    /// // None when either is empty - the merger runs only when both are Some.
    /// ]]></code>
    /// </remarks>
    public static async Task<Option<TOut>> CombineAsync<T1, T2, TOut>(
        Task<Option<T1>> first,
        Task<Option<T2>> second,
        Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(merger);
        cancellationToken.ThrowIfCancellationRequested();

        await Task.WhenAll(first, second).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Combine(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            merger);
    }

    /// <summary>
    /// Awaits three options and combines them with a merger, yielding <c>None</c> when any is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <typeparam name="T3">The third value type.</typeparam>
    /// <typeparam name="TOut">The merged value type.</typeparam>
    /// <param name="first">The first option task.</param>
    /// <param name="second">The second option task.</param>
    /// <param name="third">The third option task.</param>
    /// <param name="merger">Combines all values. Evaluated only when all are <c>Some</c>.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some(merger(a, b, c))</c> or <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// Every input is awaited before the result is decided.
    /// <code><![CDATA[
    /// var summary = await Opt.CombineAsync(
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Name)),
    ///         Task.FromResult(Opt.FromNullableValue<int>(OrderFixtures.Sample.ItemCount)),
    ///         static (shipping, name, count) => $"{name} in {shipping.City} ({count} items)")
    ///     .MapSomeAsync(static summary => summary.ToUpperInvariant());
    ///
    /// // Some("ADA LOVELACE IN LONDON (3 ITEMS)") when all three carry a value,
    /// // None when any of them is empty - the merger runs only when all are Some.
    /// ]]></code>
    /// </remarks>
    public static async Task<Option<TOut>> CombineAsync<T1, T2, T3, TOut>(
        Task<Option<T1>> first,
        Task<Option<T2>> second,
        Task<Option<T3>> third,
        Func<T1, T2, T3, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentNullException.ThrowIfNull(third);
        ArgumentNullException.ThrowIfNull(merger);
        cancellationToken.ThrowIfCancellationRequested();

        await Task.WhenAll(first, second, third).WaitAsync(cancellationToken).ConfigureAwait(false);
        return Combine(
            await first.ConfigureAwait(false),
            await second.ConfigureAwait(false),
            await third.ConfigureAwait(false),
            merger);
    }

    /// <summary>
    /// Awaits two options and pairs them into a tuple, yielding <c>None</c> when either is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <param name="first">The first option task.</param>
    /// <param name="second">The second option task.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some((a, b))</c> or <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// <c>ZipAsync</c> is <see cref="CombineAsync{T1,T2,TOut}"/> with a tuple merger.
    /// <code><![CDATA[
    /// var name = await Opt.ZipAsync(
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Name)))
    ///     .MapSomeAsync(static pair => pair.Item2);
    ///
    /// // Some("Ada Lovelace") when both options carry a value,
    /// // None when either is empty - ZipAsync is CombineAsync with a tuple merger.
    /// ]]></code>
    /// </remarks>
    public static Task<Option<(T1, T2)>> ZipAsync<T1, T2>(
        Task<Option<T1>> first,
        Task<Option<T2>> second,
        CancellationToken cancellationToken = default) =>
        CombineAsync(first, second, static (a, b) => (a, b), cancellationToken);

    /// <summary>
    /// Awaits three options and pairs them into a tuple, yielding <c>None</c> when any is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <typeparam name="T3">The third value type.</typeparam>
    /// <param name="first">The first option task.</param>
    /// <param name="second">The second option task.</param>
    /// <param name="third">The third option task.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some((a, b, c))</c> or <c>None</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled while awaiting the inputs.</exception>
    /// <remarks>
    /// <c>ZipAsync</c> is <see cref="CombineAsync{T1,T2,T3,TOut}"/> with a tuple merger.
    /// <code><![CDATA[
    /// var name = await Opt.ZipAsync(
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///         Task.FromResult(Opt.FromNullable(OrderFixtures.Sample.Customer.Name)),
    ///         Task.FromResult(Opt.FromNullableValue<int>(OrderFixtures.Sample.ItemCount)))
    ///     .MapSomeAsync(static triple => triple.Item2);
    ///
    /// // Some("Ada Lovelace") when all three options carry a value,
    /// // None when any of them is empty - ZipAsync is CombineAsync with a tuple merger.
    /// ]]></code>
    /// </remarks>
    public static Task<Option<(T1, T2, T3)>> ZipAsync<T1, T2, T3>(
        Task<Option<T1>> first,
        Task<Option<T2>> second,
        Task<Option<T3>> third,
        CancellationToken cancellationToken = default) =>
        CombineAsync(first, second, third, static (a, b, c) => (a, b, c), cancellationToken);

    /// <summary>
    /// Awaits a sequence of task-wrapped options, stopping at the first <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="tasks">The tasks to await in order. Enumerated exactly once.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some</c> with all values in order, or <c>None</c>. An empty source yields <c>Some([])</c>.</returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the sequence was started or before an element was awaited.
    /// </exception>
    /// <exception cref="InvalidOperationException">The sequence contained a null task.</exception>
    /// <remarks>
    /// Sequencing is deliberately sequential: options are awaited in enumeration order. Start the tasks eagerly at
    /// the call site if they should run concurrently. Tasks after the first <c>None</c> are not awaited.
    /// <code><![CDATA[
    /// var cities = await Opt.SequenceAsync(
    ///         new[] { Task.FromResult(Opt.Some("London")), Task.FromResult(Opt.None<string>()) })
    ///     .MapSomeAsync(static list => list.Count);
    ///
    /// // Some(1) when every option carries a value,
    /// // None at the first empty option - SequenceAsync stops there.
    /// ]]></code>
    /// </remarks>
    public static async Task<Option<IReadOnlyList<T>>> SequenceAsync<T>(
        IEnumerable<Task<Option<T>>> tasks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new List<T>();
        foreach (var task in tasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (task is null)
            {
                throw new InvalidOperationException("The sequence contained a null task.");
            }

            var option = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!option.TryGetValue(out var value))
            {
                return None<IReadOnlyList<T>>();
            }

            values.Add(value);
        }

        return Some<IReadOnlyList<T>>(Rop.ToReadOnlyList(values));
    }

    /// <summary>
    /// Maps each element to a task-wrapped option and awaits them in order, stopping at the first <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="source">The source elements. Enumerated exactly once.</param>
    /// <param name="mapper">Maps an element to a task-wrapped option.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some</c> with all values in order, or <c>None</c>. An empty source yields <c>Some([])</c>.</returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the mapper was invoked for the next element (the mapper is then not called).
    /// </exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// The mapper is not invoked after the first <c>None</c>.
    /// <code><![CDATA[
    /// var cities = await Opt.TraverseAsync(
    ///         OrderFixtures.Sample.Lines,
    ///         static line => Task.FromResult(Opt.Some(line.Sku)))
    ///     .MapSomeAsync(static list => list.Count);
    ///
    /// // Some(2) when every element mapped to a value,
    /// // None at the first empty option - the mapper is not invoked after it.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(0)]
    public static async Task<Option<IReadOnlyList<TOut>>> TraverseAsync<T, TOut>(
        IEnumerable<T> source,
        Func<T, Task<Option<TOut>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new List<TOut>();
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pending = mapper(element);
            if (pending is null)
            {
                throw new InvalidOperationException("The mapper returned a null task.");
            }

            var option = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!option.TryGetValue(out var value))
            {
                return None<IReadOnlyList<TOut>>();
            }

            values.Add(value);
        }

        return Some<IReadOnlyList<TOut>>(Rop.ToReadOnlyList(values));
    }

    /// <summary>
    /// Maps each element to a task-wrapped option with a cancellation-aware mapper and awaits them in order,
    /// stopping at the first <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="source">The source elements. Enumerated exactly once.</param>
    /// <param name="mapper">Maps an element to a task-wrapped option, receiving the same token.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Some</c> with all values in order, or <c>None</c>. An empty source yields <c>Some([])</c>.</returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the mapper was invoked for the next element (the mapper is then not called).
    /// </exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// The mapper is not invoked after the first <c>None</c>.
    /// <code><![CDATA[
    /// var cities = await Opt.TraverseAsync(
    ///         OrderFixtures.Sample.Lines,
    ///         static (line, token) => Task.FromResult(Opt.Some(line.Sku)),
    ///         CancellationToken.None)
    ///     .MapSomeAsync(static list => list.Count);
    ///
    /// // Some(2) when every element mapped to a value,
    /// // None at the first empty option - the mapper receives the same token.
    /// ]]></code>
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<Option<IReadOnlyList<TOut>>> TraverseAsync<T, TOut>(
        IEnumerable<T> source,
        Func<T, CancellationToken, Task<Option<TOut>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new List<TOut>();
        foreach (var element in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pending = mapper(element, cancellationToken);
            if (pending is null)
            {
                throw new InvalidOperationException("The mapper returned a null task.");
            }

            var option = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!option.TryGetValue(out var value))
            {
                return None<IReadOnlyList<TOut>>();
            }

            values.Add(value);
        }

        return Some<IReadOnlyList<TOut>>(Rop.ToReadOnlyList(values));
    }
}
