using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// An immutable value that is either a <c>Left</c> or a <c>Right</c>.
/// </summary>
/// <typeparam name="TLeft">The left value type.</typeparam>
/// <typeparam name="TRight">The right value type.</typeparam>
/// <remarks>
/// <para>
/// <see cref="Either{TLeft,TRight}"/> is the general two-branch union. Use <see cref="Result{TOk,TErr}"/> when the
/// two branches mean success and failure; use <c>Either</c> when both branches are ordinary values (for example a
/// payment result or a refund result) and picking a convention is misleading.
/// </para>
/// <para>
/// Instances are immutable and safe to share across threads. The parameterless <see langword="default"/> value is
/// undefined; create values through <see cref="Either.Left{TLeft,TRight}(TLeft)"/> and
/// <see cref="Either.Right{TLeft,TRight}(TRight)"/>.
/// </para>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Either<TLeft, TRight> : IEquatable<Either<TLeft, TRight>>
{
    private readonly TLeft _left;
    private readonly TRight _right;
    private readonly bool _isLeft;

    internal Either(TLeft left, TRight right, bool isLeft)
    {
        _left = left;
        _right = right;
        _isLeft = isLeft;
    }

    /// <summary>Gets a value indicating whether this instance is on the left branch.</summary>
    public bool IsLeft => _isLeft;

    /// <summary>Gets a value indicating whether this instance is on the right branch.</summary>
    public bool IsRight => !_isLeft;

    /// <summary>
    /// Attempts to read the left value without throwing.
    /// </summary>
    /// <param name="value">When this method returns <see langword="true"/>, the left value; otherwise the default.</param>
    /// <returns><see langword="true"/> when this instance is left; otherwise <see langword="false"/>.</returns>
    public bool TryGetLeft([MaybeNullWhen(false)] out TLeft value)
    {
        value = _left;
        return _isLeft;
    }

    /// <summary>
    /// Attempts to read the right value without throwing.
    /// </summary>
    /// <param name="value">When this method returns <see langword="true"/>, the right value; otherwise the default.</param>
    /// <returns><see langword="true"/> when this instance is right; otherwise <see langword="false"/>.</returns>
    public bool TryGetRight([MaybeNullWhen(false)] out TRight value)
    {
        value = _right;
        return !_isLeft;
    }

    /// <summary>
    /// Projects both branches into a single value.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="left">Projects a left value.</param>
    /// <param name="right">Projects a right value.</param>
    /// <returns>The projected value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
    public TResult Match<TResult>(Func<TLeft, TResult> left, Func<TRight, TResult> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return _isLeft ? left(_left) : right(_right);
    }

    /// <summary>
    /// Maps the left value, leaving a right value untouched.
    /// </summary>
    /// <typeparam name="TLeftOut">The mapped left type.</typeparam>
    /// <param name="mapper">Maps the left value. Not evaluated on the right branch.</param>
    /// <returns><c>Left(mapper(value))</c> or the original right value.</returns>
    public Either<TLeftOut, TRight> MapLeft<TLeftOut>(Func<TLeft, TLeftOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return _isLeft
            ? Either.Left<TLeftOut, TRight>(mapper(_left))
            : Either.Right<TLeftOut, TRight>(_right);
    }

    /// <summary>
    /// Maps the right value, leaving a left value untouched.
    /// </summary>
    /// <typeparam name="TRightOut">The mapped right type.</typeparam>
    /// <param name="mapper">Maps the right value. Not evaluated on the left branch.</param>
    /// <returns><c>Right(mapper(value))</c> or the original left value.</returns>
    public Either<TLeft, TRightOut> MapRight<TRightOut>(Func<TRight, TRightOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return _isLeft
            ? Either.Left<TLeft, TRightOut>(_left)
            : Either.Right<TLeft, TRightOut>(mapper(_right));
    }

    /// <summary>
    /// Swaps the branches.
    /// </summary>
    /// <returns><c>Right(value)</c> for a left value and <c>Left(value)</c> for a right value.</returns>
    public Either<TRight, TLeft> Swap() => _isLeft
        ? Either.Right<TRight, TLeft>(_left!)
        : Either.Left<TRight, TLeft>(_right!);

    /// <summary>
    /// Converts the left branch into an error and keeps the right branch as a success.
    /// </summary>
    /// <param name="errorFactory">Builds the error from a left value. Evaluated only on the left branch.</param>
    /// <returns><c>Err(errorFactory(left))</c> or <c>Ok(right)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    public Result<TRight, RopError> ToResult(Func<TLeft, RopError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return _isLeft
            ? Rop.Err<TRight>(errorFactory(_left))
            : Rop.Ok<TRight>(_right);
    }

    /// <inheritdoc />
    public bool Equals(Either<TLeft, TRight> other)
    {
        if (_isLeft != other._isLeft)
        {
            return false;
        }

        return _isLeft
            ? EqualityComparer<TLeft>.Default.Equals(_left, other._left)
            : EqualityComparer<TRight>.Default.Equals(_right, other._right);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Either<TLeft, TRight> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _isLeft
        ? HashCode.Combine(true, _left is null ? 0 : EqualityComparer<TLeft>.Default.GetHashCode(_left))
        : HashCode.Combine(false, _right is null ? 0 : EqualityComparer<TRight>.Default.GetHashCode(_right));

    /// <inheritdoc />
    public override string ToString() => _isLeft ? $"Left({_left})" : $"Right({_right})";

    /// <summary>Determines whether two either values are equal.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true"/> when both are on the same branch with equal payloads.</returns>
    public static bool operator ==(Either<TLeft, TRight> left, Either<TLeft, TRight> right) => left.Equals(right);

    /// <summary>Determines whether two either values differ.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true"/> when they differ.</returns>
    public static bool operator !=(Either<TLeft, TRight> left, Either<TLeft, TRight> right) => !left.Equals(right);
}

/// <summary>
/// Construction entry point for <see cref="Either{TLeft,TRight}"/>.
/// </summary>
public static class Either
{
    /// <summary>
    /// Creates a left value.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="TRight">The right type.</typeparam>
    /// <param name="value">The left value. Must not be <see langword="null"/>.</param>
    /// <returns>An either on the left branch.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "A left value must not be null.");
        }

        return new Either<TLeft, TRight>(value, default!, isLeft: true);
    }

    /// <summary>
    /// Creates a right value.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="TRight">The right type.</typeparam>
    /// <param name="value">The right value. Must not be <see langword="null"/>.</param>
    /// <returns>An either on the right branch.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "A right value must not be null.");
        }

        return new Either<TLeft, TRight>(default!, value, isLeft: false);
    }
}
