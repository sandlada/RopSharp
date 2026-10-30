namespace Sandlada.RopSharp;

/// <summary>
/// Explicit bridges from <see cref="Result{TOk,TErr}"/> and <see cref="Option{T}"/> into
/// <see cref="Either{TLeft,TRight}"/>.
/// </summary>
/// <remarks>
/// <para>
/// The right branch is always the "value" branch and the left branch is always the "reason or absence" branch, so the
/// crossing never changes which side carries the payload. The reverse crossings are
/// <see cref="Either{TLeft,TRight}.ToResult"/> (left to <see cref="RopError"/>) and
/// <see cref="Either{TLeft,TRight}.ToOption"/> (right to <see cref="Option{T}"/>, discarding the left value).
/// </para>
/// <para>
/// All conversions are explicit: the library never switches models implicitly (ADR 0001, D5).
/// </para>
/// <code><![CDATA[
/// var outcome = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
///     .ToEither()
///     .MapRight(static payment => payment.Id);
///
/// // Right("pay-ord-1001") when the charge succeeded,
/// // Left(E_CHARGE) when it failed - the crossing keeps the payload on the right.
/// ]]></code>
/// </remarks>
public static class EitherBridgeExtensions
{
    /// <summary>
    /// Converts a result into an either: success becomes <c>Right</c> and failure becomes <c>Left</c>.
    /// </summary>
    /// <typeparam name="TOk">The success type.</typeparam>
    /// <typeparam name="TErr">The error type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <returns><c>Right(value)</c> for a success; <c>Left(error)</c> for an error.</returns>
    /// <exception cref="ArgumentNullException">
    /// The success track carries <see langword="null"/> (possible after a nullable mapping), which cannot be a
    /// <c>Right</c> value.
    /// </exception>
    /// <remarks>
    /// The mapping is lossless in both directions; <see cref="Either{TLeft,TRight}.ToResult"/> recovers the original
    /// result when the left branch is converted back with an identity error factory.
    /// <code><![CDATA[
    /// var outcome = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .ToEither()
    ///     .MapRight(static payment => payment.Id);
    ///
    /// // Right("pay-ord-1001") when the charge succeeded,
    /// // Left(E_CHARGE) when it failed - the crossing keeps the payload on the right.
    /// ]]></code>
    /// </remarks>
    public static Either<TErr, TOk> ToEither<TOk, TErr>(this Result<TOk, TErr> result) =>
        result.TryGetOk(out var value)
            ? Either.Right<TErr, TOk>(value)
            : Either.Left<TErr, TOk>(result.ErrUnsafe);

    /// <summary>
    /// Converts an option into an either, using the supplied left value for <c>None</c>.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="left">The left value used when the option is empty. Must not be <see langword="null"/>.</param>
    /// <returns><c>Right(value)</c> for <c>Some</c>; <c>Left(left)</c> for <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The left value is validated eagerly, even when the option is <c>Some</c>; use the factory overload when
    /// building it is expensive.
    /// <code><![CDATA[
    /// var outcome = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
    ///     .ToEither("no shipping address on file")
    ///     .MapRight(static shipping => shipping.City);
    ///
    /// // Right("London") when the address is present,
    /// // Left(no shipping address on file) when it is missing - the crossing is explicit.
    /// ]]></code>
    /// </remarks>
    public static Either<TLeft, T> ToEither<TLeft, T>(this Option<T> option, TLeft left)
    {
        ArgumentGuards.ThrowIfNull(left, nameof(left));
        return option.TryGetValue(out var value)
            ? Either.Right<TLeft, T>(value)
            : Either.Left<TLeft, T>(left);
    }

    /// <summary>
    /// Converts an option into an either, building the left value lazily for <c>None</c>.
    /// </summary>
    /// <typeparam name="TLeft">The left type.</typeparam>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="leftFactory">Builds the left value. Evaluated only for <c>None</c>.</param>
    /// <returns><c>Right(value)</c> for <c>Some</c>; <c>Left(leftFactory())</c> for <c>None</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="leftFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The factory returned <see langword="null"/>.</exception>
    /// <remarks>
    /// The factory runs only on the empty track, so an expensive left value costs nothing when the option carries
    /// a value.
    /// <code><![CDATA[
    /// var outcome = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .ToEither(static () => "no shipping address on file")
    ///     .MapRight(static shipping => shipping.City);
    ///
    /// // Right(city) when the address is present,
    /// // Left(no shipping address on file) when it is missing - the factory runs only for None.
    /// ]]></code>
    /// </remarks>
    public static Either<TLeft, T> ToEither<TLeft, T>(this Option<T> option, Func<TLeft> leftFactory)
    {
        ArgumentNullException.ThrowIfNull(leftFactory);
        return option.TryGetValue(out var value)
            ? Either.Right<TLeft, T>(value)
            : Either.Left<TLeft, T>(MaterializeLeft(leftFactory));
    }

    private static TLeft MaterializeLeft<TLeft>(Func<TLeft> leftFactory)
    {
        var left = leftFactory();
        if (left is null)
        {
            throw new InvalidOperationException(
                "The left factory returned null. An Either left value must not be null.");
        }

        return left;
    }
}
