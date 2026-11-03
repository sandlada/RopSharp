using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sandlada.RopSharp;

/// <summary>An asynchronous computation that eventually produces a <see cref="Validation{T,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var validated = await ValidationTask<Order, RopError>.From(
///         Validation.Success<Order, RopError>(OrderFixtures.Sample))
///     .MapAsync(static (order, _) => Task.FromResult(order.Id));
///
/// // Success("ord-1001") when the lifted validation succeeds,
/// // Failure([E_X]) when it already failed - await unwraps the validation value.
/// ]]></code>
/// </remarks>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "From factories are declared on the computation struct per the M4 public contract.")]
public readonly partial struct ValidationTask<T, TErr> : IEquatable<ValidationTask<T, TErr>>
{
    internal const byte TagUninitialized = 0;
    internal const byte TagReady = 1;

    private readonly Task<Validation<T, TErr>>? _task;
    private readonly CancellationToken _cancellationToken;
    private readonly byte _tag;

    internal ValidationTask(Task<Validation<T, TErr>> task, byte tag, CancellationToken cancellationToken)
    {
        _task = task;
        _tag = tag;
        _cancellationToken = cancellationToken;
    }

    /// <summary>Wraps a completed validation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") when the lifted validation succeeds,
    /// // Failure([E_X]) when it already failed - From only wraps a completed value.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<T, TErr> From(Validation<T, TErr> validation) =>
        new(Task.FromResult(validation), TagReady, default);

    /// <summary>Adopts a task that already carries a validation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// Task<Validation<Order, RopError>> validate = Task.FromResult(
    ///     Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var id = await ValidationTask<Order, RopError>.From(validate)
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") when the adopted task completes with Success,
    /// // Failure([...]) when it completes with Failure.
    /// ]]></code>
    /// </remarks>
    public static ValidationTask<T, TErr> From(Task<Validation<T, TErr>> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new(task, TagReady, default);
    }

    /// <summary>Replaces the cancellation token observed by this computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// using var cts = new CancellationTokenSource();
    /// var id = await ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .WithCancellation(cts.Token)
    ///     .MapAsync(static (order, _) => Task.FromResult(order.Id));
    ///
    /// // Success("ord-1001") when the work completes before cancellation,
    /// // Failure([E_X]) is unchanged when the token never fires - OperationCanceledException when it does.
    /// ]]></code>
    /// </remarks>
    public ValidationTask<T, TErr> WithCancellation(CancellationToken cancellationToken)
    {
        ThrowIfUninitialized();
        return new(_task!, TagReady, cancellationToken);
    }

    /// <summary>Begins awaiting this computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = ValidationTask<Order, RopError>.From(
    ///         Validation.Success<Order, RopError>(OrderFixtures.Sample))
    ///     .GetAwaiter()
    ///     .GetResult();
    ///
    /// // Success(order) when the computation completes,
    /// // Failure([E_X]) when it completes with errors - GetAwaiter is what await expands to.
    /// ]]></code>
    /// </remarks>
    public ValidationTaskAwaiter<T, TErr> GetAwaiter()
    {
        ThrowIfUninitialized();
        return new ValidationTaskAwaiter<T, TErr>(_task!.WaitAsync(_cancellationToken).ConfigureAwait(false).GetAwaiter());
    }

    internal CancellationToken CancellationToken { get { ThrowIfUninitialized(); return _cancellationToken; } }
    internal Task<Validation<T, TErr>> TaskUnsafe { get { ThrowIfUninitialized(); return _task!; } }

    internal void ThrowIfUninitialized()
    {
        if (_tag == TagUninitialized)
        {
            throw new InvalidOperationException(
                "The computation is an uninitialized default value; construct values with ValidationTask.From.");
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var left = ValidationTask<Order, RopError>.From(task);
    /// var same = left.Equals((object)ValidationTask<Order, RopError>.From(task));
    ///
    /// // true when both wrap the same adopted task,
    /// // false when they differ - Success(order) and Failure([E_X]) never share one ValidationTask instance.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is ValidationTask<T, TErr> other && Equals(other);

    /// <summary>Determines whether two computations refer to the same underlying work and token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var left = ValidationTask<Order, RopError>.From(task);
    /// var right = ValidationTask<Order, RopError>.From(task);
    /// var same = left.Equals(right);
    ///
    /// // true when the task reference and cancellation token match,
    /// // false when they differ - Success(order) and Failure([E_X]) use different adopted work.
    /// ]]></code>
    /// </remarks>
    public bool Equals(ValidationTask<T, TErr> other) =>
        _tag == other._tag && _cancellationToken.Equals(other._cancellationToken) && ReferenceEquals(_task, other._task);

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var bucket = ValidationTask<Order, RopError>.From(task).GetHashCode() % 16;
    ///
    /// // a stable hash from the adopted task and token,
    /// // Success(order) and Failure([E_X]) both hash predictably for diagnostics only.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(_tag, _cancellationToken, _task);

    /// <summary>Determines whether two computations are equal.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var same = ValidationTask<Order, RopError>.From(task) == ValidationTask<Order, RopError>.From(task);
    ///
    /// // true when both refer to the same adopted work,
    /// // false when they differ - Failure([E_X]) is never == Success(order).
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(ValidationTask<T, TErr> left, ValidationTask<T, TErr> right) => left.Equals(right);

    /// <summary>Determines whether two computations are different.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var success = Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var failure = Task.FromResult(Validation.Failure<Order, RopError>(RopError.Create("E_ORDER", "invalid")));
    /// var different = ValidationTask<Order, RopError>.From(success) != ValidationTask<Order, RopError>.From(failure);
    ///
    /// // Success(order) differs from Failure([E_X]) when the adopted tasks differ,
    /// // false when both wrap the same task - Success(order) is never != itself.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(ValidationTask<T, TErr> left, ValidationTask<T, TErr> right) => !left.Equals(right);
}

/// <summary>Awaiter for <see cref="ValidationTask{T,TErr}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var awaiter = ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)).GetAwaiter();
/// var done = awaiter.IsCompleted;
///
/// // true when the underlying task already finished,
/// // false while running - Success(order) or Failure([E_X]) may already be available when IsCompleted is true.
/// ]]></code>
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Awaiters are ephemeral; equality is not part of the public contract.")]
public readonly struct ValidationTaskAwaiter<T, TErr> : ICriticalNotifyCompletion
{
    private readonly ConfiguredTaskAwaitable<Validation<T, TErr>>.ConfiguredTaskAwaiter _awaiter;
    internal ValidationTaskAwaiter(ConfiguredTaskAwaitable<Validation<T, TErr>>.ConfiguredTaskAwaiter awaiter) => _awaiter = awaiter;

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)).GetAwaiter();
    /// var done = awaiter.IsCompleted;
    ///
    /// // true when the validation is already available,
    /// // false while in flight - Success(order) or Failure([E_X]) when IsCompleted is true.
    /// ]]></code>
    /// </remarks>
    public bool IsCompleted => _awaiter.IsCompleted;

    /// <summary>Returns the completed validation value.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Validation.Success<Order, RopError>(OrderFixtures.Sample));
    /// var validated = ValidationTask<Order, RopError>.From(task).GetAwaiter().GetResult();
    ///
    /// // Success(order) when the task completed on the success track,
    /// // Failure([E_X]) when it completed with errors.
    /// ]]></code>
    /// </remarks>
    public Validation<T, TErr> GetResult() => _awaiter.GetResult();

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)).GetAwaiter();
    /// awaiter.OnCompleted(static () => { });
    ///
    /// // schedules the continuation when the computation completes,
    /// // for Success(order) or Failure([E_X]) without capturing the caller synchronization context.
    /// ]]></code>
    /// </remarks>
    public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = ValidationTask<Order, RopError>.From(Validation.Success<Order, RopError>(OrderFixtures.Sample)).GetAwaiter();
    /// awaiter.UnsafeOnCompleted(static () => { });
    ///
    /// // schedules the continuation on the thread pool when required,
    /// // for Success(order) or Failure([E_X]) mirroring ConfigureAwait(false) on the underlying task.
    /// ]]></code>
    /// </remarks>
    public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
}
