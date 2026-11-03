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
        return ValidationTask<TOut, TErr>.From(
                ValidationTaskCore.ZipAsync(first.TaskUnsafe, second.TaskUnsafe, merger, cancellationToken))
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
        return ValidationTask<TOut, TErr>.From(
                ValidationTaskCore.ZipAsync(first.TaskUnsafe, second.TaskUnsafe, third.TaskUnsafe, merger, cancellationToken))
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
        var tasks = new List<Task<Result<T, TErr>>>();
        foreach (var computation in computations)
        {
            computation.ThrowIfUninitialized();
            tasks.Add(computation.TaskUnsafe);
        }

        return ValidationTask<IReadOnlyList<T>, TErr>.From(ValidationTaskCore.SequenceAsync(tasks, cancellationToken))
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
        return ValidationTask<IReadOnlyList<TOut>, TErr>.From(
                ValidationTaskCore.TraverseAsync(source, mapper, cancellationToken))
            .WithCancellation(cancellationToken);
    }
}
