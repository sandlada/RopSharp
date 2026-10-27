using System.Collections.ObjectModel;

namespace Sandlada.RopSharp;

/// <summary>
/// Static entry point for the <see cref="Result{TOk,TErr}"/> domain: construction, boundary lifting and
/// combining operations that have no receiver.
/// </summary>
public static partial class Rop
{
    /// <summary>
    /// Combines two results with a merger, short-circuiting on the first error from left to right.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TOut">The merged success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="merger">Combines both success values. Evaluated only when both are <c>Ok</c>.</param>
    /// <returns><c>Ok(merger(a, b))</c>, or the first <c>Err</c>.</returns>
    /// <remarks>
    /// Monadic (short-circuiting) semantics: the merger is not evaluated when either input failed, and only the
    /// first error is returned. Use <see cref="Validate{T1,T2,TOut,TErr}"/> when every error must be collected.
    /// </remarks>
    public static Result<TOut, TErr> Combine<T1, T2, TOut, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Func<T1, T2, TOut> merger)
    {
        ArgumentNullException.ThrowIfNull(merger);

        if (!first.TryGetOk(out var a))
        {
            return Err<TOut, TErr>(first.ErrUnsafe);
        }

        if (!second.TryGetOk(out var b))
        {
            return Err<TOut, TErr>(second.ErrUnsafe);
        }

        return OkUnchecked<TOut, TErr>(merger(a, b));
    }

    /// <summary>
    /// Combines three results with a merger, short-circuiting on the first error from left to right.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="T3">The third success type.</typeparam>
    /// <typeparam name="TOut">The merged success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <param name="merger">Combines all three success values. Evaluated only when all are <c>Ok</c>.</param>
    /// <returns><c>Ok(merger(a, b, c))</c>, or the first <c>Err</c>.</returns>
    public static Result<TOut, TErr> Combine<T1, T2, T3, TOut, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Result<T3, TErr> third,
        Func<T1, T2, T3, TOut> merger)
    {
        ArgumentNullException.ThrowIfNull(merger);

        if (!first.TryGetOk(out var a))
        {
            return Err<TOut, TErr>(first.ErrUnsafe);
        }

        if (!second.TryGetOk(out var b))
        {
            return Err<TOut, TErr>(second.ErrUnsafe);
        }

        if (!third.TryGetOk(out var c))
        {
            return Err<TOut, TErr>(third.ErrUnsafe);
        }

        return OkUnchecked<TOut, TErr>(merger(a, b, c));
    }

    /// <summary>
    /// Pairs two results into a tuple, short-circuiting on the first error from left to right.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <returns><c>Ok((a, b))</c>, or the first <c>Err</c>.</returns>
    public static Result<(T1, T2), TErr> Zip<T1, T2, TErr>(Result<T1, TErr> first, Result<T2, TErr> second) =>
        Combine(first, second, static (a, b) => (a, b));

    /// <summary>
    /// Pairs three results into a tuple, short-circuiting on the first error from left to right.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="T3">The third success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <returns><c>Ok((a, b, c))</c>, or the first <c>Err</c>.</returns>
    public static Result<(T1, T2, T3), TErr> Zip<T1, T2, T3, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Result<T3, TErr> third) =>
        Combine(first, second, third, static (a, b, c) => (a, b, c));

    /// <summary>
    /// Combines two results applicatively, collecting every error.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="TOut">The merged success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="merger">Combines both success values. Evaluated only when both are <c>Ok</c>.</param>
    /// <returns><c>Ok(merger(a, b))</c>, or <c>Err</c> with all errors in argument order (never empty).</returns>
    /// <remarks>
    /// Applicative (aggregating) semantics: every input is inspected, no short-circuit. Use
    /// <see cref="Combine{T1,T2,TOut,TErr}"/> when only the first error matters.
    /// </remarks>
    public static Result<TOut, IReadOnlyList<TErr>> Validate<T1, T2, TOut, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Func<T1, T2, TOut> merger)
    {
        ArgumentNullException.ThrowIfNull(merger);

        List<TErr>? errors = null;
        if (!first.TryGetOk(out var a))
        {
            errors = AddError(errors, first.ErrUnsafe);
        }

        if (!second.TryGetOk(out var b))
        {
            errors = AddError(errors, second.ErrUnsafe);
        }

        return errors is null
            ? OkUnchecked<TOut, IReadOnlyList<TErr>>(merger(a!, b!))
            : Err<TOut, IReadOnlyList<TErr>>(ToReadOnlyList(errors));
    }

    /// <summary>
    /// Combines three results applicatively, collecting every error.
    /// </summary>
    /// <typeparam name="T1">The first success type.</typeparam>
    /// <typeparam name="T2">The second success type.</typeparam>
    /// <typeparam name="T3">The third success type.</typeparam>
    /// <typeparam name="TOut">The merged success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <param name="merger">Combines all success values. Evaluated only when all are <c>Ok</c>.</param>
    /// <returns><c>Ok(merger(a, b, c))</c>, or <c>Err</c> with all errors in argument order (never empty).</returns>
    public static Result<TOut, IReadOnlyList<TErr>> Validate<T1, T2, T3, TOut, TErr>(
        Result<T1, TErr> first,
        Result<T2, TErr> second,
        Result<T3, TErr> third,
        Func<T1, T2, T3, TOut> merger)
    {
        ArgumentNullException.ThrowIfNull(merger);

        List<TErr>? errors = null;
        if (!first.TryGetOk(out var a))
        {
            errors = AddError(errors, first.ErrUnsafe);
        }

        if (!second.TryGetOk(out var b))
        {
            errors = AddError(errors, second.ErrUnsafe);
        }

        if (!third.TryGetOk(out var c))
        {
            errors = AddError(errors, third.ErrUnsafe);
        }

        return errors is null
            ? OkUnchecked<TOut, IReadOnlyList<TErr>>(merger(a!, b!, c!))
            : Err<TOut, IReadOnlyList<TErr>>(ToReadOnlyList(errors));
    }

    /// <summary>
    /// Turns a sequence of results into a result of a list, short-circuiting on the first error.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="results">The results to sequence. Enumerated exactly once, immediately.</param>
    /// <returns><c>Ok</c> with all success values in order, or the first <c>Err</c>. An empty source yields <c>Ok([])</c>.</returns>
    public static Result<IReadOnlyList<T>, TErr> Sequence<T, TErr>(IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = new List<T>();
        foreach (var result in results)
        {
            if (!result.TryGetOk(out var value))
            {
                return Err<IReadOnlyList<T>, TErr>(result.ErrUnsafe);
            }

            values.Add(value);
        }

        return Ok<IReadOnlyList<T>, TErr>(ToReadOnlyList(values));
    }

    /// <summary>
    /// Turns a sequence of results into a result of a list, collecting every error.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="results">The results to sequence. Enumerated exactly once, immediately.</param>
    /// <returns><c>Ok</c> with all success values, or <c>Err</c> with all errors (never empty).</returns>
    public static Result<IReadOnlyList<T>, IReadOnlyList<TErr>> SequenceValidation<T, TErr>(
        IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = CreateList<Result<T, TErr>, T>(results);
        List<TErr>? errors = null;
        foreach (var result in results)
        {
            if (result.TryGetOk(out var value))
            {
                values.Add(value);
            }
            else
            {
                errors = AddError(errors, result.ErrUnsafe);
            }
        }

        return errors is null
            ? Ok<IReadOnlyList<T>, IReadOnlyList<TErr>>(ToReadOnlyList(values))
            : Err<IReadOnlyList<T>, IReadOnlyList<TErr>>(ToReadOnlyList(errors));
    }

    /// <summary>
    /// Maps each element with <paramref name="mapper"/> and sequences the results, short-circuiting on the first error.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TOut">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="source">The source elements. Enumerated exactly once, immediately.</param>
    /// <param name="mapper">Maps an element to a result.</param>
    /// <returns><c>Ok</c> with all success values in order, or the first <c>Err</c>.</returns>
    public static Result<IReadOnlyList<TOut>, TErr> Traverse<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, Result<TOut, TErr>> mapper)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);

        var values = new List<TOut>();
        foreach (var element in source)
        {
            var result = mapper(element);
            if (!result.TryGetOk(out var value))
            {
                return Err<IReadOnlyList<TOut>, TErr>(result.ErrUnsafe);
            }

            values.Add(value);
        }

        return Ok<IReadOnlyList<TOut>, TErr>(ToReadOnlyList(values));
    }

    /// <summary>
    /// Splits a sequence of results into successes and errors. This operation never fails.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="results">The results to partition. Enumerated exactly once, immediately.</param>
    /// <returns>A tuple of all success values and all errors, preserving order.</returns>
    public static (IReadOnlyList<T> Oks, IReadOnlyList<TErr> Errs) Partition<T, TErr>(
        IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = CreateList<Result<T, TErr>, T>(results);
        var errors = CreateList<Result<T, TErr>, TErr>(results);
        foreach (var result in results)
        {
            if (result.TryGetOk(out var value))
            {
                values.Add(value);
            }
            else
            {
                errors.Add(result.ErrUnsafe);
            }
        }

        return (ToReadOnlyList(values), ToReadOnlyList(errors));
    }

    internal static List<T> CreateList<TSource, T>(IEnumerable<TSource> source) =>
        source is ICollection<TSource> sized ? new List<T>(sized.Count) : new List<T>();

    private static List<T> AddError<T>(List<T>? errors, T error)
    {
        if (errors is null)
        {
            errors = new List<T>(capacity: 2);
        }

        errors.Add(error);
        return errors;
    }

    internal static IReadOnlyList<T> ToReadOnlyList<T>(List<T> items)
    {
        if (items.Count == 0)
        {
            return Array.Empty<T>();
        }

        // When the list is exactly sized (the common case for small aggregations) the wrapper can expose its
        // backing store directly, saving an array copy. Otherwise copy first so that a grown list's spare
        // capacity is not retained by the exposed read-only view.
        return items.Capacity == items.Count
            ? new ReadOnlyCollection<T>(items)
            : new ReadOnlyCollection<T>(items.ToArray());
    }
}
