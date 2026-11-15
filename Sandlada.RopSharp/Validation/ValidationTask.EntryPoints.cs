using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>Static aggregation entry points for <see cref="ValidationTask{T,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var validated = await ValidationTask.Zip(
///     ResultTask.From(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
///     ResultTask.From(Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive"))),
///     static (order, amount) => order.Id);
///
/// // Success("ord-1001") when every input is Ok,
/// // Failure([E_AMOUNT]) collects every Err without short-circuiting.
/// ]]></code>
/// </remarks>
[SuppressMessage("ApiDesign", "RS0026:Do not add multiple overloads with optional parameters",
    Justification = "Zip overloads differ by arity; the optional token follows the ct-last convention.")]
public static class ValidationTask
{
    /// <summary>Wraps a completed validation in a computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    ///
    /// // Success(order) when the source was Success,
    /// // Failure([E_X]) when the source was Failure.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<T, TErr> From<T, TErr>(Validation<T, TErr> validation) =>
        ValidationTask<T, TErr>.From(validation);

    /// <summary>Adopts a task that already carries a validation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.From(
    ///     Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample)));
    ///
    /// // Success(order) when the task completes with Success,
    /// // Failure([E_X]) when the task completes with Failure.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<T, TErr> From<T, TErr>(Task<Validation<T, TErr>> task) =>
        ValidationTask<T, TErr>.From(task);

