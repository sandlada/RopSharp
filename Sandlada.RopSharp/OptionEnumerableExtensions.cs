namespace Sandlada.RopSharp;

/// <summary>
/// Sequence and side-effect helpers for <see cref="Option{T}"/> that sit on the boundary between LINQ
/// collections and the optional track.
/// </summary>
/// <remarks>
/// <para>
/// These helpers are intentionally small: they do not introduce a second collection abstraction. They either
/// probe a sequence (<c>TryFirst</c>, <c>TryLast</c>, <c>TryFind</c>), materialize optional values
/// (<c>Choose</c>) or run a side effect on one of the option tracks (<c>Execute</c>, <c>ExecuteNoValue</c>).
/// </para>
/// <para>
/// Methods that inspect a sequence enumerate it exactly once. <c>TryFirst</c> and <c>TryLast</c> stop as soon
/// as the answer is known; <c>Choose</c> materializes the result immediately and never returns <see langword="null"/>.
/// </para>
/// <para>
/// An <see cref="Option{T}"/> never contains <see langword="null"/>, so a <see langword="null"/> element, dictionary
/// value or mapped value is a programming error: the probing helpers and the projecting <c>Choose</c> throw
/// <see cref="ArgumentNullException"/> instead of turning the null into <c>Some(null)</c> or a null list entry. Use
/// <see cref="Opt.FromNullable{T}(T)"/> or filter the sequence when nulls are expected data.
/// </para>
/// <code><![CDATA[
/// var firstLine = OrderFixtures.Sample.Lines
///     .TryFirst(static line => line.Quantity > 0)
///     .MapSome(static line => line.Sku);
///
/// // Some("SKU-1") when a matching line exists,
/// // None when the sequence is empty or nothing matches.
/// ]]></code>
/// </remarks>
public static class OptionEnumerableExtensions
{
    private const string NullElementMessage =
        "The sequence contained null. An Option never contains null; remove the null element or use Opt.FromNullable.";

    private const string NullMappedValueMessage =
        "The mapper returned null. An Option never contains null; return Opt.None or use Opt.FromNullable instead.";

