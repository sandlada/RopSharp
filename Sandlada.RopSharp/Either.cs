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
/// <para>
/// An either is consumed by projecting both branches into one value with <see cref="Match{TResult}(Func{TLeft,TResult},Func{TRight,TResult})"/>,
/// or by crossing into the result world with <see cref="ToResult(Func{TLeft,RopError})"/> when the left branch
/// turns out to carry a reason after all.
/// </para>
/// <code><![CDATA[
/// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
///     .MapRight(static payment => payment.Id);
///
/// // Right("pay-ord-1001") when the payment was captured,
/// // Left(reason) when the outcome is a refund - both branches are ordinary values here.
/// ]]></code>
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
    /// <remarks>
    /// <para>
    /// The flag tells you which branch carries the payload without reading it. Use it before a
    /// <see cref="TryGetLeft"/> or <see cref="TryGetRight"/> when only one of the two branches is interesting.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapRight(static payment => payment.Id);
    /// var refunded = outcome.IsLeft;
    ///
    /// // Right("pay-ord-1001") makes IsLeft false,
    /// // Left(reason) makes IsLeft true - the branch decides which value is present.
    /// ]]></code>
    /// </remarks>
    public bool IsLeft => _isLeft;

    /// <summary>Gets a value indicating whether this instance is on the right branch.</summary>
    /// <remarks>
    /// <para>
    /// This is the exact complement of <see cref="IsLeft"/>. It is the signal that the "expected" branch of the
    /// union was taken.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapRight(static payment => payment.Id);
    /// var captured = outcome.IsRight;
    ///
    /// // Right("pay-ord-1001") makes IsRight true,
    /// // Left(reason) makes IsRight false - the branch decides which value is present.
    /// ]]></code>
    /// </remarks>
    public bool IsRight => !_isLeft;

    /// <summary>
    /// Attempts to read the left value without throwing.
    /// </summary>
    /// <param name="value">When this method returns <see langword="true"/>, the left value; otherwise the default.</param>
    /// <returns><see langword="true"/> when this instance is left; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The probe never throws. On the right branch the output stays at its default, so the return value has to
    /// be checked before the value is used.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Left<string, Payment>("the customer asked for a refund");
    /// if (outcome.TryGetLeft(out var reason))
    /// {
    ///     var note = reason;
    /// }
    ///
    /// // Left(reason) is read into 'reason' on the left branch,
    /// // Right(payment) leaves 'reason' at its default - TryGetLeft never throws.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The probe never throws. On the left branch the output stays at its default, so the return value has to be
    /// checked before the value is used.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// if (outcome.TryGetRight(out var payment))
    /// {
    ///     var receipt = payment.Id;
    /// }
    ///
    /// // Right(payment) is read into 'payment' on the right branch,
    /// // Left(reason) leaves 'payment' at its default - TryGetRight never throws.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// <c>Match</c> is the total exit of an either: it handles both branches, so no payload is ever left
    /// behind. The two delegates are named arguments in practice, which keeps the projection readable.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var label = outcome.Match(
    ///     left: static reason => $"refund: {reason}",
    ///     right: static payment => $"captured: {payment.Id}");
    ///
    /// // "captured: pay-ord-1001" for Right(payment),
    /// // "refund: ..." for Left(reason) - Match handles both branches.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The mapper runs on the left branch only, so it may assume the left payload. It is the way to reshape the
    /// "unexpected" side of the union without touching the "expected" side.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Left<string, Payment>("the customer asked for a refund");
    /// var mapped = outcome.MapLeft(static reason => reason.Length);
    ///
    /// // Left(35) when the left branch is taken,
    /// // Right(payment) keeps the original right value - the mapper never runs there.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The mapper runs on the right branch only, so it may assume the right payload. It is the way to reshape the
    /// "expected" side of the union without touching the "unexpected" side.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var mapped = outcome.MapRight(static payment => payment.Id);
    ///
    /// // Right("pay-ord-1001") when the right branch is taken,
    /// // Left(reason) keeps the original left value - the mapper never runs there.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// <c>Swap</c> exchanges the two positions. It is the quickest way to reuse a union whose branches were
    /// declared in the opposite order, and it keeps the payload of whichever branch was taken.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var swapped = outcome.Swap().MapRight(static reason => reason.Length);
    ///
    /// // Right(payment) becomes Left(payment),
    /// // Left(reason) becomes Right(reason) - Swap only exchanges the two positions.
    /// ]]></code>
    /// </remarks>
    public Either<TRight, TLeft> Swap() => _isLeft
        ? Either.Right<TRight, TLeft>(_left!)
        : Either.Left<TRight, TLeft>(_right!);

    /// <summary>
    /// Converts the left branch into an error and keeps the right branch as a success.
    /// </summary>
    /// <param name="errorFactory">Builds the error from a left value. Evaluated only on the left branch.</param>
    /// <returns><c>Err(errorFactory(left))</c> or <c>Ok(right)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// Both <c>ToResult</c> overloads reject a null error value the same way: with the library-wide
    /// <see cref="InvalidOperationException"/> rather than the argument exception raised by <see cref="Rop.Err{TOk,TErr}"/>.
    /// </para>
    /// <para>
    /// The crossing is explicit: the left payload becomes the error, and the factory decides how much of it
    /// survives.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Left<string, Payment>("the customer asked for a refund");
    /// var placed = outcome.ToResult(static reason => RopError.Create("E_REFUND", reason));
    ///
    /// // Err(E_REFUND) on the left branch,
    /// // Ok(payment) on the right branch - the left value becomes the error.
    /// ]]></code>
    /// </remarks>
    public Result<TRight, RopError> ToResult(Func<TLeft, RopError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return _isLeft
            ? Rop.Err<TRight>(Rop.RequireError(errorFactory(_left)))
            : Rop.Ok<TRight>(_right);
    }

    /// <summary>
    /// Converts the left branch into a caller-defined error and keeps the right branch as a success.
    /// </summary>
    /// <typeparam name="TErr">The error value type.</typeparam>
    /// <param name="errorFactory">Builds the error from a left value. Evaluated only on the left branch.</param>
    /// <returns><c>Err(errorFactory(left))</c> or <c>Ok(right)</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errorFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// Use this overload when the caller's error system is not <see cref="RopError"/>. The <see cref="RopError"/>
    /// overload remains the default.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Left<string, Payment>("the customer asked for a refund");
    /// var placed = outcome.ToResult<string>(static reason => reason);
    ///
    /// // Err("the customer asked for a refund") on the left branch,
    /// // Ok(payment) on the right branch - a custom error type keeps the reason verbatim.
    /// ]]></code>
    /// </remarks>
    public Result<TRight, TErr> ToResult<TErr>(Func<TLeft, TErr> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(errorFactory);
        return _isLeft
            ? Rop.Err<TRight, TErr>(Rop.RequireError(errorFactory(_left)))
            : Rop.Ok<TRight, TErr>(_right);
    }

    /// <summary>
    /// Converts the right branch into a present value, discarding the left branch.
    /// </summary>
    /// <returns><c>Some(right)</c> on the right branch; <c>None</c> on the left branch.</returns>
    /// <remarks>
    /// <para>
    /// The left value is dropped and cannot be recovered. Use <see cref="ToResult"/> when the left value carries a
    /// reason that must survive the crossing, and bridge to <see cref="Option{T}"/> only when the branch itself is
    /// the answer.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var receipt = outcome.ToOption().MapSome(static payment => payment.Id);
    ///
    /// // Some("pay-ord-1001") on the right branch,
    /// // None on the left branch - the left value is dropped, not converted.
    /// ]]></code>
    /// </remarks>
    public Option<TRight> ToOption() => _isLeft ? Opt.None<TRight>() : Opt.Some(_right);

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Two either values are equal when they sit on the same branch and carry equal payloads. This is what lets
    /// an either be used as a dictionary key or compared inside a test.
    /// </para>
    /// <code><![CDATA[
    /// var first = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var second = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var same = first.Equals(second);
    ///
    /// // Right(payment) equals Right(payment) because the payloads are equal,
    /// // Left(reason) equals Left(reason) only when the two reasons are equal.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The boxed overload defers to the strongly typed comparison, so an either stored in an
    /// <see cref="object"/> still compares by value.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var same = outcome.Equals((object)outcome);
    ///
    /// // Right(payment) equals its own boxed copy,
    /// // Left(reason) equals its own boxed copy - the boxed form compares by value.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is Either<TLeft, TRight> other && Equals(other);

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The hash code is derived from the payload of the branch the either sits on, so equal values always share
    /// a hash code.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapRight(static payment => payment.Id);
    /// var bucket = outcome.GetHashCode() % 16;
    ///
    /// // Right("pay-ord-1001") hashes by its payload,
    /// // Left(reason) hashes by its reason - equal values land in the same bucket.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() => _isLeft
        ? HashCode.Combine(true, _left is null ? 0 : EqualityComparer<TLeft>.Default.GetHashCode(_left))
        : HashCode.Combine(false, _right is null ? 0 : EqualityComparer<TRight>.Default.GetHashCode(_right));

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The text is a debugging aid, not a wire format. Branch on <see cref="IsLeft"/> and read the payload through
    /// the combinators instead of parsing this string.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapRight(static payment => payment.Id);
    /// var text = outcome.ToString();
    ///
    /// // Right("pay-ord-1001") on the right branch,
    /// // Left(reason) on the left branch - the text is meant for logs, not for branching.
    /// ]]></code>
    /// </remarks>
    public override string ToString() => _isLeft ? $"Left({_left})" : $"Right({_right})";

    /// <summary>Determines whether two either values are equal.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true"/> when both are on the same branch with equal payloads.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of <see cref="Equals(Either{TLeft,TRight})"/> and is what a cache
    /// lookup or a test assertion naturally reaches for.
    /// </para>
    /// <code><![CDATA[
    /// var first = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapRight(static payment => payment.Id);
    /// var second = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapRight(static payment => payment.Id);
    /// var same = first == second;
    ///
    /// // Right(payment) == Right(payment) because the payloads are equal,
    /// // Left(reason) == Left(reason) only when the two reasons are equal.
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(Either<TLeft, TRight> left, Either<TLeft, TRight> right) => left.Equals(right);

    /// <summary>Determines whether two either values differ.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true"/> when they differ.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of the negated equality, and is how an either is compared against a
    /// known expected value.
    /// </para>
    /// <code><![CDATA[
    /// var first = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample));
    /// var second = Either.Left<string, Payment>("the customer asked for a refund");
    /// var differs = first != second;
    ///
    /// // Right(payment) != Left(reason) because the branches differ,
    /// // Right(a) != Right(b) as soon as the two payloads are not equal.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(Either<TLeft, TRight> left, Either<TLeft, TRight> right) => !left.Equals(right);
}

