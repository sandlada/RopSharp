namespace Sandlada.RopSharp;

/// <summary>
/// Receiver-based combinators and terminals for <see cref="Option{T}"/>.
/// </summary>
/// <remarks>
/// Naming mirrors <see cref="ResultExtensions"/>: <c>MapSome</c>/<c>TeeSome</c>, <c>BindSome</c>/<c>BindNone</c>.
/// There is no <c>MapNone</c> because a missing value has nothing to map; the only actions on the empty track are
/// recovery (<c>BindNone</c>) and observation (<c>TeeNone</c>).
/// <code><![CDATA[
/// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
///     .MapSome(static shipping => shipping.City)
///     .BindNoneValue("unknown");
///
/// // Some("London") when the customer has a shipping address,
/// // None becomes unknown - BindNoneValue is the recovery of the empty track.
/// ]]></code>
/// </remarks>
public static class OptionExtensions
{
    /// <summary>
    /// Maps the contained value, leaving <c>None</c> untouched.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="mapper">Maps the value. Not evaluated when the option is <c>None</c>.</param>
    /// <returns><c>Some(mapper(value))</c> or <c>None</c>.</returns>
    /// <remarks>
    /// <c>MapSome</c> reshapes a present value. It is the option counterpart of
    /// <see cref="ResultExtensions.MapOk{TOk,TOut,TErr}"/>, and it never runs on the empty track.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - the mapper never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> MapSome<T, TOut>(this Option<T> option, Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return option.TryGetValue(out var value) ? Opt.Some(mapper(value)) : Opt.None<TOut>();
    }

    /// <summary>
    /// Observes the contained value without changing the option.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The side effect. Exceptions propagate.</param>
    /// <returns>The same option value.</returns>
    /// <remarks>
    /// Only for logging and metrics. The option itself is returned unchanged, so the observation can be spliced
    /// into a chain without changing its shape.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .TeeSome(static shipping => Console.WriteLine($"shipping to {shipping.City}"));
    ///
    /// // Some(address) runs the side effect and keeps the option,
    /// // None skips it - the option itself is never changed.
    /// ]]></code>
    /// </remarks>
    public static Option<T> TeeSome<T>(this Option<T> option, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (option.TryGetValue(out var value))
        {
            action(value);
        }

        return option;
    }

    /// <summary>
    /// Observes the empty track without changing the option.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="action">The side effect. Exceptions propagate.</param>
    /// <returns>The same option value.</returns>
    /// <remarks>
    /// The empty-track counterpart of <see cref="TeeSome{T}(Option{T},Action{T})"/>. It is how a pipeline reports
    /// that a value was missing without giving up on the value it may still recover.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .TeeNone(static () => Console.WriteLine("no shipping address on file"));
    ///
    /// // None runs the side effect and keeps the option,
    /// // Some(address) skips it - the option itself is never changed.
    /// ]]></code>
    /// </remarks>
    public static Option<T> TeeNone<T>(this Option<T> option, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (option.IsNone)
        {
            action();
        }

        return option;
    }

    /// <summary>
    /// Keeps the value only when it satisfies a predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="predicate">The predicate. Not evaluated when the option is <c>None</c>.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    /// <remarks>
    /// <c>Filter</c> is the option counterpart of <see cref="ResultExtensions.Ensure{TOk,TErr}(Result{TOk,TErr},Func{TOk,bool},TErr)"/>:
    /// it turns a present but unwanted value into an absent one, without needing an error value.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .Filter(static shipping => shipping.Country is "GB")
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is in the UK,
    /// // None when it is not - the predicate never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return option.TryGetValue(out var value) && !predicate(value) ? Opt.None<T>() : option;
    }

    /// <summary>
    /// Replaces the contained value with a constant, leaving <c>None</c> untouched.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The replacement type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="value">The replacement value for the <c>Some</c> track. Must not be <see langword="null"/>.</param>
    /// <returns><c>Some(value)</c> or <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Use it to discard the payload and keep only the presence information.
    /// <code><![CDATA[
    /// var present = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSomeValue(true);
    ///
    /// // Some(True) when the address is present,
    /// // None when it is missing - the payload is replaced, not mapped.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> MapSomeValue<T, TOut>(this Option<T> option, TOut value)
    {
        ArgumentGuards.ThrowIfNull(value, nameof(value));
        return option.IsSome ? Opt.Some(value) : Opt.None<TOut>();
    }

    /// <summary>
    /// Expands a value into another option, leaving <c>None</c> untouched.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The bound value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">Maps the value to an option. Not evaluated when the option is <c>None</c>.</param>
    /// <returns>The bound option, or <c>None</c>.</returns>
    /// <remarks>
    /// <c>BindSome</c> is the expansion step of an option pipeline: the binder returns a whole new option, so it
    /// may be empty in its own right.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .BindSome(static shipping => Opt.Some(shipping.City));
    ///
    /// // Some("London") when the address is present and the binding succeeded,
    /// // None when it is missing - the binder never runs on the empty track.
    /// ]]></code>
    /// </remarks>
    public static Option<TOut> BindSome<T, TOut>(this Option<T> option, Func<T, Option<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return option.TryGetValue(out var value) ? binder(value) : Opt.None<TOut>();
    }

    /// <summary>
    /// Recovers from <c>None</c> by producing another option, leaving <c>Some</c> untouched.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="fallback">Produces the fallback option. Not evaluated on the <c>Some</c> track.</param>
    /// <returns>The original option, or the fallback option.</returns>
    /// <remarks>
    /// <c>BindNone</c> is the recovery step of an option pipeline. It is the only way to produce a value for the
    /// empty track, and it never runs when a value is already present.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .MapSome(static shipping => shipping.City)
    ///     .BindNone(static () => Opt.Some("unknown"));
    ///
    /// // Some("unknown") when the address is missing,
    /// // Some(city) when it is present - the fallback never runs on the present track.
    /// ]]></code>
    /// </remarks>
    public static Option<T> BindNone<T>(this Option<T> option, Func<Option<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return option.IsNone ? fallback() : option;
    }

    /// <summary>
    /// Recovers from <c>None</c> with a constant value, leaving <c>Some</c> untouched.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="fallback">The value used on the empty track. Must not be <see langword="null"/>.</param>
    /// <returns>The original option, or <c>Some(fallback)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The cheapest recovery: the pipeline continues with a known value instead of staying empty.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .MapSome(static shipping => shipping.City)
    ///     .BindNoneValue("unknown");
    ///
    /// // Some("unknown") when the address is missing,
    /// // Some(city) when it is present - a constant fallback replaces the empty track.
    /// ]]></code>
    /// </remarks>
    public static Option<T> BindNoneValue<T>(this Option<T> option, T fallback)
    {
        ArgumentGuards.ThrowIfNull(fallback, nameof(fallback));
        return option.IsNone ? Opt.Some(fallback) : option;
    }

    /// <summary>
    /// Recovers from <c>None</c> with a lazily built value, leaving <c>Some</c> untouched.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="fallbackFactory">Builds the fallback value. Evaluated only on the empty track.</param>
    /// <returns>The original option, or <c>Some(fallbackFactory())</c>.</returns>
    /// <remarks>
    /// The factory is lazy, so an expensive fallback is only built when the value is actually missing.
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .MapSome(static shipping => shipping.City)
    ///     .BindNoneValue(static () => "unknown");
    ///
    /// // Some("unknown") when the address is missing,
    /// // Some(city) when it is present - the factory runs only on the empty track.
    /// ]]></code>
    /// </remarks>
    public static Option<T> BindNoneValue<T>(this Option<T> option, Func<T> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        return option.IsNone ? Opt.Some(fallbackFactory()) : option;
    }

    /// <summary>
    /// Flattens a nested option.
    /// </summary>
    /// <typeparam name="T">The inner value type.</typeparam>
    /// <param name="option">The nested option.</param>
    /// <returns><c>Some(Some(v))</c> becomes <c>Some(v)</c>; <c>Some(None)</c> and <c>None</c> become <c>None</c>.</returns>
    /// <remarks>
    /// <c>Flatten</c> collapses two levels into one, which is what makes a nested option usable with the rest of
    /// the pipeline.
    /// <code><![CDATA[
    /// var nested = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .MapSome(static shipping => Opt.Some(shipping.City));
    /// var city = nested.Flatten();
    ///
    /// // Some("London") when both levels carry a value,
    /// // None when either level is empty - the two levels collapse into one.
    /// ]]></code>
    /// </remarks>
    public static Option<T> Flatten<T>(this Option<Option<T>> option) =>
        option.TryGetValue(out var inner) ? inner : Opt.None<T>();

    /// <summary>
    /// Exits the option by projecting both tracks into a single value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="some">Projects a present value.</param>
    /// <param name="none">Projects an empty option.</param>
    /// <returns>The projected value.</returns>
    /// <remarks>
    /// <c>Match</c> is the total exit of an option: it handles both tracks, so nothing is ever left behind.
    /// <code><![CDATA[
    /// var label = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .Match(
    ///         some: static shipping => $"ships to {shipping.City}",
    ///         none: static () => "no shipping address");
    ///
    /// // "ships to London" for Some(address),
    /// // "no shipping address" for None - Match is the production exit.
    /// ]]></code>
    /// </remarks>
    public static TResult Match<T, TResult>(
        this Option<T> option,
        Func<T, TResult> some,
        Func<TResult> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return option.TryGetValue(out var value) ? some(value) : none();
    }

    /// <summary>
    /// Returns the contained value or throws when the option is empty.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <returns>The contained value.</returns>
    /// <exception cref="InvalidOperationException">The option is <c>None</c>.</exception>
    /// <remarks>Intended for tests and boundaries only.
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.Sample.Customer.Address);
    /// var shipping = address.OrThrow();
    ///
    /// // Some(address) yields the address,
    /// // None throws - OrThrow is for tests and boundaries only.
    /// ]]></code>
    /// </remarks>
    public static T OrThrow<T>(this Option<T> option) => option.TryGetValue(out var value)
        ? value
        : throw new InvalidOperationException("Expected Some but was None.");
}
