using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// Static entry point for the <see cref="Result{TOk,TErr}"/> domain: asynchronous boundaries and collection sequencing.
/// </summary>
/// <remarks>
/// <para>
/// The asynchronous boundary is <c>TryAsync</c>: it is the only place where a faulted task becomes a domain value.
/// Everything else only reshapes the result.
/// </para>
/// <code><![CDATA[
/// var receiptId = await Rop.TryAsync(
///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
///         "E_CHARGE")
///     .MapOkAsync(static payment => payment.Id);
///
/// // Ok("pay-ord-1001") when the provider answered,
/// // Err(E_CHARGE) when the task faulted - TryAsync is the asynchronous boundary.
/// ]]></code>
/// </remarks>
public static partial class Rop
{
    /// <summary>
    /// Lifts an asynchronous function into a function returning a task-wrapped <see cref="Result{TOk,TErr}"/> on the
    /// success track.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="func">The asynchronous function to lift. Exceptions are not caught.</param>
    /// <returns>A function that awaits <paramref name="func"/> and always returns <c>Ok</c> on success.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The function returned a null task.</exception>
    /// <remarks>
    /// <para>
    /// This is the asynchronous counterpart of <see cref="Rop.Switch{T,TOut,TErr}"/>; the name is intentionally the
    /// same operation with an awaited result. It is not the action terminal
    /// <see cref="ResultTerminalExtensions.SwitchAsync{TOk,TErr}"/>, which consumes an existing result.
    /// </para>
    /// <para>
    /// The lifted function carries no token of its own; close over the caller's token inside
    /// <paramref name="func"/> when cancellation is required. A null payload throws
    /// <see cref="ArgumentNullException"/> from <see cref="Ok{TOk,TErr}"/>, exactly like the synchronous lift.
    /// </para>
    /// <code><![CDATA[
    /// Func<Order, Task<Result<Payment, RopError>>> charge = Rop.SwitchAsync<Order, Payment, RopError>(
    ///     static order => OrderFixtures.ChargeAsync(order));
    /// var receipt = await charge(OrderFixtures.Sample)
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the task completed,
    /// // Err(E_ORDER) is the other track - the lifted function always returns Ok.
    /// ]]></code>
    /// </remarks>
    public static Func<T, Task<Result<TOut, TErr>>> SwitchAsync<T, TOut, TErr>(Func<T, Task<TOut>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return async value => Ok<TOut, TErr>(
            await AsyncGuards.Require(func(value), "func").ConfigureAwait(false));
    }

    /// <summary>
    /// Lifts an asynchronous function into a function returning a task-wrapped <see cref="Result{TOk,TErr}"/>,
    /// converting thrown exceptions into errors except for <see cref="OperationCanceledException"/>.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="func">The asynchronous function to lift.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <returns>A function that never throws for expected failures.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> or <paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The returned function delegates to <see cref="TryAsync{T}(Func{Task{T}},Func{Exception,RopError},CancellationToken)"/>,
    /// so a null task faults and a null payload produces <c>E_NULL_RESULT</c> exactly as that boundary does.
    /// <code><![CDATA[
    /// Func<Order, Task<Result<Payment, RopError>>> charge = Rop.TrySwitchAsync<Order, Payment>(
    ///     static order => OrderFixtures.ChargeAsync(order),
    ///     static exception => RopError.Create("E_CHARGE", exception.Message));
    /// var receipt = await charge(OrderFixtures.Sample)
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the provider answered,
    /// // Err(E_CHARGE) when the task faulted - TrySwitchAsync is the reusable boundary.
    /// ]]></code>
    /// </remarks>
    public static Func<T, Task<Result<TOut, RopError>>> TrySwitchAsync<T, TOut>(
        Func<T, Task<TOut>> func,
        Func<Exception, RopError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(errorFactory);
        return value => TryAsync(() => func(value), errorFactory);
    }

