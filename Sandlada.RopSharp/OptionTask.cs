using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sandlada.RopSharp;

/// <summary>An asynchronous computation that eventually produces an <see cref="Option{T}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
///     .MapSomeAsync(static (shipping, _) => Task.FromResult(shipping.City));
///
/// // Some("London") when the address is present,
/// // None when it is missing - await unwraps the option value.
/// ]]></code>
/// </remarks>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "From factories are declared on the computation struct per the M4 public contract.")]
public readonly partial struct OptionTask<T> : IEquatable<OptionTask<T>>
{
    internal const byte TagUninitialized = 0;
    internal const byte TagReady = 1;

    private readonly Task<Option<T>>? _task;
    private readonly CancellationToken _cancellationToken;
    private readonly byte _tag;

    internal OptionTask(Task<Option<T>> task, byte tag, CancellationToken cancellationToken)
    {
        _task = task;
        _tag = tag;
        _cancellationToken = cancellationToken;
    }

    /// <summary>Wraps a completed option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the address is present,
    /// // None when it is missing - From only lifts an existing option.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> From(Option<T> option) => new(Task.FromResult(option), TagReady, default);

    /// <summary>Adopts a task that already carries an option.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// Task<Option<Address>> lookup = Task.FromResult(
    ///     Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var city = await OptionTask.From(lookup)
    ///     .MapSome(static shipping => shipping.City);
    ///
    /// // Some("London") when the adopted task completes with Some,
    /// // None when it completes empty - From does not add a boundary.
    /// ]]></code>
    /// </remarks>
    public static OptionTask<T> From(Task<Option<T>> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new(task, TagReady, default);
    }

    /// <summary>Replaces the cancellation token observed by this computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// using var cts = new CancellationTokenSource();
    /// var city = await OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .WithCancellation(cts.Token)
    ///     .MapSomeAsync(static (shipping, _) => Task.FromResult(shipping.City));
    ///
    /// // Some("London") when the work completes before cancellation,
    /// // None when the address is missing - OperationCanceledException when the token fires first.
    /// ]]></code>
    /// </remarks>
    public OptionTask<T> WithCancellation(CancellationToken cancellationToken)
    {
        ThrowIfUninitialized();
        return new(_task!, TagReady, cancellationToken);
    }

    /// <summary>Begins awaiting this computation.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var address = OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address))
    ///     .GetAwaiter()
    ///     .GetResult();
    ///
    /// // Some(address) when the computation completes,
    /// // None when it completes empty - GetAwaiter is what await expands to.
    /// ]]></code>
    /// </remarks>
    public OptionTaskAwaiter<T> GetAwaiter()
    {
        ThrowIfUninitialized();
        return new OptionTaskAwaiter<T>(_task!.WaitAsync(_cancellationToken).ConfigureAwait(false).GetAwaiter());
    }

    internal CancellationToken CancellationToken { get { ThrowIfUninitialized(); return _cancellationToken; } }
    internal Task<Option<T>> TaskUnsafe { get { ThrowIfUninitialized(); return _task!; } }

    internal void ThrowIfUninitialized()
    {
        if (_tag == TagUninitialized)
        {
            throw new InvalidOperationException(
                "The computation is an uninitialized default value; construct values with OptionTask.From.");
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var left = OptionTask.From(task);
    /// var same = left.Equals((object)OptionTask.From(task));
    ///
    /// // true when both wrap the same adopted task,
    /// // false when they differ - Some(address) and None never share one OptionTask instance.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is OptionTask<T> other && Equals(other);

    /// <summary>Determines whether two computations refer to the same underlying work and token.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var left = OptionTask.From(task);
    /// var right = OptionTask.From(task);
    /// var same = left.Equals(right);
    ///
    /// // true when the task reference and cancellation token match,
    /// // false when they differ - Some(address) and None use different adopted work.
    /// ]]></code>
    /// </remarks>
    public bool Equals(OptionTask<T> other) =>
        _tag == other._tag && _cancellationToken.Equals(other._cancellationToken) && ReferenceEquals(_task, other._task);

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var bucket = OptionTask.From(task).GetHashCode() % 16;
    ///
    /// // a stable hash from the adopted task and token,
    /// // Some(address) and None both hash predictably for diagnostics only.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(_tag, _cancellationToken, _task);

    /// <summary>Determines whether two computations are equal.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var same = OptionTask.From(task) == OptionTask.From(task);
    ///
    /// // true when both refer to the same adopted work,
    /// // false when they differ - None never equals Some(address).
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(OptionTask<T> left, OptionTask<T> right) => left.Equals(right);

    /// <summary>Determines whether two computations are different.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var adopted = Task.FromResult(Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var different = OptionTask.From(adopted) != OptionTask.From(Task.FromResult(Option.None<Address>()));
    ///
    /// // Some(address) differs from None when the adopted tasks differ,
    /// // false when both wrap the same task - Some(address) is never != itself.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(OptionTask<T> left, OptionTask<T> right) => !left.Equals(right);
}

/// <summary>Awaiter for <see cref="OptionTask{T}"/>.</summary>
/// <remarks>
/// <code><![CDATA[
/// var awaiter = OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)).GetAwaiter();
/// var done = awaiter.IsCompleted;
///
/// // true when the underlying task already finished,
/// // false while running - Some(address) or None may already be available when IsCompleted is true.
/// ]]></code>
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1815:Override equals and operator equals on value types",
    Justification = "Awaiters are ephemeral; equality is not part of the public contract.")]
public readonly struct OptionTaskAwaiter<T> : ICriticalNotifyCompletion
{
    private readonly ConfiguredTaskAwaitable<Option<T>>.ConfiguredTaskAwaiter _awaiter;
    internal OptionTaskAwaiter(ConfiguredTaskAwaitable<Option<T>>.ConfiguredTaskAwaiter awaiter) => _awaiter = awaiter;

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)).GetAwaiter();
    /// var done = awaiter.IsCompleted;
    ///
    /// // true when the option is already available,
    /// // false while in flight - Some(address) or None when IsCompleted is true.
    /// ]]></code>
    /// </remarks>
    public bool IsCompleted => _awaiter.IsCompleted;

    /// <summary>Returns the completed option value.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var task = Task.FromResult(Option.FromNullable(OrderFixtures.Sample.Customer.Address));
    /// var address = OptionTask.From(task).GetAwaiter().GetResult();
    ///
    /// // Some(address) when the task completed with a value,
    /// // None when it completed empty - GetResult does not throw for None.
    /// ]]></code>
    /// </remarks>
    public Option<T> GetResult() => _awaiter.GetResult();

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)).GetAwaiter();
    /// awaiter.OnCompleted(static () => { });
    ///
    /// // schedules the continuation when the computation completes,
    /// // for Some(address) or None without capturing the caller synchronization context.
    /// ]]></code>
    /// </remarks>
    public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var awaiter = OptionTask.From(Option.FromNullable(OrderFixtures.Sample.Customer.Address)).GetAwaiter();
    /// awaiter.UnsafeOnCompleted(static () => { });
    ///
    /// // schedules the continuation on the thread pool when required,
    /// // for Some(address) or None mirroring ConfigureAwait(false) on the underlying task.
    /// ]]></code>
    /// </remarks>
    public void UnsafeOnCompleted(Action continuation) => _awaiter.UnsafeOnCompleted(continuation);
}
