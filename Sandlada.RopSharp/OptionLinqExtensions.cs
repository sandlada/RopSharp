namespace Sandlada.RopSharp;

/// <summary>
/// LINQ query-syntax support for <see cref="Option{T}"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="Result{TOk,TErr}"/>, options support <c>where</c>: filtering only needs to produce <c>None</c>,
/// so no error value is required.
/// </remarks>
public static class OptionLinqExtensions
{
    /// <summary>
    /// Projects a present value. Query-syntax equivalent of <c>select</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="selector">The projection.</param>
    /// <returns>The projected value, or <c>None</c>.</returns>
    public static Option<TOut> Select<T, TOut>(this Option<T> option, Func<T, TOut> selector) =>
        option.MapSome(selector);

    /// <summary>
    /// Binds a present value to another option. Query-syntax equivalent of a second <c>from</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The bound value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="binder">The binding function.</param>
    /// <returns>The bound option, or <c>None</c>.</returns>
    public static Option<TOut> SelectMany<T, TOut>(
        this Option<T> option,
        Func<T, Option<TOut>> binder) => option.BindSome(binder);

    /// <summary>
    /// Binds a present value and projects the pair. Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TCol">The intermediate value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="collectionSelector">The intermediate binding.</param>
    /// <param name="resultSelector">Projects both values.</param>
    /// <returns>The projected value, or <c>None</c>.</returns>
    public static Option<TOut> SelectMany<T, TCol, TOut>(
        this Option<T> option,
        Func<T, Option<TCol>> collectionSelector,
        Func<T, TCol, TOut> resultSelector) =>
        option.BindSome(first =>
            collectionSelector(first).MapSome(second => resultSelector(first, second)));

    /// <summary>
    /// Filters a present value. Query-syntax equivalent of <c>where</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The original option when the predicate holds; otherwise <c>None</c>.</returns>
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Filter(predicate);

    /// <summary>
    /// Projects a present value of a task-wrapped option. Query-syntax equivalent of <c>select</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="source">The source task.</param>
    /// <param name="selector">The projection.</param>
    /// <returns>A task that completes with the projected value, or <c>None</c>.</returns>
    public static Task<Option<TOut>> Select<T, TOut>(
        this Task<Option<T>> source,
        Func<T, TOut> selector) => source.MapSomeAsync(selector);

    /// <summary>
    /// Binds a present value of a task-wrapped option to another task-wrapped option and projects the pair.
    /// Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TCol">The intermediate value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="source">The source task.</param>
    /// <param name="collectionSelector">The intermediate asynchronous binding.</param>
    /// <param name="resultSelector">Projects both values.</param>
    /// <returns>A task that completes with the projected value, or <c>None</c>.</returns>
    /// <exception cref="InvalidOperationException">The collection selector returned a null task.</exception>
    /// <remarks>Query syntax has no token slot; cancellation remains an outer concern.</remarks>
    public static async Task<Option<TOut>> SelectMany<T, TCol, TOut>(
        this Task<Option<T>> source,
        Func<T, Task<Option<TCol>>> collectionSelector,
        Func<T, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var option = await source.ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        var intermediate = await AsyncGuards.Require(collectionSelector(value), "collectionSelector")
            .ConfigureAwait(false);
        return intermediate.MapSome(second => resultSelector(value, second));
    }

    /// <summary>
    /// Binds a present value of a task-wrapped option to a synchronous option and projects the pair.
    /// Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="T">The current value type.</typeparam>
    /// <typeparam name="TCol">The intermediate value type.</typeparam>
    /// <typeparam name="TOut">The projected value type.</typeparam>
    /// <param name="source">The source task.</param>
    /// <param name="collectionSelector">The intermediate synchronous binding.</param>
    /// <param name="resultSelector">Projects both values.</param>
    /// <returns>A task that completes with the projected value, or <c>None</c>.</returns>
    /// <remarks>Query syntax has no token slot; cancellation remains an outer concern.</remarks>
    public static async Task<Option<TOut>> SelectMany<T, TCol, TOut>(
        this Task<Option<T>> source,
        Func<T, Option<TCol>> collectionSelector,
        Func<T, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var option = await source.ConfigureAwait(false);
        if (!option.TryGetValue(out var value))
        {
            return Opt.None<TOut>();
        }

        return collectionSelector(value).MapSome(second => resultSelector(value, second));
    }

    /// <summary>
    /// Filters a present value of a task-wrapped option. Query-syntax equivalent of <c>where</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="source">The source task.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A task that completes with the original option when the predicate holds; otherwise <c>None</c>.</returns>
    public static Task<Option<T>> Where<T>(
        this Task<Option<T>> source,
        Func<T, bool> predicate) => source.FilterAsync(predicate);
}
