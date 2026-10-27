namespace Sandlada.RopSharp;

/// <summary>
/// Explicit bridges between <see cref="Option{T}"/> and <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// Absence and failure are different concepts, so every crossing is explicit. <c>RequireSome</c> is the only
/// Option-to-Result bridge; <c>ToOption</c> is the only reverse bridge and deliberately discards the error.
/// </remarks>
public static class OptionBridgeExtensions
{
    /// <summary>
    /// Converts an option into a result, using the supplied error when the option is empty.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="error">The error used for <c>None</c>. Must not be <see langword="null"/>.</param>
    /// <returns><c>Ok(value)</c> or <c>Err(error)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<T, TErr> RequireSome<T, TErr>(this Option<T> option, TErr error)
    {
        ArgumentGuards.ThrowIfNull(error, nameof(error));
        return option.TryGetValue(out var value)
            ? Rop.Ok<T, TErr>(value)
            : Rop.Err<T, TErr>(error);
    }

    /// <summary>
    /// Converts an option into a result, building the error lazily when the option is empty.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="errorFactory">Builds the error. Evaluated only for <c>None</c>.</param>
    /// <returns><c>Ok(value)</c> or <c>Err(errorFactory())</c>.</returns>
    public static Result<T, TErr> RequireSome<T, TErr>(this Option<T> option, Func<TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return option.TryGetValue(out var value)
            ? Rop.Ok<T, TErr>(value)
            : Rop.Err<T, TErr>(errorFactory());
    }

    /// <summary>
    /// Converts a result into an option, discarding the error.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns><c>Some(value)</c> for a success; <c>None</c> for an error.</returns>
    /// <remarks>
    /// Use only when the reason for failure is genuinely irrelevant; it cannot be recovered afterwards.
    /// A success carrying <see langword="null"/> (possible after a nullable mapping) is treated as <c>None</c>,
    /// because an option never contains null.
    /// </remarks>
    public static Option<T> ToOption<T, TErr>(this Result<T, TErr> result) =>
        result.TryGetOk(out var value) && value is not null ? Opt.Some(value) : Opt.None<T>();

    /// <summary>
    /// Flips <c>Option&lt;Result&lt;T, TErr&gt;&gt;</c> into <c>Result&lt;Option&lt;T&gt;, TErr&gt;</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="option">The nested value.</param>
    /// <returns>
    /// <c>Some(Ok(v))</c> becomes <c>Ok(Some(v))</c>; <c>Some(Err(e))</c> becomes <c>Err(e)</c>; <c>None</c> becomes
    /// <c>Ok(None)</c>. A success carrying <see langword="null"/> becomes <c>Ok(None)</c>, because an option never
    /// contains null.
    /// </returns>
    public static Result<Option<T>, TErr> Transpose<T, TErr>(this Option<Result<T, TErr>> option)
    {
        if (!option.TryGetValue(out var inner))
        {
            return Rop.Ok<Option<T>, TErr>(Opt.None<T>());
        }

        if (!inner.TryGetOk(out var value))
        {
            return Rop.Err<Option<T>, TErr>(inner.ErrUnsafe);
        }

        return Rop.OkUnchecked<Option<T>, TErr>(value is null ? Opt.None<T>() : Opt.Some(value));
    }

    /// <summary>
    /// Flips <c>Result&lt;Option&lt;T&gt;, TErr&gt;</c> into <c>Option&lt;Result&lt;T, TErr&gt;&gt;</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The nested value.</param>
    /// <returns>
    /// <c>Ok(Some(v))</c> becomes <c>Some(Ok(v))</c>; <c>Ok(None)</c> becomes <c>None</c>; <c>Err(e)</c> becomes
    /// <c>Some(Err(e))</c>.
    /// </returns>
    public static Option<Result<T, TErr>> Transpose<T, TErr>(this Result<Option<T>, TErr> result)
    {
        if (!result.TryGetOk(out var inner))
        {
            return Opt.Some(Rop.Err<T, TErr>(result.ErrUnsafe));
        }

        return inner.TryGetValue(out var value)
            ? Opt.Some(Rop.Ok<T, TErr>(value))
            : Opt.None<Result<T, TErr>>();
    }
}
