using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// Static entry point for the <see cref="Result{TOk,TErr}"/> domain: asynchronous boundaries and collection sequencing.
/// </summary>
public static partial class Rop
{
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
    /// Runs an asynchronous boundary function and converts a thrown exception into an error with the given
    /// <paramref name="code"/>.
    /// </summary>
    /// <typeparam name="T">The produced value type.</typeparam>
    /// <param name="func">The boundary function to execute.</param>
    /// <param name="code">The error code to attach to captured exceptions.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns><c>Ok(value)</c> or <c>Err</c> as described by <see cref="TryAsync{T}(Func{Task{T}},Func{Exception,RopError},CancellationToken)"/>.</returns>
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
