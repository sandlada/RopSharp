namespace Sandlada.RopSharp;

/// <summary>
/// Bridges synchronous <see cref="Validation{T,TErr}"/> values into <see cref="ValidationTask{T,TErr}"/> pipelines.
/// </summary>
/// <remarks>
/// <code><![CDATA[
/// var validated = await Validation.Success<Order, RopError>(OrderFixtures.Sample)
///     .ToValidationTask()
///     .MapAsync(static (order, _) => Task.FromResult(order.Id));
///
/// // Success("ord-1001") when the lifted validation succeeds,
/// // Failure([E_X]) when it failed - ToValidationTask lifts a completed validation.
/// ]]></code>
/// </remarks>
public static class ValidationTaskBridgeExtensions
{
    /// <summary>Wraps a validation as a completed computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await Validation.Success<Order, RopError>(OrderFixtures.Sample)
    ///     .ToValidationTask()
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") when the source was Success,
    /// // Failure([E_X]) when the source was Failure.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<T, TErr> ToValidationTask<T, TErr>(this Validation<T, TErr> validation) =>
        ValidationTask<T, TErr>.From(validation);

    /// <summary>Adopts a task that already carries a validation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var validated = await task.ToValidationTask()
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") when the task completes with Success,
    /// // Failure([E_X]) when the task completes with Failure.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<T, TErr> ToValidationTask<T, TErr>(this Task<Validation<T, TErr>> task) =>
        ValidationTask<T, TErr>.From(task);

    /// <summary>Maps the success value with an asynchronous mapper.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = await Validation.Success<Order, RopError>(OrderFixtures.Sample)
    ///     .MapAsync(static (order, _) => Task.FromResult(order.Id));
    ///
    /// // Success("ord-1001") when the source was Success,
    /// // Failure([E_X]) when the source was Failure.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<TOut, TErr> MapAsync<T, TOut, TErr>(
        this Validation<T, TErr> validation,
        Func<T, CancellationToken, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return ValidationTask<T, TErr>.From(validation).MapAsync(mapper);
    }
}
