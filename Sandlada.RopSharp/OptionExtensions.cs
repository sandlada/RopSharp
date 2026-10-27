namespace Sandlada.RopSharp;

/// <summary>
/// Receiver-based combinators and terminals for <see cref="Option{T}"/>.
/// </summary>
/// <remarks>
/// Naming mirrors <see cref="ResultExtensions"/>: <c>MapSome</c>/<c>TeeSome</c>, <c>BindSome</c>/<c>BindNone</c>.
/// There is no <c>MapNone</c> because a missing value has nothing to map; the only actions on the empty track are
/// recovery (<c>BindNone</c>) and observation (<c>TeeNone</c>).
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
    /// <remarks>Intended for tests and boundaries only.</remarks>
    public static T OrThrow<T>(this Option<T> option) => option.TryGetValue(out var value)
        ? value
        : throw new InvalidOperationException("Expected Some but was None.");
}
