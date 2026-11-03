using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sandlada.RopSharp;

/// <summary>
/// An asynchronous computation that eventually produces a <see cref="Result{TOk,TErr}"/>.
/// </summary>
/// <typeparam name="TOk">The success type.</typeparam>
/// <typeparam name="TErr">The error type.</typeparam>
/// <remarks>
/// <code><![CDATA[
/// var receiptId = await Result.TryAsync(
///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
///         "E_CHARGE")
///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
///
/// // Ok("pay-ord-1001") when the provider answered,
/// // Err(E_CHARGE) when the boundary faulted - await unwraps the result value.
/// ]]></code>
/// </remarks>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "From factories are declared on the computation struct per the M4 public contract.")]
public readonly partial struct ResultTask<TOk, TErr> : IEquatable<ResultTask<TOk, TErr>>
{
    internal const byte TagUninitialized = 0;
    internal const byte TagReady = 1;

    private readonly Task<Result<TOk, TErr>>? _task;
    private readonly CancellationToken _cancellationToken;
    private readonly byte _tag;

    internal ResultTask(Task<Result<TOk, TErr>> task, byte tag, CancellationToken cancellationToken)
    {
        _task = task;
        _tag = tag;
        _cancellationToken = cancellationToken;
    }

    /// <summary>Wraps a completed result.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var receiptId = await ResultTask.From(
    ///         Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"))
    ///     .MapOk(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the lifted result is Ok,
    /// // Err(E_CHARGE) when it is Err - From only wraps a completed value.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr> From(Result<TOk, TErr> result)
    {
        result.ThrowIfUninitialized();
        return new(Task.FromResult(result), TagReady, default);
    }

    /// <summary>Adopts an existing task that already carries a result.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// Task<Result<Payment, RopError>> charge = Task.FromResult(
    ///     Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var receiptId = await ResultTask.From(charge)
    ///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the adopted task completes with Ok,
    /// // Err(E_CHARGE) when it completes with Err - From does not add a boundary.
    /// ]]></code>
    /// </remarks>
    public static ResultTask<TOk, TErr> From(Task<Result<TOk, TErr>> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new(task, TagReady, default);
    }

    /// <summary>Replaces the cancellation token observed by this computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// using var cts = new CancellationTokenSource();
    /// var receiptId = await Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .WithCancellation(cts.Token)
    ///     .MapOkAsync(static (payment, _) => Task.FromResult(payment.Id));
    ///
    /// // Ok("pay-ord-1001") when the work completes before cancellation,
    /// // Err(E_CHARGE) is unchanged when the token never fires - OperationCanceledException when it does.
    /// ]]></code>
    /// </remarks>
    public ResultTask<TOk, TErr> WithCancellation(CancellationToken cancellationToken)
    {
        ThrowIfUninitialized();
        return new(_task!, TagReady, cancellationToken);
    }

    /// <summary>Begins awaiting this computation.</summary>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var payment = Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .GetAwaiter()
    ///     .GetResult();
    ///
    /// // Ok(payment) when the computation completes,
    /// // Err(E_CHARGE) when the boundary captured a fault - GetAwaiter is what await expands to.
    /// ]]></code>
    /// </remarks>
    public ResultTaskAwaiter<TOk, TErr> GetAwaiter()
    {
        ThrowIfUninitialized();
        return new ResultTaskAwaiter<TOk, TErr>(
            _task!.WaitAsync(_cancellationToken).ConfigureAwait(false).GetAwaiter());
    }

    internal CancellationToken CancellationToken
    {
        get
        {
            ThrowIfUninitialized();
            return _cancellationToken;
        }
    }

    internal Task<Result<TOk, TErr>> TaskUnsafe
    {
        get
        {
            ThrowIfUninitialized();
            return _task!;
        }
    }

    internal void ThrowIfUninitialized()
    {
        if (_tag == TagUninitialized)
        {
            throw new InvalidOperationException(
                "The computation is an uninitialized default value; construct values with ResultTask.From.");
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var left = ResultTask.From(task);
    /// var same = left.Equals((object)ResultTask.From(task));
    ///
    /// // true when both wrap the same adopted task,
    /// // false when they differ - Ok(payment) and Err(E_CHARGE) never share one ResultTask instance.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is ResultTask<TOk, TErr> other && Equals(other);

    /// <summary>Determines whether two computations refer to the same underlying work and token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var left = ResultTask.From(task);
    /// var right = ResultTask.From(task);
    /// var same = left.Equals(right);
    ///
    /// // true when the task reference and cancellation token match,
    /// // false when they differ - Ok(payment) and Err(E_CHARGE) use different adopted work.
    /// ]]></code>
    /// </remarks>
    public bool Equals(ResultTask<TOk, TErr> other) =>
        _tag == other._tag && _cancellationToken.Equals(other._cancellationToken) && ReferenceEquals(_task, other._task);

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var bucket = ResultTask.From(task).GetHashCode() % 16;
    ///
    /// // a stable hash from the adopted task and token,
    /// // Ok(payment) and Err(E_CHARGE) both hash predictably for diagnostics only.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(_tag, _cancellationToken, _task);

    /// <summary>Determines whether two computations are equal.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var same = ResultTask.From(task) == ResultTask.From(task);
    ///
    /// // true when both refer to the same adopted work,
    /// // false when they differ - Err(E_CHARGE) is never == Ok(payment).
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(ResultTask<TOk, TErr> left, ResultTask<TOk, TErr> right) => left.Equals(right);

    /// <summary>Determines whether two computations are different.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var charge = Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var declined = Task.FromResult(Result.Err<Payment>(RopError.Create("E_CHARGE", "declined")));
    /// var different = ResultTask.From(charge) != ResultTask.From(declined);
    ///
    /// // Some(address) differs from None when the adopted tasks differ,
    /// // false when both wrap the same task - Ok(payment) is never != itself.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(ResultTask<TOk, TErr> left, ResultTask<TOk, TErr> right) => !left.Equals(right);
}

/// <summary>Awaiter for <see cref="ResultTask{TOk,TErr}"/>.</summary>
/// <typeparam name="TOk">The success type.</typeparam>
/// <typeparam name="TErr">The error type.</typeparam>
/// <remarks>
/// <code><![CDATA[
/// var awaiter = Result.TryAsync(
///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
///         "E_CHARGE")
///     .GetAwaiter();
/// var done = awaiter.IsCompleted;
///
/// // true when the underlying task already finished,
/// // false while running - Ok(payment) or Err(E_CHARGE) may already be available when IsCompleted is true.
/// ]]></code>
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Awaiters are ephemeral; equality is not part of the public contract.")]
public readonly struct ResultTaskAwaiter<TOk, TErr> : ICriticalNotifyCompletion
{
    private readonly ConfiguredTaskAwaitable<Result<TOk, TErr>>.ConfiguredTaskAwaiter _awaiter;

    internal ResultTaskAwaiter(ConfiguredTaskAwaitable<Result<TOk, TErr>>.ConfiguredTaskAwaiter awaiter) =>
        _awaiter = awaiter;

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .GetAwaiter();
    /// var done = awaiter.IsCompleted;
    ///
    /// // true when the result is already available,
    /// // false while in flight - Ok(payment) or Err(E_CHARGE) when IsCompleted is true.
    /// ]]></code>
    /// </remarks>
    public bool IsCompleted => _awaiter.IsCompleted;

    /// <summary>Returns the completed result without throwing for the error track.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Result.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE"));
    /// var payment = ResultTask.From(task).GetAwaiter().GetResult();
    ///
    /// // Ok(payment) when the task completed on the success track,
    /// // Err(E_CHARGE) when it completed on the error track - GetResult never throws for Err.
    /// ]]></code>
    /// </remarks>
    public Result<TOk, TErr> GetResult() => _awaiter.GetResult();

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .GetAwaiter();
    /// awaiter.OnCompleted(static () => { });
    ///
    /// // schedules the continuation when the computation completes,
    /// // for Ok(payment) or Err(E_CHARGE) without capturing the caller synchronization context.
    /// ]]></code>
    /// </remarks>
    public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = Result.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_CHARGE")
    ///     .GetAwaiter();
    /// awaiter.UnsafeOnCompleted(static () => { });
    ///
    /// // schedules the continuation on the thread pool when required,
    /// // for Ok(payment) or Err(E_CHARGE) mirroring ConfigureAwait(false) on the underlying task.
    /// ]]></code>
    /// </remarks>
    public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
}
