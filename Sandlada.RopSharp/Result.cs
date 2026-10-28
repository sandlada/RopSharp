using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// An immutable value that is either a success (<c>Ok</c>) carrying <typeparamref name="TOk"/> or a
/// failure (<c>Err</c>) carrying <typeparamref name="TErr"/>.
/// </summary>
/// <typeparam name="TOk">The success value type.</typeparam>
/// <typeparam name="TErr">The error value type. Production code should use <see cref="RopError"/>.</typeparam>
/// <remarks>
/// <para>
/// Create values through <see cref="Rop.Ok{TOk,TErr}(TOk)"/> and <see cref="Rop.Err{TOk,TErr}(TErr)"/>.
/// The parameterless <see langword="default"/> value is not a valid result: it reports <see cref="IsErr"/> but
/// carries no error. Reading that error through <see cref="TryGetErr"/> or a terminal/error-track combinator
/// throws <see cref="InvalidOperationException"/> instead of exposing a null error.
/// </para>
/// <para>
/// Instances are immutable and safe to share across threads. Expected failures are values, never exceptions.
/// </para>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Result<TOk, TErr> : IEquatable<Result<TOk, TErr>>
{
    private readonly TOk _ok;
    private readonly TErr _err;
    private readonly bool _isOk;

    internal Result(TOk ok, TErr err, bool isOk)
    {
        _ok = ok;
        _err = err;
        _isOk = isOk;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is on the success track.
    /// </summary>
    public bool IsOk => _isOk;

    /// <summary>
    /// Gets a value indicating whether this instance is on the failure track.
    /// </summary>
    public bool IsErr => !_isOk;

    /// <summary>
    /// Attempts to read the success value without throwing.
    /// </summary>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, the success value; otherwise the default of
    /// <typeparamref name="TOk"/> (which may be <see langword="null"/>).
    /// </param>
    /// <returns><see langword="true"/> when the result is <c>Ok</c>; otherwise <see langword="false"/>.</returns>
    public bool TryGetOk([MaybeNullWhen(false)] out TOk value)
    {
        value = _ok;
        return _isOk;
    }

    /// <summary>
    /// Attempts to read the error value without throwing for a valid result.
    /// </summary>
    /// <param name="error">
    /// When this method returns <see langword="true"/>, the error value; otherwise the default of
    /// <typeparamref name="TErr"/> (which may be <see langword="null"/>).
    /// </param>
    /// <returns><see langword="true"/> when the result is <c>Err</c>; otherwise <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// The result is an uninitialized <see langword="default"/> value. The default is not a valid result;
    /// construct values with <see cref="Rop.Ok{TOk,TErr}(TOk)"/> or <see cref="Rop.Err{TOk,TErr}(TErr)"/>.
    /// </exception>
    /// <remarks>
    /// An uninitialized default reports <see cref="IsErr"/> as <see langword="true"/> so failure routing cannot
    /// mistake it for a success, but it carries no error. This probe therefore faults instead of returning
    /// <see langword="true"/> with a <see langword="null"/> error. The exception is only for that invalid state;
    /// an ordinary <c>Err</c> never throws.
    /// </remarks>
    public bool TryGetErr([MaybeNullWhen(false)] out TErr error)
    {
        ThrowIfUninitialized();
        error = _err;
        return !_isOk;
    }

    /// <summary>
    /// Returns the success value when present, or the default of <typeparamref name="TOk"/> when on the failure track.
    /// </summary>
    /// <returns>The success value, or <see langword="default"/>.</returns>
    /// <remarks>Prefer <see cref="ResultExtensions.Match{TOk,TErr,T}(Result{TOk,TErr},Func{TOk,T},Func{TErr,T})"/> for production exits.</remarks>
    [return: MaybeNull]
    public TOk OkOrDefault() => _ok;

    internal TOk OkUnsafe => _ok;

    /// <summary>
    /// Gets a value indicating whether this instance was produced by a factory. An uninitialized default has no
    /// error value; non-nullable value-type errors cannot be distinguished from a legitimate default error.
    /// </summary>
    internal bool IsInitialized => _isOk || _err is not null || default(TErr) is not null;

    internal TErr ErrUnsafe
    {
        get
        {
            ThrowIfUninitialized();
            return _err;
        }
    }

    internal void ThrowIfUninitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "The result is an uninitialized default value; construct results with Rop.Ok or Rop.Err.");
        }
    }

    /// <inheritdoc />
    public bool Equals(Result<TOk, TErr> other)
    {
        if (_isOk != other._isOk)
        {
            return false;
        }

        return _isOk
            ? EqualityComparer<TOk>.Default.Equals(_ok, other._ok)
            : EqualityComparer<TErr>.Default.Equals(_err, other._err);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result<TOk, TErr> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        _isOk
            ? HashCode.Combine(true, _ok is null ? 0 : EqualityComparer<TOk>.Default.GetHashCode(_ok))
            : HashCode.Combine(false, _err is null ? 0 : EqualityComparer<TErr>.Default.GetHashCode(_err));

    /// <inheritdoc />
    public override string ToString() => _isOk ? $"Ok({_ok})" : $"Err({_err})";

    /// <summary>Determines whether two results are equal (success values or error values must be equal).</summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when both are on the same track with equal payloads.</returns>
    public static bool operator ==(Result<TOk, TErr> left, Result<TOk, TErr> right) => left.Equals(right);

    /// <summary>Determines whether two results are different.</summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when they differ.</returns>
    public static bool operator !=(Result<TOk, TErr> left, Result<TOk, TErr> right) => !left.Equals(right);
}