    /// <summary>
    /// Returns the first element of a sequence as <c>Some</c>, or <c>None</c> when the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence. Enumerated once.</param>
    /// <returns><c>Some(first)</c> or <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>, or its first element is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The sequence is enumerated only as far as the first element, so the probe is cheap even on a long list.
    /// <code><![CDATA[
    /// var firstLine = OrderFixtures.Sample.Lines
    ///     .TryFirst()
    ///     .MapSome(static line => line.Sku);
    ///
    /// // Some("SKU-1") when the sequence has an element,
    /// // None when it is empty - only the first element is read.
    /// ]]></code>
    /// </remarks>
    public static Option<T> TryFirst<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext()
            ? Opt.Some(RequireElement(enumerator.Current, nameof(source), NullElementMessage))
            : Opt.None<T>();
    }

    /// <summary>
    /// Returns the first element that satisfies a predicate as <c>Some</c>, or <c>None</c> when no element matches.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence. Enumerated once and stopped at the first match.</param>
    /// <param name="predicate">The predicate. Not evaluated after a match.</param>
    /// <returns><c>Some(firstMatch)</c> or <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>, or the matching element is
    /// <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The sequence stops at the first match, so the predicate is not evaluated on the remaining elements.
    /// <code><![CDATA[
    /// var firstLine = OrderFixtures.Sample.Lines
    ///     .TryFirst(static line => line.Quantity > 1)
    ///     .MapSome(static line => line.Sku);
    ///
    /// // Some("SKU-1") when a matching line exists,
    /// // None when nothing matches - the sequence stops at the first match.
    /// ]]></code>
    /// </remarks>
    public static Option<T> TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        foreach (var element in source)
        {
            if (predicate(element))
            {
                return Opt.Some(RequireElement(element, nameof(source), NullElementMessage));
            }
        }

        return Opt.None<T>();
    }

    /// <summary>
    /// Returns the last element of a sequence as <c>Some</c>, or <c>None</c> when the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence. Enumerated once.</param>
    /// <returns><c>Some(last)</c> or <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>, or its last element is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The whole sequence is enumerated, because the last element is only known at the end.
    /// <code><![CDATA[
    /// var lastLine = OrderFixtures.Sample.Lines
    ///     .TryLast()
    ///     .MapSome(static line => line.Sku);
    ///
    /// // Some("SKU-2") when the sequence has an element,
    /// // None when it is empty - the whole sequence is read.
    /// ]]></code>
    /// </remarks>
    public static Option<T> TryLast<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var found = false;
        T last = default!;
        foreach (var element in source)
        {
            found = true;
            last = element;
        }

        return found ? Opt.Some(RequireElement(last, nameof(source), NullElementMessage)) : Opt.None<T>();
    }

    /// <summary>
    /// Returns the last element that satisfies a predicate as <c>Some</c>, or <c>None</c> when no element matches.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence. Enumerated once.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns><c>Some(lastMatch)</c> or <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>, or the matching element is
    /// <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The whole sequence is enumerated, because the last match is only known at the end.
    /// <code><![CDATA[
    /// var lastLine = OrderFixtures.Sample.Lines
    ///     .TryLast(static line => line.Quantity > 0)
    ///     .MapSome(static line => line.Sku);
    ///
    /// // Some("SKU-2") when a matching line exists,
    /// // None when nothing matches - the whole sequence is read.
    /// ]]></code>
    /// </remarks>
    public static Option<T> TryLast<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        var found = false;
        T last = default!;
        foreach (var element in source)
        {
            if (predicate(element))
            {
                found = true;
                last = element;
            }
        }

        return found ? Opt.Some(RequireElement(last, nameof(source), NullElementMessage)) : Opt.None<T>();
    }

    /// <summary>
    /// Looks up a key in a read-only dictionary and returns the value as <c>Some</c> when present.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The dictionary. Not enumerated.</param>
    /// <param name="key">The key to find.</param>
    /// <returns><c>Some(value)</c> when the key exists; otherwise <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>, or the stored value is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The dictionary is not enumerated: the lookup is a single hash probe.
    /// <code><![CDATA[
    /// IReadOnlyDictionary<string, decimal> prices = new Dictionary<string, decimal> { ["SKU-1"] = 19.50m };
    /// var price = prices.TryFind("SKU-1")
    ///     .BindNoneValue(0m);
    ///
    /// // Some(19.50) when the key exists,
    /// // None becomes 0m when it does not - the dictionary is not enumerated.
    /// ]]></code>
    /// </remarks>
    public static Option<TValue> TryFind<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> source,
        TKey key)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.TryGetValue(key, out var value)
            ? Opt.Some(RequireElement(value, nameof(source), NullElementMessage))
            : Opt.None<TValue>();
    }

    /// <summary>
    /// Discards <c>None</c> values and materializes the present values in source order.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="source">The options to choose from. Enumerated once.</param>
    /// <returns>A read-only list of present values; empty when no option is <c>Some</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <c>Choose</c> keeps the present values and drops the empty ones. The result is a list, so it is never
    /// <see langword="null"/> and never empty-by-accident.
    /// <code><![CDATA[
    /// var skus = new[] { Opt.Some("SKU-1"), Opt.None<string>(), Opt.Some("SKU-2") }
    ///     .Choose();
    ///
    /// // ["SKU-1", "SKU-2"] keeps the Some(sku) entries,
    /// // None is dropped instead of failing - the result is never null.
    /// ]]></code>
    /// </remarks>
    public static IReadOnlyList<T> Choose<T>(this IEnumerable<Option<T>> source) => Opt.Values(source);

    /// <summary>
    /// Maps each present value and materializes the results in source order, discarding <c>None</c> values.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="source">The options to choose from. Enumerated once.</param>
    /// <param name="mapper">Projects a present value. Not evaluated for <c>None</c>.</param>
    /// <returns>A read-only list of projected values; empty when no option is <c>Some</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="mapper"/> is <see langword="null"/>, or the mapper returned
    /// <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The mapping happens only for the present values, and the mapper must not return null: an option never
    /// contains null.
    /// <code><![CDATA[
    /// var totals = new[] { Opt.Some(OrderFixtures.Sample), Opt.None<Order>() }
    ///     .Choose(static order => order.Total);
    ///
    /// // [44.00] keeps the Some(order) entries,
    /// // None is dropped instead of failing - the mapper never sees an empty option.
    /// ]]></code>
    /// </remarks>
    public static IReadOnlyList<TOut> Choose<T, TOut>(
        this IEnumerable<Option<T>> source,
        Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        var values = Rop.CreateList<Option<T>, TOut>(source);
        foreach (var option in source)
        {
            if (option.TryGetValue(out var value))
            {
                values.Add(RequireElement(mapper(value), nameof(mapper), NullMappedValueMessage));
            }
        }

        return Rop.ToReadOnlyList(values);
    }

    /// <summary>
    /// Runs an action when the option is <c>Some</c>; does nothing for <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The side effect. Exceptions propagate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <c>Execute</c> is the side-effecting counterpart of <see cref="OptionExtensions.TeeSome{T}(Option{T},Action{T})"/>:
    /// it returns nothing, so it can only be the last statement of a pipeline.
    /// <code><![CDATA[
    /// Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .Execute(static shipping => Console.WriteLine($"shipping to {shipping.City}"));
    ///
    /// // Some(address) runs the action,
    /// // None does nothing - Execute returns no value.
    /// ]]></code>
    /// </remarks>
    public static void Execute<T>(this Option<T> option, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (option.TryGetValue(out var value))
        {
            action(value);
        }
    }

    /// <summary>
    /// Runs an action when the option is <c>None</c>; does nothing for <c>Some</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The side effect. Exceptions propagate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <c>ExecuteNoValue</c> is the empty-track counterpart of <see cref="Execute{T}(Option{T},Action{T})"/>.
    /// <code><![CDATA[
    /// Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .ExecuteNoValue(static () => Console.WriteLine("no shipping address on file"));
    ///
    /// // None runs the action,
    /// // Some(address) does nothing - ExecuteNoValue returns no value.
    /// ]]></code>
    /// </remarks>
    public static void ExecuteNoValue<T>(this Option<T> option, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (option.IsNone)
        {
            action();
        }
    }

    private static T RequireElement<T>(T value, string paramName, string message) =>
        value is null ? throw new ArgumentNullException(paramName, message) : value!;
}
