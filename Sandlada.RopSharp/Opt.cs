namespace Sandlada.RopSharp;

/// <summary>
/// Static entry point for the <see cref="Option{T}"/> domain: construction and combining operations that have
/// no receiver.
/// </summary>
/// <remarks>
/// <para>
/// An option expresses a missing value, never a failed operation. When the reason for absence matters, convert
/// at the boundary with <see cref="OptionBridgeExtensions.RequireSome{T,TErr}(Option{T},TErr)"/>.
/// </para>
/// <para>
/// <see cref="Option{T}"/> has no aggregating counterpart: there is no error to collect, so a sequence collapses
/// to the first <c>None</c>.
/// </para>
/// <para>
/// <see cref="Opt"/> is where an option is born: a value enters the pipeline here, either because it is present
/// (<see cref="Some{T}(T)"/>) or because it is genuinely absent (<see cref="None{T}"/>). Everything downstream
/// only reshapes what was decided here.
/// </para>
/// <code><![CDATA[
/// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
///     .MapSome(static shipping => shipping.City);
///
/// // Some("London") when the customer has a shipping address,
/// // None when the address is missing - Opt is where an option is born.
/// ]]></code>
/// </remarks>
public static partial class Opt
{
    /// <summary>
    /// Creates an option containing <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The contained value. Must not be <see langword="null"/>.</param>
    /// <returns><c>Some(value)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>Use <see cref="FromNullable{T}(T)"/> or <see cref="None{T}"/> when the value may be missing.
    /// <code><![CDATA[
    /// var city = Opt.Some(OrderFixtures.Sample.Customer.Address!)
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is present,
    /// // None is the other track - Some never produces an empty option.
    /// ]]></code>
    /// </remarks>
    public static Option<T> Some<T>(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "Some requires a value. Use Opt.FromNullable or Opt.None when the value may be missing.");
        }

        return new Option<T>(value, isSome: true);
    }

    /// <summary>
    /// Creates an empty option.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns><c>None</c>.</returns>
    /// <remarks>
    /// The empty option is the "no value" track of the pipeline. It is also the default value of
    /// <see cref="Option{T}"/>, so an uninitialized field is contractually <c>None</c>.
    /// <code><![CDATA[
    /// var city = Opt.None<string>()
    ///     .BindNoneValue("unknown");
    ///
    /// // None stays the empty track when a fallback is applied,
    /// // Some(city) is the other track of the same option.
    /// ]]></code>
    /// </remarks>
    public static Option<T> None<T>() => default;

    /// <summary>
    /// Lifts a possibly-null reference into an option.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The possibly-null value.</param>
    /// <returns><c>None</c> when <paramref name="value"/> is <see langword="null"/>; otherwise <c>Some(value)</c>.</returns>
    /// <remarks>
    /// A literal <see langword="null"/> argument cannot be inferred; write <see cref="None{T}"/> or put the value
    /// in a typed variable first.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the customer has a shipping address,
    /// // None when the address is null - FromNullable never throws.
    /// ]]></code>
    /// </remarks>
    public static Option<T> FromNullable<T>(T? value)
        where T : class => value is null ? default : new Option<T>(value, isSome: true);

    /// <summary>
    /// Lifts a possibly-null value type into an option.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="value">The possibly-null value.</param>
    /// <returns><c>None</c> when <paramref name="value"/> has no value; otherwise <c>Some(value)</c>.</returns>
    /// <remarks>
    /// Named distinctly from <see cref="FromNullable{T}(T)"/> because C# does not permit two
    /// overloads that differ only by generic constraints.
    /// <code><![CDATA[
    /// int? quantity = OrderFixtures.Sample.ItemCount;
    /// var count = Opt.FromNullableValue(quantity)
    ///     .MapSome(static value => value.ToString(CultureInfo.InvariantCulture));
    ///
    /// // Some("3") when the order has an item count,
    /// // None when the count is null - FromNullableValue never throws.
    /// ]]></code>
    /// </remarks>
    public static Option<T> FromNullableValue<T>(T? value)
        where T : struct => value is null ? default : new Option<T>(value.GetValueOrDefault(), isSome: true);

    /// <summary>
    /// Creates an option from a condition, evaluating the factory only when the condition holds.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <param name="valueFactory">Builds the value. Not evaluated when <paramref name="condition"/> is <see langword="false"/>.</param>
    /// <returns><c>Some(valueFactory())</c> when the condition holds; otherwise <c>None</c>.</returns>
    /// <remarks>
    /// The factory is lazy, so a missing value costs nothing. It is the idiomatic way to express "this is
    /// optional, and here is what it is when it is there".
    /// <code><![CDATA[
    /// var expedited = Opt.When(OrderFixtures.Sample.ItemCount > 1, static () => "expedited")
    ///     .BindNoneValue("standard");
    ///
    /// // Some(expedited) when the order has more than one item,
    /// // None becomes standard - the factory runs only when the condition holds.
    /// ]]></code>
    /// </remarks>
    public static Option<T> When<T>(bool condition, Func<T> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
        return condition ? Some(valueFactory()) : None<T>();
    }

    /// <summary>
    /// Combines two options with a merger, yielding <c>None</c> when either is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <typeparam name="TOut">The merged value type.</typeparam>
    /// <param name="first">The first option.</param>
    /// <param name="second">The second option.</param>
    /// <param name="merger">Combines both values. Evaluated only when both are <c>Some</c>.</param>
    /// <returns><c>Some(merger(a, b))</c> or <c>None</c>.</returns>
    /// <remarks>
    /// The combination is short-circuiting: one missing value makes the whole combination missing, and the merger
    /// is never called.
    /// <code><![CDATA[
    /// var summary = Opt.Combine(
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Address),
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Name),
    ///         static (shipping, name) => $"{name} in {shipping.City}")
    ///     .BindNoneValue("unknown customer");
    ///
    /// // Some("Ada Lovelace in London") when both values are present,
    /// // None becomes unknown customer - the merger runs only when both are Some.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> Combine<T1, T2, TOut>(
        Option<T1> first,
        Option<T2> second,
        Func<T1, T2, TOut> merger)
    {
        ArgumentNullException.ThrowIfNull(merger);
        return first.TryGetValue(out var a) && second.TryGetValue(out var b)
            ? Some(merger(a, b))
            : None<TOut>();
    }

    /// <summary>
    /// Combines three options with a merger, yielding <c>None</c> when any is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <typeparam name="T3">The third value type.</typeparam>
    /// <typeparam name="TOut">The merged value type.</typeparam>
    /// <param name="first">The first option.</param>
    /// <param name="second">The second option.</param>
    /// <param name="third">The third option.</param>
    /// <param name="merger">Combines all values. Evaluated only when all are <c>Some</c>.</param>
    /// <returns><c>Some(merger(a, b, c))</c> or <c>None</c>.</returns>
    /// <remarks>
    /// The combination is short-circuiting: one missing value makes the whole combination missing, and the merger
    /// is never called.
    /// <code><![CDATA[
    /// var summary = Opt.Combine(
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Address),
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Name),
    ///         Opt.FromNullableValue<int>(OrderFixtures.Sample.ItemCount),
    ///         static (shipping, name, count) => $"{name} in {shipping.City} ({count} items)")
    ///     .BindNoneValue("unknown customer");
    ///
    /// // Some("Ada Lovelace in London (3 items)") when all three values are present,
    /// // None becomes unknown customer - the merger runs only when all are Some.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> Combine<T1, T2, T3, TOut>(
        Option<T1> first,
        Option<T2> second,
        Option<T3> third,
        Func<T1, T2, T3, TOut> merger)
    {
        ArgumentNullException.ThrowIfNull(merger);
        return first.TryGetValue(out var a) && second.TryGetValue(out var b) && third.TryGetValue(out var c)
            ? Some(merger(a, b, c))
            : None<TOut>();
    }

    /// <summary>
    /// Pairs two options into a tuple, yielding <c>None</c> when either is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <param name="first">The first option.</param>
    /// <param name="second">The second option.</param>
    /// <returns><c>Some((a, b))</c> or <c>None</c>.</returns>
    /// <remarks>
    /// <c>Zip</c> is <see cref="Combine{T1,T2,TOut}(Option{T1},Option{T2},Func{T1,T2,TOut})"/> with a tuple merger.
    /// Use it when the two values belong together.
    /// <code><![CDATA[
    /// var name = Opt.Zip(
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Address),
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Name))
    ///     .MapSome(static pair => pair.Item2);
    ///
    /// // Some("Ada Lovelace") when both values are present,
    /// // None when either is missing - Zip is Combine with a tuple merger.
    /// ]]></code>
    /// </remarks>
    public static Option<(T1, T2)> Zip<T1, T2>(Option<T1> first, Option<T2> second) =>
        Combine(first, second, static (a, b) => (a, b));

    /// <summary>
    /// Pairs three options into a tuple, yielding <c>None</c> when any is <c>None</c>.
    /// </summary>
    /// <typeparam name="T1">The first value type.</typeparam>
    /// <typeparam name="T2">The second value type.</typeparam>
    /// <typeparam name="T3">The third value type.</typeparam>
    /// <param name="first">The first option.</param>
    /// <param name="second">The second option.</param>
    /// <param name="third">The third option.</param>
    /// <returns><c>Some((a, b, c))</c> or <c>None</c>.</returns>
    /// <remarks>
    /// <c>Zip</c> is <see cref="Combine{T1,T2,T3,TOut}(Option{T1},Option{T2},Option{T3},Func{T1,T2,T3,TOut})"/>
    /// with a tuple merger. Use it when the three values belong together.
    /// <code><![CDATA[
    /// var name = Opt.Zip(
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Address),
    ///         Opt.FromNullable(OrderFixtures.Sample.Customer.Name),
    ///         Opt.FromNullableValue<int>(OrderFixtures.Sample.ItemCount))
    ///     .MapSome(static triple => triple.Item2);
    ///
    /// // Some("Ada Lovelace") when all three values are present,
    /// // None when any of them is missing - Zip is Combine with a tuple merger.
    /// ]]></code>
    /// </remarks>
    public static Option<(T1, T2, T3)> Zip<T1, T2, T3>(
        Option<T1> first,
        Option<T2> second,
        Option<T3> third) =>
        Combine(first, second, third, static (a, b, c) => (a, b, c));

    /// <summary>
    /// Turns a sequence of options into an option of a list, stopping at the first <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="options">The options to sequence. Enumerated exactly once, immediately.</param>
    /// <returns><c>Some</c> with all values in order, or <c>None</c>. An empty source yields <c>Some([])</c>.</returns>
    /// <remarks>
    /// The sequence is short-circuiting: the first empty option ends the traversal and the result is <c>None</c>.
    /// There is no aggregating counterpart, because an option carries no reason to collect.
    /// <code><![CDATA[
    /// var cities = Opt.Sequence(
    /// [
    ///     Opt.FromNullable(OrderFixtures.Sample.Customer.Address),
    ///     Opt.FromNullable(OrderFixtures.Sample.Customer.Address),
    /// ])
    ///     .MapSome(static list => list.Count);
    ///
    /// // Some(2) when every option carries a value,
    /// // None at the first missing value - Sequence stops there.
    /// ]]></code>
    /// </remarks>
    public static Option<IReadOnlyList<T>> Sequence<T>(IEnumerable<Option<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var values = new List<T>();
        foreach (var option in options)
        {
            if (!option.TryGetValue(out var value))
            {
                return None<IReadOnlyList<T>>();
            }

            values.Add(value);
        }

        return Some<IReadOnlyList<T>>(Rop.ToReadOnlyList(values));
    }

    /// <summary>
    /// Maps each element with <paramref name="mapper"/> and sequences the options, stopping at the first <c>None</c>.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="source">The source elements. Enumerated exactly once, immediately.</param>
    /// <param name="mapper">Maps an element to an option.</param>
    /// <returns><c>Some</c> with all values in order, or <c>None</c>.</returns>
    /// <remarks>
    /// <c>Traverse</c> is <see cref="Sequence{T}(IEnumerable{Option{T}})"/> with a mapping step in front. It maps
    /// first and sequences second, and stops at the first missing element.
    /// <code><![CDATA[
    /// var cities = Opt.Traverse(
    ///         OrderFixtures.Sample.Lines,
    ///         static line => Opt.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSome(static list => list.Count);
    ///
    /// // Some(2) when every mapped element is present,
    /// // None at the first missing element - Traverse maps first, then sequences.
    /// ]]></code>
    /// </remarks>
    public static Option<IReadOnlyList<TOut>> Traverse<T, TOut>(
        IEnumerable<T> source,
        Func<T, Option<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        var values = new List<TOut>();
        foreach (var element in source)
        {
            if (!mapper(element).TryGetValue(out var value))
            {
                return None<IReadOnlyList<TOut>>();
            }

            values.Add(value);
        }

        return Some<IReadOnlyList<TOut>>(Rop.ToReadOnlyList(values));
    }

    /// <summary>
    /// Discards empty options while preserving the order of present values. This operation never fails.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="options">The options to filter. Enumerated exactly once, immediately.</param>
    /// <returns>All present values in order.</returns>
    /// <remarks>
    /// Unlike <see cref="Sequence{T}(IEnumerable{Option{T}})"/>, this operation never produces <c>None</c>: the
    /// missing values are simply skipped. It is the right tool when a missing line must not cancel the whole batch.
    /// <code><![CDATA[
    /// var cities = Opt.Values(
    /// [
    ///     Opt.FromNullable(OrderFixtures.Sample.Customer.Address),
    ///     Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address),
    /// ])
    ///     .Select(static address => address.City);
    ///
    /// // ["London"] keeps the Some(address) entries,
    /// // None is dropped instead of failing - Values never produces an empty option.
    /// ]]></code>
    /// </remarks>
    public static IReadOnlyList<T> Values<T>(IEnumerable<Option<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var values = Rop.CreateList<Option<T>, T>(options);
        foreach (var option in options)
        {
            if (option.TryGetValue(out var value))
            {
                values.Add(value);
            }
        }

        return Rop.ToReadOnlyList(values);
    }
}
