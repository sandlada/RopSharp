namespace Sandlada.RopSharp;

/// <summary>
/// The shared core of the nullable-boundary helpers.
/// </summary>
/// <remarks>
/// <para>
/// The reference-typed and the value-typed variants of the nullable boundary differ in two places only: the
/// payload type and the way an empty payload is recognized and unwrapped. Everything else - the short-circuit on
/// an empty option, the null check on the binder result and the lift back into <c>Some</c> - is the same, so it
/// lives here once. The public surface keeps the two distinct names, because C# cannot overload a method by
/// generic constraints alone.
/// </para>
/// <para>
/// The unwrapping delegate is a cached static, so the shared core does not add an allocation per call.
/// </para>
/// </remarks>
internal static class OptionNullableCore
{
    /// <summary>
    /// Maps a value through a binder whose result may be empty, and lifts a non-empty result back into an option.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The mapped value type.</typeparam>
    /// <typeparam name="TPayload">The binder result type: the mapped value itself, or a nullable wrapper of it.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">Maps the value to a possibly-empty payload. Not evaluated for an empty option.</param>
    /// <param name="unwrap">Extracts the value from a payload that is known to be non-empty.</param>
    /// <returns><c>Some</c> when the binder returns a non-empty payload; otherwise <c>None</c>.</returns>
    internal static Option<TOut> BindSomeNullable<T, TOut, TPayload>(
        Option<T> option,
        Func<T, TPayload> binder,
        Func<TPayload, TOut> unwrap)
    {
        ArgumentNullException.ThrowIfNull(binder);
        ArgumentNullException.ThrowIfNull(unwrap);

        if (!option.TryGetValue(out var value))
        {
            return Option.None<TOut>();
        }

        var mapped = binder(value);

        return ArgumentGuards.IsNull(mapped) ? Option.None<TOut>() : Option.Some(unwrap(mapped));
    }
}
