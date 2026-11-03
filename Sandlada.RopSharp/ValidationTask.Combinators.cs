namespace Sandlada.RopSharp;

/// <summary>Combinators for <see cref="ValidationTask{T,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
///     .Map(static order => order.Id);
///
/// // Success("ord-1001") on the success track,
/// // Failure([E_X]) preserves every collected error.
/// ]]></code>
/// </remarks>
public readonly partial struct ValidationTask<T, TErr>
{
    /// <summary>Maps the success value when validation succeeds.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") when the validation succeeds,
    /// // Failure([E_X]) when it already failed - the mapper never runs on Failure.
    /// ]]></code>
    /// </remarks>
    public ValidationTask<TOut, TErr> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();
        SuccessTypeGuards.ThrowIfTaskLike<TOut>("Map");
        return ValidationTask<TOut, TErr>.From(ValidationTaskCore.Map(_task!, mapper, _cancellationToken));
    }

    /// <summary>Maps the success value with an asynchronous mapper that receives the computation token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .MapAsync(static (order, _) => Task.FromResult(order.Id));
    ///
    /// // Success("ord-1001") when the validation succeeds,
    /// // Failure([E_X]) when it already failed - the asynchronous mapper never runs on Failure.
    /// ]]></code>
    /// </remarks>
    public ValidationTask<TOut, TErr> MapAsync<TOut>(Func<T, CancellationToken, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();
        return ValidationTask<TOut, TErr>.From(ValidationTaskCore.MapAsync(_task!, mapper, _cancellationToken));
    }
}
