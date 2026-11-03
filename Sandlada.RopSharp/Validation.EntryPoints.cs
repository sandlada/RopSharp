namespace Sandlada.RopSharp;

/// <summary>Construction and aggregating combinators for <see cref="Validation{T,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var validated = Validation.Zip(
///         Result.Ok<Order, RopError>(OrderFixtures.Sample),
///         Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
///         static (order, amount) => order.Id);
///
/// // Success("ord-1001") when every input is Ok,
/// // Failure([E_AMOUNT]) collects every error without short-circuiting.
/// ]]></code>
/// </remarks>
public static partial class Validation
{
    /// <summary>Creates a success validation.</summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="value">The success value. Must not be <see langword="null"/> when <typeparamref name="T"/> is a reference type.</param>
    /// <returns>A validation on the success track.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.Success<Order, RopError>(OrderFixtures.Sample)
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") is created on the success track,
    /// // Failure([E_X]) is never constructed here - Success and Failure are the only factories.
    /// ]]></code>
    /// </remarks>
    public static Validation<T, TErr> Success<T, TErr>(T value)
    {
        if (ArgumentGuards.IsNull(value))
        {
            throw new ArgumentNullException(nameof(value), "A success value must not be null.");
        }

        return new Validation<T, TErr>(value, Array.Empty<TErr>(), Validation<T, TErr>.TagSuccess);
    }

    /// <summary>Creates a failure validation with a single error.</summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="error">The error. Must not be <see langword="null"/> when <typeparamref name="TErr"/> is a reference type.</param>
    /// <returns>A validation on the failure track.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.Failure<Order, RopError>(RopError.Create("E_ORDER", "missing"));
    ///
    /// // Failure([E_ORDER]) wraps one error,
    /// // Success(order) is never constructed here.
    /// ]]></code>
    /// </remarks>
    public static Validation<T, TErr> Failure<T, TErr>(TErr error)
    {
        Result.RequireError(error);
        return new Validation<T, TErr>(
            default!,
            Result.ToReadOnlyList(new List<TErr>(capacity: 1) { error }),
            Validation<T, TErr>.TagFailure);
    }

    /// <summary>Creates a failure validation with every error in the list.</summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="errors">A non-empty list with no null elements.</param>
    /// <returns>A validation on the failure track.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> or an element is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.Failure<Order, RopError>(
    ///     new[] { RopError.Create("E_A", "first"), RopError.Create("E_B", "second") });
    ///
    /// // Failure([E_A, E_B]) preserves every error,
    /// // Success(order) is never constructed here.
    /// ]]></code>
    /// </remarks>
    public static Validation<T, TErr> Failure<T, TErr>(IReadOnlyList<TErr> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("The error list must not be empty.", nameof(errors));
        }

        for (var index = 0; index < errors.Count; index++)
        {
            if (ArgumentGuards.IsNull(errors[index]))
            {
                throw new ArgumentNullException(nameof(errors), "The error list must not contain null elements.");
            }
        }

        return new Validation<T, TErr>(default!, errors, Validation<T, TErr>.TagFailure);
    }

    /// <summary>Lifts a short-circuit result onto the validation track.</summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns><c>Success</c> when the result is Ok; otherwise <c>Failure([error])</c>.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE"));
    ///
    /// // Success(payment) when the result is Ok,
    /// // Failure([E_CHARGE]) wraps the single Err error.
    /// ]]></code>
    /// </remarks>
    public static Validation<T, TErr> From<T, TErr>(Result<T, TErr> result)
    {
        if (result.TryGetOk(out var value))
        {
            return new Validation<T, TErr>(value, Array.Empty<TErr>(), Validation<T, TErr>.TagSuccess);
        }

        result.ThrowIfUninitialized();
        return Failure<T, TErr>(result.ErrUnsafe);
    }

    /// <summary>Combines two results applicatively, collecting every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.Zip(
    ///         Result.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
    ///         static (order, amount) => order.Id);
    ///
    /// // Success("ord-1001") when both inputs are Ok,
    /// // Failure([E_AMOUNT]) when either failed - Zip keeps every error.
    /// ]]></code>
    /// </remarks>
    public static Validation<TOut, TErr> Zip<T1, T2, TOut, TErr>(
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
            ? Success<TOut, TErr>(merger(a!, b!))
            : Failure<TOut, TErr>(Result.ToReadOnlyList(errors));
    }

    /// <summary>Combines three results applicatively, collecting every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.Zip(
    ///         Result.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total),
    ///         Result.Err<string>(RopError.Create("E_NOTE", "note required")),
    ///         static (order, amount, note) => order.Id);
    ///
    /// // Success("ord-1001") when every input is Ok,
    /// // Failure([E_NOTE]) when any input failed - all errors are collected.
    /// ]]></code>
    /// </remarks>
    public static Validation<TOut, TErr> Zip<T1, T2, T3, TOut, TErr>(
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
            ? Success<TOut, TErr>(merger(a!, b!, c!))
            : Failure<TOut, TErr>(Result.ToReadOnlyList(errors));
    }

    /// <summary>Sequences results, collecting every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.Sequence(new[]
    /// {
    ///     Result.Ok<int, RopError>(1),
    ///     Result.Err<int>(RopError.Create("E_LINE", "invalid line")),
    /// });
    ///
    /// // Success([1]) when every result is Ok,
    /// // Failure([E_LINE]) when any result failed - successes are still collected internally.
    /// ]]></code>
    /// </remarks>
    public static Validation<IReadOnlyList<T>, TErr> Sequence<T, TErr>(IEnumerable<Result<T, TErr>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = Result.CreateList<Result<T, TErr>, T>(results);
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
            ? Success<IReadOnlyList<T>, TErr>(Result.ToReadOnlyList(values))
            : Failure<IReadOnlyList<T>, TErr>(Result.ToReadOnlyList(errors));
    }

    /// <summary>Maps every element and collects every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.Traverse(
    ///     new[] { 1, 2 },
    ///     value => Result.Ok<int, RopError>(value));
    ///
    /// // Success([1, 2]) when every mapper returned Ok,
    /// // Failure([E_X]) when any mapper failed - every element is still mapped.
    /// ]]></code>
    /// </remarks>
    public static Validation<IReadOnlyList<TOut>, TErr> Traverse<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, Result<TOut, TErr>> mapper)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);

        var values = Result.CreateList<T, TOut>(source);
        List<TErr>? errors = null;
        foreach (var element in source)
        {
            var result = mapper(element);
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
            ? Success<IReadOnlyList<TOut>, TErr>(Result.ToReadOnlyList(values))
            : Failure<IReadOnlyList<TOut>, TErr>(Result.ToReadOnlyList(errors));
    }

    private static List<T> AddError<T>(List<T>? errors, T error)
    {
        errors ??= new List<T>(capacity: 2);
        errors.Add(error);
        return errors;
    }
}