/// <summary>
/// Construction entry point for <see cref="Either{TLeft,TRight}"/>.
/// </summary>
/// <remarks>
/// <para>
/// The two factories are the only way to create an either. Naming the branch at the construction site is what
/// makes the union readable at the call site later on.
/// </para>
/// <code><![CDATA[
/// var outcome = Either.Left<string, Payment>("the customer asked for a refund")
///     .MapRight(static payment => payment.Id);
///
/// // Left(reason) on the left branch,
/// // Right(payment) on the right branch - Either.Left and Either.Right are the only two constructors.
/// ]]></code>
/// </remarks>
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
    /// <remarks>
    /// <para>
    /// The left branch is the "unexpected but still a value" side of the union. A null payload is rejected,
    /// because an either always carries a real value on its branch.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Left<string, Payment>("the customer asked for a refund")
    ///     .MapRight(static payment => payment.Id);
    ///
    /// // Left(reason) is created on the left branch,
    /// // Right(payment) is never constructed - the two factories are the only entry points.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The right branch is the "expected" side of the union. A null payload is rejected for the same reason as
    /// on the left branch: an either always carries a real value.
    /// </para>
    /// <code><![CDATA[
    /// var outcome = Either.Right<string, Payment>(OrderFixtures.Charge(OrderFixtures.Sample))
    ///     .MapRight(static payment => payment.Id);
    ///
    /// // Right(payment) is created on the right branch,
    /// // Left(reason) is never constructed - the two factories are the only entry points.
    /// ]]></code>
    /// </remarks>
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "A right value must not be null.");
        }

        return new Either<TLeft, TRight>(default!, value, isLeft: false);
    }
}
