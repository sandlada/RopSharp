using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>Static aggregation entry points for <see cref="OptionTask{T}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var pair = await OptionTask.Zip(
///     OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)),
///     OptionTask.From(Option.Some(OrderFixtures.Sample.Id)));
///
/// // Some((address, "ord-1001")) when both complete with Some,
/// // None when any input is None - Zip short-circuits on the first empty track.
/// ]]></code>
/// </remarks>
[SuppressMessage("ApiDesign", "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "Zip overloads differ by arity; the optional token follows the ct-last convention.")]
public static class OptionTask
{
    /// <summary>Wraps a completed option in a computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the lifted option is Some,
    /// // None when it is empty - From only wraps a completed value.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> From<T>(Option<T> option) => OptionTask<T>.From(option);

    /// <summary>Adopts a task that already carries an option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// Task<Option<Address>> lookup = Task.FromResult(
    ///     Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var city = await OptionTask.From(lookup)
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the adopted task completes with Some,
    /// // None when it completes empty.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> From<T>(Task<Option<T>> task) => OptionTask<T>.From(task);

    /// <summary>Awaits two computations and merges present values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await OptionTask.Zip(
    ///     OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Id)),
    ///     static (shipping, id) => $"{id}:{shipping.City}");
    ///
    /// // Some("ord-1001:London") when both are Some,
    /// // None when either input is None - the merger never runs after None.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> Zip<T1, T2, TOut>(
        OptionTask<T1> first, OptionTask<T2> second, Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        return OptionTask<TOut>.From(OptionTaskCore.CombineAsync(first.TaskUnsafe, second.TaskUnsafe, merger, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits three computations and merges present values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = await OptionTask.Zip(
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Id)),
    ///     OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Total)),
    ///     static (id, shipping, total) => $"{id}:{shipping.City}:{total}");
    ///
    /// // Some("ord-1001:London:44.00") when all three are Some,
    /// // None when any input is None.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<TOut> Zip<T1, T2, T3, TOut>(
        OptionTask<T1> first, OptionTask<T2> second, OptionTask<T3> third, Func<T1, T2, T3, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        third.ThrowIfUninitialized();
        return OptionTask<TOut>.From(
                OptionTaskCore.CombineAsync(first.TaskUnsafe, second.TaskUnsafe, third.TaskUnsafe, merger, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits two computations and pairs their present values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var pair = await OptionTask.Zip(
    ///     OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Id)));
    ///
    /// // Some((address, "ord-1001")) when both are Some,
    /// // None when either input is None.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<(T1, T2)> Zip<T1, T2>(
        OptionTask<T1> first, OptionTask<T2> second, CancellationToken cancellationToken = default) =>
        Zip(first, second, static (a, b) => (a, b), cancellationToken);

    /// <summary>Awaits three computations and tuples their present values.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var triple = await OptionTask.Zip(
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Id)),
    ///     OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)),
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Total)));
    ///
    /// // Some((id, address, total)) when all three are Some,
    /// // None when any input is None.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<(T1, T2, T3)> Zip<T1, T2, T3>(
        OptionTask<T1> first, OptionTask<T2> second, OptionTask<T3> third, CancellationToken cancellationToken = default) =>
        Zip(first, second, third, static (a, b, c) => (a, b, c), cancellationToken);

    /// <summary>Awaits every computation in order, returning None when any element is None.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var lines = await OptionTask.Sequence(new[]
    /// {
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Lines[0])),
    ///     OptionTask.From(Option.Some(OrderFixtures.Sample.Lines[1])),
    /// });
    ///
    /// // Some([line0, line1]) when every element is Some,
    /// // None when any element is None - Sequence short-circuits on the first empty track.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<IReadOnlyList<T>> Sequence<T>(
        IEnumerable<OptionTask<T>> computations, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(computations);
        var tasks = new List<Task<Option<T>>>();
        foreach (var computation in computations)
        {
            computation.ThrowIfUninitialized();
            tasks.Add(computation.TaskUnsafe);
        }
        return OptionTask<IReadOnlyList<T>>.From(OptionTaskCore.SequenceAsync(tasks, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Maps each element to a computation and awaits in order.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var skus = await OptionTask.Traverse(
    ///     OrderFixtures.Sample.Lines,
    ///     static (line, _) => Task.FromResult(Option.Some(line.Sku)));
    ///
    /// // Some(["SKU-1", "SKU-2"]) when every mapper returns Some,
    /// // None when any mapper returns None.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<IReadOnlyList<TOut>> Traverse<T, TOut>(
        IEnumerable<T> source, Func<T, CancellationToken, Task<Option<TOut>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        return OptionTask<IReadOnlyList<TOut>>.From(OptionTaskCore.TraverseAsync(source, mapper, cancellationToken))
            .WithCancellation(cancellationToken);
    }
}