    /// <summary>Awaits two result computations and merges successes, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ResultTask.From(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     ResultTask.From(Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive"))),
    ///     static (order, amount) => order.Id);
    ///
    /// // Success("ord-1001") when both inputs are Ok,
    /// // Failure([E_AMOUNT]) when any input is Err - every error is preserved.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<TOut, TErr> Zip<T1, T2, TOut, TErr>(
        ResultTask<T1, TErr> first, ResultTask<T2, TErr> second, Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<TOut, TErr>.From(
                ValidationTaskCore.ZipAsync(first.ObservedTask, second.ObservedTask, merger, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits three result computations and merges successes, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ResultTask.From(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     ResultTask.From(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///     ResultTask.From(Result.Err<int>(RopError.Create("E_COUNT", "invalid count"))),
    ///     static (order, amount, count) => $"{order.Id}:{amount}:{count}");
    ///
    /// // Success("ord-1001:44:3") when all three are Ok,
    /// // Failure([E_COUNT]) collects every Err without stopping at the first.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<TOut, TErr> Zip<T1, T2, T3, TOut, TErr>(
        ResultTask<T1, TErr> first, ResultTask<T2, TErr> second, ResultTask<T3, TErr> third,
        Func<T1, T2, T3, TOut> merger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        third.ThrowIfUninitialized();
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<TOut, TErr>.From(
                ValidationTaskCore.ZipAsync(first.ObservedTask, second.ObservedTask, third.ObservedTask, merger, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits every result computation and accumulates all errors.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Sequence<OrderLine, RopError>(new ResultTask<OrderLine, RopError>[]
    /// {
    ///     ResultTask.From(Result.Ok<OrderLine, RopError>(OrderFixtures.Sample.Lines[0])),
    ///     ResultTask.From(Result.Err<OrderLine>(RopError.Create("E_LINE", "missing line"))),
    /// });
    ///
    /// // Success([line0]) when every element is Ok,
    /// // Failure([E_LINE]) collects every Err - Sequence never short-circuits.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        IEnumerable<ResultTask<T, TErr>> computations, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(computations);
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<IReadOnlyList<T>, TErr>.From(ValidationTaskCore.SequenceAsync(computations, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Maps each element to a result computation and accumulates every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Traverse(
    ///     OrderFixtures.Sample.Lines,
    ///     static (line, _) => Task.FromResult(Result.Ok<OrderLine, RopError>(line)));
    ///
    /// // Success([line0, line1]) when every mapper returns Ok,
    /// // Failure([E_LINE]) collects every Err from the mappers.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<IReadOnlyList<TOut>, TErr> Traverse<T, TOut, TErr>(
        IEnumerable<T> source, Func<T, CancellationToken, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<IReadOnlyList<TOut>, TErr>.From(
                ValidationTaskCore.TraverseAsync(source, mapper, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits two result computations and merges successes into a tuple, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ResultTask.From(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     ResultTask.From(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)));
    ///
    /// // Success((order, 44.00m)) when both inputs are Ok,
    /// // Failure([...]) when any input fails.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<(T1 First, T2 Second), TErr> Zip<T1, T2, TErr>(
        ResultTask<T1, TErr> first,
        ResultTask<T2, TErr> second,
        CancellationToken cancellationToken = default) =>
        Zip(first, second, static (a, b) => (a, b), cancellationToken);

    /// <summary>Awaits three result computations and merges successes into a tuple, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ResultTask.From(Result.Ok<Order, RopError>(OrderFixtures.Sample)),
    ///     ResultTask.From(Result.Ok<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///     ResultTask.From(Result.Ok<int, RopError>(OrderFixtures.Sample.Lines.Count)));
    ///
    /// // Success((order, 44.00m, 2)) when all three inputs are Ok,
    /// // Failure([...]) when any input fails.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<(T1 First, T2 Second, T3 Third), TErr> Zip<T1, T2, T3, TErr>(
        ResultTask<T1, TErr> first,
        ResultTask<T2, TErr> second,
        ResultTask<T3, TErr> third,
        CancellationToken cancellationToken = default) =>
        Zip(first, second, third, static (a, b, c) => (a, b, c), cancellationToken);

    /// <summary>Awaits two validation computations and merges successes, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)),
    ///     ValidationTask.From(Validation.Failure<decimal, RopError>(RopError.Create("E_AMOUNT", "invalid amount"))),
    ///     static (order, amount) => order.Id);
    ///
    /// // Success("ord-1001") when both inputs succeed,
    /// // Failure([E_AMOUNT]) accumulates errors from both sides.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<TOut, TErr> Zip<T1, T2, TOut, TErr>(
        ValidationTask<T1, TErr> first,
        ValidationTask<T2, TErr> second,
        Func<T1, T2, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<TOut, TErr>.From(
                ValidationTaskCore.ZipAsync(first.ObservedTask, second.ObservedTask, merger, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits two validation computations into a tuple, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)),
    ///     ValidationTask.From(Validation.Success<decimal, RopError>(OrderFixtures.Sample.Total)));
    ///
    /// // Success((order, 44.00m)) when both inputs succeed,
    /// // Failure([...]) accumulates errors from both sides.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<(T1 First, T2 Second), TErr> Zip<T1, T2, TErr>(
        ValidationTask<T1, TErr> first,
        ValidationTask<T2, TErr> second,
        CancellationToken cancellationToken = default) =>
        Zip(first, second, static (a, b) => (a, b), cancellationToken);

    /// <summary>Awaits three validation computations and merges successes, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)),
    ///     ValidationTask.From(Validation.Success<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///     ValidationTask.From(Validation.Failure<int, RopError>(RopError.Create("E_COUNT", "invalid count"))),
    ///     static (order, amount, count) => $"{order.Id}:{amount}:{count}");
    ///
    /// // Success("ord-1001:44:2") when all three succeed,
    /// // Failure([E_COUNT]) accumulates errors across all three inputs.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<TOut, TErr> Zip<T1, T2, T3, TOut, TErr>(
        ValidationTask<T1, TErr> first,
        ValidationTask<T2, TErr> second,
        ValidationTask<T3, TErr> third,
        Func<T1, T2, T3, TOut> merger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(merger);
        first.ThrowIfUninitialized();
        second.ThrowIfUninitialized();
        third.ThrowIfUninitialized();
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<TOut, TErr>.From(
                ValidationTaskCore.ZipAsync(first.ObservedTask, second.ObservedTask, third.ObservedTask, merger, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Awaits three validation computations into a tuple, accumulating every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Zip(
    ///     ValidationTask.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)),
    ///     ValidationTask.From(Validation.Success<decimal, RopError>(OrderFixtures.Sample.Total)),
    ///     ValidationTask.From(Validation.Success<int, RopError>(OrderFixtures.Sample.Lines.Count)));
    ///
    /// // Success((order, 44.00m, 2)) when all three succeed,
    /// // Failure([...]) accumulates errors across all three inputs.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<(T1 First, T2 Second, T3 Third), TErr> Zip<T1, T2, T3, TErr>(
        ValidationTask<T1, TErr> first,
        ValidationTask<T2, TErr> second,
        ValidationTask<T3, TErr> third,
        CancellationToken cancellationToken = default) =>
        Zip(first, second, third, static (a, b, c) => (a, b, c), cancellationToken);

    /// <summary>Awaits every validation computation and accumulates all errors.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Sequence<OrderLine, RopError>(new ValidationTask<OrderLine, RopError>[]
    /// {
    ///     ValidationTask.From(Validation.Success<OrderLine, RopError>(OrderFixtures.Sample.Lines[0])),
    ///     ValidationTask.From(Validation.Failure<OrderLine, RopError>(RopError.Create("E_LINE", "missing line"))),
    /// });
    ///
    /// // Success([line0]) when every element succeeds,
    /// // Failure([E_LINE]) collects every error across all computations.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<IReadOnlyList<T>, TErr> Sequence<T, TErr>(
        IEnumerable<ValidationTask<T, TErr>> computations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(computations);
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<IReadOnlyList<T>, TErr>.From(
                ValidationTaskCore.SequenceAsync(computations, cancellationToken))
            .WithCancellation(cancellationToken);
    }

    /// <summary>Maps each element to a validation computation and accumulates every error.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await ValidationTask.Traverse(
    ///     OrderFixtures.Sample.Lines,
    ///     static (line, _) => Task.FromResult(Validation.Success<OrderLine, RopError>(line)));
    ///
    /// // Success([line0, line1]) when every mapper succeeds,
    /// // Failure([...]) collects errors from every mapped element.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<IReadOnlyList<TOut>, TErr> Traverse<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, CancellationToken, Task<Validation<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();
        return ValidationTask<IReadOnlyList<TOut>, TErr>.From(
                ValidationTaskCore.TraverseAsync(source, mapper, cancellationToken))
            .WithCancellation(cancellationToken);
    }
}
