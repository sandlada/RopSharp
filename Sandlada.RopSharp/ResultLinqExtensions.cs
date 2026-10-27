namespace Sandlada.RopSharp;

/// <summary>
/// LINQ query-syntax support for <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <remarks>
/// Only <c>select</c> and <c>from</c> are supported. <c>where</c> is intentionally absent because a result query
/// has no error value to produce for a failing predicate; use <see cref="ResultExtensions.Ensure{TOk,TErr}(Result{TOk,TErr},Func{TOk,bool},TErr)"/> instead.
/// </remarks>
public static class ResultLinqExtensions
{
    /// <summary>
    /// Projects a success value. Query-syntax equivalent of <c>select</c>.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TOut">The projected type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="selector">The projection.</param>
    /// <returns>The projected success, or the original error.</returns>
    public static Result<TOut, TErr> Select<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, TOut> selector) => result.MapOk(selector);

    /// <summary>
    /// Binds a success value to another result. Query-syntax equivalent of a second <c>from</c>.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TOut">The bound success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="binder">The binding function.</param>
    /// <returns>The bound result, or the original error.</returns>
    public static Result<TOut, TErr> SelectMany<TOk, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<TOut, TErr>> binder) => result.BindOk(binder);

    /// <summary>
    /// Binds a success value and projects the pair. Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TCol">The intermediate success type.</typeparam>
    /// <typeparam name="TOut">The projected type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="collectionSelector">The intermediate binding.</param>
    /// <param name="resultSelector">Projects both success values.</param>
    /// <returns>The projected success, or the first error.</returns>
    public static Result<TOut, TErr> SelectMany<TOk, TCol, TOut, TErr>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<TCol, TErr>> collectionSelector,
        Func<TOk, TCol, TOut> resultSelector) =>
        result.BindOk(first => collectionSelector(first).MapOk(second => resultSelector(first, second)));

    /// <summary>
    /// Projects a success value of a task-wrapped result. Query-syntax equivalent of <c>select</c>.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TOut">The projected type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="task">The source task.</param>
    /// <param name="selector">The projection.</param>
    /// <returns>A task that completes with the projected success or the original error.</returns>
    public static Task<Result<TOut, TErr>> Select<TOk, TOut, TErr>(
        this Task<Result<TOk, TErr>> task,
        Func<TOk, TOut> selector) => task.MapOkAsync(selector);

    /// <summary>
    /// Binds a success value of a task-wrapped result to another task-wrapped result and projects the pair.
    /// Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TCol">The intermediate success type.</typeparam>
    /// <typeparam name="TOut">The projected type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="source">The source task.</param>
    /// <param name="collectionSelector">The intermediate asynchronous binding.</param>
    /// <param name="resultSelector">Projects both success values.</param>
    /// <returns>A task that completes with the projected success or the first error.</returns>
    public static async Task<Result<TOut, TErr>> SelectMany<TOk, TCol, TOut, TErr>(
        this Task<Result<TOk, TErr>> source,
        Func<TOk, Task<Result<TCol, TErr>>> collectionSelector,
        Func<TOk, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var first = await source.ConfigureAwait(false);
        if (!first.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(first.ErrUnsafe);
        }

        var intermediate = await AsyncGuards.Require(collectionSelector(value), "collectionSelector")
            .ConfigureAwait(false);
        return intermediate.MapOk(second => resultSelector(value, second));
    }

    /// <summary>
    /// Binds a success value of a task-wrapped result to a synchronous result and projects the pair.
    /// Query-syntax equivalent of two <c>from</c> clauses plus <c>select</c>.
    /// </summary>
    /// <typeparam name="TOk">The current success type.</typeparam>
    /// <typeparam name="TCol">The intermediate success type.</typeparam>
    /// <typeparam name="TOut">The projected type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="source">The source task.</param>
    /// <param name="collectionSelector">The intermediate synchronous binding.</param>
    /// <param name="resultSelector">Projects both success values.</param>
    /// <returns>A task that completes with the projected success or the first error.</returns>
    public static async Task<Result<TOut, TErr>> SelectMany<TOk, TCol, TOut, TErr>(
        this Task<Result<TOk, TErr>> source,
        Func<TOk, Result<TCol, TErr>> collectionSelector,
        Func<TOk, TCol, TOut> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        var first = await source.ConfigureAwait(false);
        if (!first.TryGetOk(out var value))
        {
            return Rop.Err<TOut, TErr>(first.ErrUnsafe);
        }

        return collectionSelector(value).MapOk(intermediate => resultSelector(value, intermediate));
    }
}
