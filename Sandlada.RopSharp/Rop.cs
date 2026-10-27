using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// Static entry point for the <see cref="Result{TOk,TErr}"/> domain: construction, boundary lifting and
/// combining operations that have no receiver.
/// </summary>
/// <remarks>
/// <para>
/// Receiver-based operations (mapping, binding, matching) are extension methods only; this class never
/// duplicates them. The parallel entry point for the <see cref="Option{T}"/> domain is <see cref="Opt"/>.
/// </para>
/// <para>
/// Production code should prefer the single-type-parameter factories <see cref="Ok{TOk}(TOk)"/> and
/// <see cref="Err{TOk}(RopError)"/>, which fix the error type to <see cref="RopError"/>.
/// </para>
/// </remarks>
public static partial class Rop
{
    /// <summary>
    /// Creates a successful result carrying <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TOk">The success value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="value">The success value. Must not be <see langword="null"/>.</param>
    /// <returns>A result on the success track.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Null is never a valid success value. Use <c>Rop.FromNullable</c> or
    /// <see cref="Opt"/> when absence is expected.
    /// </remarks>
    public static Result<TOk, TErr> Ok<TOk, TErr>(TOk value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "A success value must not be null. Use Rop.FromNullable, Rop.Try or Option for missing values.");
        }

        return new Result<TOk, TErr>(value, default!, isOk: true);
    }

    /// <summary>
    /// Creates a successful result carrying <paramref name="value"/> with <see cref="RopError"/> as error type.
    /// </summary>
    /// <typeparam name="TOk">The success value type.</typeparam>
    /// <param name="value">The success value. Must not be <see langword="null"/>.</param>
    /// <returns>A result on the success track.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static Result<TOk, RopError> Ok<TOk>(TOk value) => Ok<TOk, RopError>(value);

    /// <summary>
    /// Creates a failed result carrying <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="TOk">The success value type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="error">The error value. Must not be <see langword="null"/>.</param>
    /// <returns>A result on the failure track.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<TOk, TErr> Err<TOk, TErr>(TErr error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error), "An error value must not be null.");
        }

        return new Result<TOk, TErr>(default!, error, isOk: false);
    }

    /// <summary>
    /// Creates a failed result carrying <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="TOk">The success value type.</typeparam>
    /// <param name="error">The error value. Must not be <see langword="null"/>.</param>
    /// <returns>A result on the failure track.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<TOk, RopError> Err<TOk>(RopError error) => Err<TOk, RopError>(error);

    /// <summary>
    /// Lifts a plain function into a function returning a <see cref="Result{TOk,TErr}"/> on the success track.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="func">The function to lift. Exceptions are not caught.</param>
    /// <returns>A function that always returns <c>Ok</c> for a successful invocation.</returns>
    public static Func<T, Result<TOut, TErr>> Switch<T, TOut, TErr>(Func<T, TOut> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return value => Ok<TOut, TErr>(func(value));
    }

    /// <summary>
    /// Lifts a plain function into a function returning a <see cref="Result{TOk,TErr}"/>, converting thrown
    /// exceptions into errors except for <see cref="OperationCanceledException"/>.
    /// </summary>
    /// <typeparam name="T">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="func">The function to lift.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <returns>A function that never throws for expected failures.</returns>
    public static Func<T, Result<TOut, RopError>> TrySwitch<T, TOut>(
        Func<T, TOut> func,
        Func<Exception, RopError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(errorFactory);
        return value => Try(() => func(value), errorFactory);
    }

    /// <summary>
    /// Runs <paramref name="func"/> and converts any thrown exception into an error, except
    /// <see cref="OperationCanceledException"/> which is always rethrown.
    /// </summary>
    /// <typeparam name="T">The produced value type.</typeparam>
    /// <param name="func">The boundary function to execute.</param>
    /// <param name="errorFactory">Maps a caught exception to an error. Must return a non-null error.</param>
    /// <returns>
    /// <c>Ok(value)</c> when the function returns a non-null value; <c>Err(errorFactory(exception))</c> when it throws;
    /// <c>Err</c> with code <c>E_NULL_RESULT</c> when it returns <see langword="null"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The function threw <see cref="OperationCanceledException"/>.</exception>
    /// <remarks>
    /// This is the only supported place where exceptions become domain values. Never wrap mapper/binder
    /// bodies in a local <c>try/catch</c>; lift the boundary instead.
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Rop.Try is the single boundary where any non-cancellation exception becomes a domain error.")]
    public static Result<T, RopError> Try<T>(Func<T> func, Func<Exception, RopError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(errorFactory);

        try
        {
            var value = func();
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
    /// Runs <paramref name="func"/> and converts any thrown exception into an error with the given
    /// <paramref name="code"/>, except <see cref="OperationCanceledException"/> which is always rethrown.
    /// </summary>
    /// <typeparam name="T">The produced value type.</typeparam>
    /// <param name="func">The boundary function to execute.</param>
    /// <param name="code">The error code to attach to captured exceptions.</param>
    /// <returns><c>Ok(value)</c> or <c>Err</c> as described by <see cref="Try{T}(Func{T},Func{Exception,RopError})"/>.</returns>
    public static Result<T, RopError> Try<T>(Func<T> func, string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return Try(func, exception => RopError.FromException(exception, code));
    }

    /// <summary>
    /// Converts a possibly-null reference into a <see cref="Result{TOk,TErr}"/> where null is a failure.
    /// </summary>
    /// <typeparam name="TOk">The value type.</typeparam>
    /// <param name="value">The possibly-null value.</param>
    /// <param name="errorFactory">Creates the error when <paramref name="value"/> is <see langword="null"/>.</param>
    /// <returns><c>Ok(value)</c> when non-null; otherwise <c>Err(errorFactory())</c>.</returns>
    /// <remarks>The factory is evaluated only when the value is <see langword="null"/>.</remarks>
    public static Result<TOk, RopError> FromNullable<TOk>(TOk? value, Func<RopError> errorFactory)
        where TOk : class
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return value is null
            ? Err<TOk, RopError>(Materialize(errorFactory))
            : Ok<TOk, RopError>(value);
    }

    /// <summary>
    /// Converts a possibly-null value type into a <see cref="Result{TOk,TErr}"/> where null is a failure.
    /// </summary>
    /// <typeparam name="TOk">The underlying value type.</typeparam>
    /// <param name="value">The possibly-null value.</param>
    /// <param name="errorFactory">Creates the error when <paramref name="value"/> has no value.</param>
    /// <returns><c>Ok(value)</c> when it has a value; otherwise <c>Err(errorFactory())</c>.</returns>
    /// <remarks>
    /// Named distinctly from <c>Rop.FromNullable</c> because C# does not permit two
    /// overloads that differ only by generic constraints.
    /// </remarks>
    public static Result<TOk, RopError> FromNullableValue<TOk>(TOk? value, Func<RopError> errorFactory)
        where TOk : struct
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return value is null
            ? Err<TOk, RopError>(Materialize(errorFactory))
            : Ok<TOk, RopError>(value.GetValueOrDefault());
    }

    internal static RopError Materialize(Func<RopError> factory)
    {
        var error = factory();
        if (error is null)
        {
            throw new InvalidOperationException("The error factory returned null. A RopError is required.");
        }

        return error;
    }

    internal static Result<TOk, TErr> OkUnchecked<TOk, TErr>(TOk value) =>
        new(value, default!, isOk: true);

    private static RopError MaterializeException(Func<Exception, RopError> factory, Exception exception)
    {
        var error = factory(exception);
        if (error is null)
        {
            throw new InvalidOperationException("The error factory returned null. A RopError is required.");
        }

        return error;
    }

    private static RopError NullResultError() => RopError.Create(
        "E_NULL_RESULT",
        "The boundary function returned null. A successful result requires a non-null value.");
}