    /// <summary>
    /// Lifts an asynchronous function into a function returning a task-wrapped <see cref="Result{TOk,TErr}"/> with a
    /// caller-defined error type, converting thrown exceptions except for <see cref="OperationCanceledException"/>.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="func">The asynchronous function to lift.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <param name="nullResultErrorFactory">
    /// Builds the error used when <paramref name="func"/> resolves to <see langword="null"/>. Must return a non-null error.
    /// </param>
    /// <returns>A function that never throws for expected failures.</returns>
    /// <exception cref="ArgumentNullException">An argument is <see langword="null"/>.</exception>
    /// <remarks>
    /// The returned function delegates to
    /// <see cref="TryAsync{T,TErr}(Func{Task{T}},Func{Exception,TErr},Func{TErr},CancellationToken)"/>, so a null task
    /// faults and a null payload produces <c>Err(nullResultErrorFactory())</c> exactly as that boundary does.
    /// <code><![CDATA[
    /// Func<Order, Task<Result<Payment, string>>> charge = Rop.TrySwitchAsync<Order, Payment, string>(
    ///     static order => OrderFixtures.ChargeAsync(order),
    ///     static exception => exception.Message,
    ///     static () => "the provider returned no payment");
    /// var receipt = await charge(OrderFixtures.Sample)
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the provider answered,
    /// // Err(the provider returned no payment) when it returned null - a custom error type.
    /// ]]></code>
    /// </remarks>
    public static Func<T, Task<Result<TOut, TErr>>> TrySwitchAsync<T, TOut, TErr>(
        Func<T, Task<TOut>> func,
        Func<Exception, TErr> errorFactory,
        Func<TErr> nullResultErrorFactory)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(errorFactory);
        ArgumentNullException.ThrowIfNull(nullResultErrorFactory);
        return value => TryAsync(() => func(value), errorFactory, nullResultErrorFactory);
    }

    /// <summary>
    /// Runs an asynchronous boundary function and converts a thrown exception into an error, except
    /// <see cref="OperationCanceledException"/> which is always rethrown.</summary>
    /// <param name="action">The asynchronous boundary action to execute.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(Unit.Value)</c> when the action completes; otherwise <c>Err(errorFactory(exception))</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled or the action threw <see cref="OperationCanceledException"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The boundary action returned a null task, or the error factory returned null.
    /// </exception>
    /// <remarks>
    /// Use this overload for asynchronous side effects that produce no value; the success payload is
    /// <see cref="Unit"/>. Prefer this over <see cref="TryAsync{T}(Func{Task{T}},Func{Exception,RopError},CancellationToken)"/>
    /// with a discarded value, which would require inventing a placeholder payload.
    /// <code><![CDATA[
    /// var shipped = await Rop.TryAsync(
    ///         () => OrderFixtures.ShipAsync(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)),
    ///         static exception => RopError.Create("E_SHIP", exception.Message))
    ///     .MapOkAsync(static _ => Unit.Value);
    ///
    /// // Ok(()) when the parcel was handed over,
    /// // Err(E_SHIP) when the task faulted - the action overload returns Unit.
    /// ]]></code>
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Rop.TryAsync is the single asynchronous boundary where any non-cancellation exception becomes a domain error.")]
    public static async Task<Result<Unit, RopError>> TryAsync(
        Func<Task> action,
        Func<Exception, RopError> errorFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(errorFactory);
        cancellationToken.ThrowIfCancellationRequested();

        Task pending;
        try
        {
            pending = action();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<Unit, RopError>(MaterializeException(errorFactory, exception));
        }

        if (pending is null)
        {
            throw new InvalidOperationException("The boundary function returned a null task.");
        }

        try
        {
            await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            return Ok<Unit, RopError>(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<Unit, RopError>(MaterializeException(errorFactory, exception));
        }
    }

    /// <summary>
    /// Runs an asynchronous boundary action and converts a thrown exception into a caller-defined error, except
    /// <see cref="OperationCanceledException"/> which is always rethrown.
    /// </summary>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="action">The asynchronous boundary action to execute.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(Unit.Value)</c> when the action completes; otherwise <c>Err(errorFactory(exception))</c>.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled or the action threw <see cref="OperationCanceledException"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The boundary action returned a null task, or the error factory returned null.
    /// </exception>
    /// <remarks>
    /// There is no null-result branch because an action produces no payload. Use this overload when the caller's
    /// error system is not <see cref="RopError"/>.
    /// <code><![CDATA[
    /// var shipped = await Rop.TryAsync(
    ///         () => OrderFixtures.ShipAsync(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)),
    ///         static exception => exception.Message)
    ///     .MapOkAsync(static _ => Unit.Value);
    ///
    /// // Ok(()) when the parcel was handed over,
    /// // Err(the carrier refused) when the task faulted - a custom error type.
    /// ]]></code>
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Rop.TryAsync is the single asynchronous boundary where any non-cancellation exception becomes a domain error.")]
    public static async Task<Result<Unit, TErr>> TryAsync<TErr>(
        Func<Task> action,
        Func<Exception, TErr> errorFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(errorFactory);
        cancellationToken.ThrowIfCancellationRequested();

        Task pending;
        try
        {
            pending = action();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<Unit, TErr>(MaterializeException(errorFactory, exception));
        }

        if (pending is null)
        {
            throw new InvalidOperationException("The boundary function returned a null task.");
        }

        try
        {
            await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            return Ok<Unit, TErr>(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<Unit, TErr>(MaterializeException(errorFactory, exception));
        }
    }

    /// <summary>
    /// Runs an asynchronous boundary action and converts a thrown exception into an error with the given
    /// <paramref name="code"/>, except <see cref="OperationCanceledException"/> which is always rethrown.
    /// </summary>
    /// <param name="action">The asynchronous boundary action to execute.</param>
    /// <param name="code">The error code to attach to captured exceptions.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(Unit.Value)</c> or <c>Err</c> as described by <see cref="TryAsync(Func{Task},Func{Exception,RopError},CancellationToken)"/>.</returns>
    /// <remarks>
    /// The shortest asynchronous boundary: the action runs, and a faulted task becomes a coded error.
    /// <code><![CDATA[
    /// var shipped = await Rop.TryAsync(
    ///         () => OrderFixtures.ShipAsync(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)),
    ///         "E_SHIP")
    ///     .MapOkAsync(static _ => Unit.Value);
    ///
    /// // Ok(()) when the parcel was handed over,
    /// // Err(E_SHIP) when the task faulted - the code overload attaches the exception.
    /// ]]></code>
    /// </remarks>
    public static Task<Result<Unit, RopError>> TryAsync(
        Func<Task> action,
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return TryAsync(action, exception => RopError.FromException(exception, code), cancellationToken);
    }

    /// <summary>
    /// Runs an asynchronous boundary function and converts a thrown exception into an error, except
    /// <see cref="OperationCanceledException"/> which is always rethrown.
    /// </summary>
    /// <typeparam name="T">The produced value type.</typeparam>
    /// <param name="func">The boundary function to execute.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// <c>Ok(value)</c> when the function returns a non-null value; <c>Err(errorFactory(exception))</c> when it throws;
    /// <c>Err</c> with code <c>E_NULL_RESULT</c> when it returns <see langword="null"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The token was cancelled or the function threw <see cref="OperationCanceledException"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The boundary function returned a null task, or the error factory returned null.
    /// </exception>
    /// <remarks>
    /// This is the asynchronous boundary for a function that produces a value. The task is awaited, and a fault
    /// becomes an error instead of an exception.
    /// <code><![CDATA[
    /// var charged = await Rop.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         static exception => RopError.Create("E_PAYMENT", exception.Message))
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the provider answered,
    /// // Err(E_PAYMENT) when the task faulted - TryAsync is the asynchronous boundary.
    /// ]]></code>
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Rop.TryAsync is the single asynchronous boundary where any non-cancellation exception becomes a domain error.")]
    public static async Task<Result<T, RopError>> TryAsync<T>(
        Func<Task<T>> func,
        Func<Exception, RopError> errorFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(errorFactory);
        cancellationToken.ThrowIfCancellationRequested();

        Task<T> pending;
        try
        {
            pending = func();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<T, RopError>(MaterializeException(errorFactory, exception));
        }

        if (pending is null)
        {
            throw new InvalidOperationException("The boundary function returned a null task.");
        }

        try
        {
            var value = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            return value is null
                ? Err<T, RopError>(NullResultError())
                : Ok<T, RopError>(value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<T, RopError>(MaterializeException(errorFactory, exception));
        }
    }

    /// <summary>
    /// Runs an asynchronous boundary function and converts a thrown exception into a caller-defined error, except
    /// <see cref="OperationCanceledException"/> which is always rethrown.
    /// </summary>
    /// <typeparam name="T">The produced value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="func">The boundary function to execute.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <param name="nullResultErrorFactory">
    /// Builds the error used when <paramref name="func"/> resolves to <see langword="null"/>. Evaluated only on that
    /// branch and must return a non-null error.
    /// </param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// <c>Ok(value)</c> when the function returns a non-null value; <c>Err(errorFactory(exception))</c> when it throws;
    /// <c>Err(nullResultErrorFactory())</c> when it resolves to <see langword="null"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The token was cancelled or the function threw <see cref="OperationCanceledException"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The boundary function returned a null task, or an error factory returned null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the single asynchronous boundary for callers whose error type is not <see cref="RopError"/>. The
    /// null-result branch is explicit because the library cannot synthesize a domain error for an arbitrary
    /// <typeparamref name="TErr"/>; the <see cref="RopError"/> overload still reports <c>E_NULL_RESULT</c>.
    /// </para>
    /// <para>
    /// Cancellation is checked before the function is invoked and while awaiting, so an already-completed task cannot
    /// bypass a cancelled token. Argument validation surfaces through the returned task because the method is
    /// asynchronous.
    /// </para>
    /// <code><![CDATA[
    /// var charged = await Rop.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         static exception => exception.Message,
    ///         static () => "the provider returned no payment")
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the provider answered,
    /// // Err(the provider returned no payment) when it returned null - a custom error type.
    /// ]]></code>
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Rop.TryAsync is the single asynchronous boundary where any non-cancellation exception becomes a domain error.")]
    public static async Task<Result<T, TErr>> TryAsync<T, TErr>(
        Func<Task<T>> func,
        Func<Exception, TErr> errorFactory,
        Func<TErr> nullResultErrorFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(errorFactory);
        ArgumentNullException.ThrowIfNull(nullResultErrorFactory);
        cancellationToken.ThrowIfCancellationRequested();

        Task<T> pending;
        try
        {
            pending = func();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<T, TErr>(MaterializeException(errorFactory, exception));
        }

        if (pending is null)
        {
            throw new InvalidOperationException("The boundary function returned a null task.");
        }

        T value;
        try
        {
            value = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Err<T, TErr>(MaterializeException(errorFactory, exception));
        }

        return value is null
            ? Err<T, TErr>(MaterializeError(nullResultErrorFactory))
            : Ok<T, TErr>(value);
    }

    /// <summary>
    /// Runs an asynchronous boundary function and converts a thrown exception into an error with the given
    /// <paramref name="code"/>.
    /// </summary>
    /// <typeparam name="T">The produced value type.</typeparam>
    /// <param name="func">The boundary function to execute.</param>
    /// <param name="code">The error code to attach to captured exceptions.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(value)</c> or <c>Err</c> as described by <see cref="TryAsync{T}(Func{Task{T}},Func{Exception,RopError},CancellationToken)"/>.</returns>
    /// <remarks>
    /// The shortest asynchronous boundary for a function that produces a value.
    /// <code><![CDATA[
    /// var charged = await Rop.TryAsync(
    ///         () => OrderFixtures.ChargeAsync(OrderFixtures.Sample),
    ///         "E_PAYMENT")
    ///     .MapOkAsync(static payment => payment.Id);
    ///
    /// // Ok("pay-ord-1001") when the provider answered,
    /// // Err(E_PAYMENT) when the task faulted - the code overload attaches the exception.
    /// ]]></code>
    /// </remarks>
    public static Task<Result<T, RopError>> TryAsync<T>(
        Func<Task<T>> func,
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return TryAsync(func, exception => RopError.FromException(exception, code), cancellationToken);
    }

    /// <summary>
    /// Awaits a sequence of task-wrapped results, short-circuiting on the first error.
    /// </summary>
    /// <typeparam name="T">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="tasks">The tasks to await in order. Enumerated exactly once.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok</c> with all success values in order, or the first <c>Err</c>.</returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the sequence was started or before an element was awaited.
    /// </exception>
    /// <exception cref="InvalidOperationException">The sequence contained a null task.</exception>
    /// <remarks>
    /// <para>
    /// Sequencing is deliberately sequential: results are awaited in enumeration order so the failure that is
    /// reported is deterministic. Start the tasks eagerly at the call site if they should run concurrently.
    /// </para>
    /// <para>
    /// Cancellation is observed by the operation itself, not only by the awaited tasks: an already-completed task
    /// does not short-circuit the token check, so a cancelled token always cancels the sequence.
    /// </para>
    /// <code><![CDATA[
    /// var skus = await Rop.SequenceAsync(
    ///         OrderFixtures.Sample.Lines.Select(static line => Task.FromResult(Rop.Ok(line.Sku))))
    ///     .MapOkAsync(static list => list.Count);
    ///
    /// // Ok(2) when every task produced Ok,
    /// // Err(E_LINE) with the first error - SequenceAsync stops at the first failure.
    /// ]]></code>
    /// </remarks>
    public static async Task<Result<IReadOnlyList<T>, TErr>> SequenceAsync<T, TErr>(
        IEnumerable<Task<Result<T, TErr>>> tasks,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new List<T>();
        foreach (var task in tasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (task is null)
            {
                throw new InvalidOperationException("The sequence contained a null task.");
            }

            var result = await task.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!result.TryGetOk(out var value))
            {
                return Err<IReadOnlyList<T>, TErr>(result.ErrUnsafe);
            }

            values.Add(value);
        }

        return Ok<IReadOnlyList<T>, TErr>(ToReadOnlyList(values));
    }

    /// <summary>
    /// Maps each element to a task-wrapped result and awaits them in order, short-circuiting on the first error.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="TOut">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="source">The source elements. Enumerated exactly once.</param>
    /// <param name="mapper">Maps an element to a task-wrapped result.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok</c> with all success values in order, or the first <c>Err</c>.</returns>
    /// <exception cref="OperationCanceledException">
    /// The token was cancelled before the mapper was invoked for the next element (the mapper is then not called).
    /// </exception>
    /// <exception cref="InvalidOperationException">The mapper returned a null task.</exception>
    /// <remarks>
    /// The mapper is invoked for every element in turn, and the traversal stops at the first failure.
    /// <code><![CDATA[
    /// var skus = await Rop.TraverseAsync(
    ///         OrderFixtures.Sample.Lines,
    ///         static line => Task.FromResult(Rop.Ok(line.Sku)))
    ///     .MapOkAsync(static list => list.Count);
    ///
    /// // Ok(2) when every element mapped successfully,
    /// // Err(E_LINE) with the first error - TraverseAsync stops at the first failure.
    /// ]]></code>
    /// </remarks>
    public static async Task<Result<IReadOnlyList<TOut>, TErr>> TraverseAsync<T, TOut, TErr>(
        IEnumerable<T> source,
        Func<T, Task<Result<TOut, TErr>>> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new List<TOut>();
        foreach (var element in source)
        {
            // Check before invoking the mapper: a cancelled token must not start new work, even when the
            // previous task has already completed.
            cancellationToken.ThrowIfCancellationRequested();
            var pending = mapper(element);
            if (pending is null)
            {
                throw new InvalidOperationException("The mapper returned a null task.");
            }

            var result = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (!result.TryGetOk(out var value))
            {
                return Err<IReadOnlyList<TOut>, TErr>(result.ErrUnsafe);
            }

            values.Add(value);
        }

        return Ok<IReadOnlyList<TOut>, TErr>(ToReadOnlyList(values));
    }
}
