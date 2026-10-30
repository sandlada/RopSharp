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
/// <para>
/// A result is a value in a pipeline, not a value in isolation. It is created at a boundary, reshaped by the
/// success track, recovered or reported by the failure track, and finally consumed by a terminal such as
/// <see cref="ResultExtensions.Match{TOk,TErr,T}(Result{TOk,TErr},Func{TOk,T},Func{TErr,T})"/>.
/// </para>
/// <code><![CDATA[
/// var placed = Rop.Ok(OrderFixtures.Sample)
///     .BindOk(static order => Rop.Ok(order.Total))
///     .MapOk(static total => total.ToString("0.00", CultureInfo.InvariantCulture));
///
/// // Ok("44.00") when the order could be priced,
/// // Err(E_PRICE) when the pricing boundary failed - the chain short-circuits on the first failure.
/// ]]></code>
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
    /// <remarks>
    /// <para>
    /// The flag is the cheapest way to branch without allocating a delegate. It is a question about the track,
    /// not about the payload: to read the payload, use <see cref="TryGetOk"/>.
    /// </para>
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE");
    /// var succeeded = charged.IsOk;
    ///
    /// // Ok(payment) makes IsOk true,
    /// // Err(E_CHARGE) makes IsOk false - the error is still reachable through TryGetErr.
    /// ]]></code>
    /// </remarks>
    public bool IsOk => _isOk;

    /// <summary>
    /// Gets a value indicating whether this instance is on the failure track.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the exact complement of <see cref="IsOk"/>: a success reports <see langword="false"/> here and a
    /// failure reports <see langword="true"/>. It never inspects the error value itself.
    /// </para>
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE");
    /// var failed = charged.IsErr;
    ///
    /// // Err(E_CHARGE) makes IsErr true,
    /// // Ok(payment) makes IsErr false - the error code is read with TryGetErr.
    /// ]]></code>
    /// </remarks>
    public bool IsErr => !_isOk;

    /// <summary>
    /// Attempts to read the success value without throwing.
    /// </summary>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, the success value; otherwise the default of
    /// <typeparamref name="TOk"/> (which may be <see langword="null"/>).
    /// </param>
    /// <returns><see langword="true"/> when the result is <c>Ok</c>; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The probe never throws, which makes it the safe way to look at a result before deciding how to continue.
    /// On the failure track the output stays at its default, so always check the return value before using it.
    /// </para>
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE");
    /// if (charged.TryGetOk(out var payment))
    /// {
    ///     var receipt = payment.Id;
    /// }
    ///
    /// // Ok(payment) is read into 'payment' when the charge succeeded,
    /// // Err(E_CHARGE) leaves 'payment' at its default - TryGetOk never throws.
    /// ]]></code>
    /// </remarks>
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
    /// <para>
    /// An uninitialized default reports <see cref="IsErr"/> as <see langword="true"/> so failure routing cannot
    /// mistake it for a success, but it carries no error. This probe therefore faults instead of returning
    /// <see langword="true"/> with a <see langword="null"/> error. The exception is only for that invalid state;
    /// an ordinary <c>Err</c> never throws.
    /// </para>
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Declined), "E_CHARGE");
    /// if (charged.TryGetErr(out var error))
    /// {
    ///     var code = error.Code;
    /// }
    ///
    /// // Err(E_CHARGE) is read into 'error' when the charge failed,
    /// // Ok(payment) leaves 'error' at its default - TryGetErr never throws for a real result.
    /// ]]></code>
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
    /// <remarks>
    /// <para>
    /// Prefer <see cref="ResultExtensions.Match{TOk,TErr,T}(Result{TOk,TErr},Func{TOk,T},Func{TErr,T})"/> for
    /// production exits. This member is the cheap escape hatch for a value that has a sensible default, such as
    /// a zero amount in a report.
    /// </para>
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE");
    /// var amount = charged.MapOk(static payment => payment.Amount).OkOrDefault();
    ///
    /// // Ok(44.00) yields 44.00,
    /// // Err(E_CHARGE) yields 0m - the error is discarded, so use it only where a default is acceptable.
    /// ]]></code>
    /// </remarks>
    [return: MaybeNull]
    public TOk OkOrDefault() => _ok;

    internal TOk OkUnsafe => _ok;

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
    /// <remarks>
    /// <para>
    /// Two results are equal when they sit on the same track and carry equal payloads. This is what lets a
    /// result be used as a dictionary key or compared inside a test.
    /// </para>
    /// <code><![CDATA[
    /// var first = Rop.Ok(OrderFixtures.Sample).MapOk(static order => order.Id);
    /// var second = Rop.Ok(OrderFixtures.Sample).MapOk(static order => order.Id);
    /// var same = first.Equals(second);
    ///
    /// // Ok("ord-1001") equals Ok("ord-1001") because the payloads are equal,
    /// // Err(E_X) equals Err(E_Y) only when the two errors are equal.
    /// ]]></code>
    /// </remarks>
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
    /// <remarks>
    /// <para>
    /// The boxed overload defers to the strongly typed comparison, so a result stored in an
    /// <see cref="object"/> still compares by value and not by reference.
    /// </para>
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .MapOk(static payment => payment.Id);
    /// var same = charged.Equals((object)charged);
    ///
    /// // Ok("pay-ord-1001") equals its own boxed copy,
    /// // Err(E_CHARGE) equals its own boxed copy - the boxed form compares by value.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is Result<TOk, TErr> other && Equals(other);

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The hash code is derived from the payload of the track the result sits on, so equal results always share
    /// a hash code and can be used as dictionary keys.
    /// </para>
    /// <code><![CDATA[
    /// var placed = Rop.Ok(OrderFixtures.Sample).MapOk(static order => order.Id);
    /// var bucket = placed.GetHashCode() % 16;
    ///
    /// // Ok("ord-1001") hashes by its payload,
    /// // Err(E_X) hashes by its error - equal values land in the same bucket.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() =>
        _isOk
            ? HashCode.Combine(true, _ok is null ? 0 : EqualityComparer<TOk>.Default.GetHashCode(_ok))
            : HashCode.Combine(false, _err is null ? 0 : EqualityComparer<TErr>.Default.GetHashCode(_err));

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The text is a debugging aid, not a wire format. Branch on <see cref="IsOk"/> and read the payload through
    /// the combinators instead of parsing this string.
    /// </para>
    /// <code><![CDATA[
    /// var charged = Rop.Try(() => OrderFixtures.Charge(OrderFixtures.Sample), "E_CHARGE")
    ///     .MapOk(static payment => payment.Id);
    /// var text = charged.ToString();
    ///
    /// // Ok("pay-ord-1001") for a successful charge,
    /// // Err(E_CHARGE) for a failed one - the text is meant for logs, not for branching.
    /// ]]></code>
    /// </remarks>
    public override string ToString() => _isOk ? $"Ok({_ok})" : $"Err({_err})";

    /// <summary>Determines whether two results are equal (success values or error values must be equal).</summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when both are on the same track with equal payloads.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of <see cref="Equals(Result{TOk,TErr})"/> and is what a test or a
    /// cache lookup naturally reaches for.
    /// </para>
    /// <code><![CDATA[
    /// var placed = Rop.Ok(OrderFixtures.Sample).MapOk(static order => order.Id);
    /// var same = placed == Rop.Ok(OrderFixtures.Sample).MapOk(static order => order.Id);
    ///
    /// // Ok("ord-1001") == Ok("ord-1001") because the payloads are equal,
    /// // Err(E_X) == Err(E_Y) only when the two errors are equal.
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(Result<TOk, TErr> left, Result<TOk, TErr> right) => left.Equals(right);

    /// <summary>Determines whether two results are different.</summary>
    /// <param name="left">The left result.</param>
    /// <param name="right">The right result.</param>
    /// <returns><see langword="true"/> when they differ.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of the negated equality, and is how two results of the same shape are
    /// told apart in a test assertion.
    /// </para>
    /// <code><![CDATA[
    /// var placed = Rop.Ok(OrderFixtures.Sample).MapOk(static order => order.Id);
    /// var other = Rop.Ok(OrderFixtures.Sample).MapOk(static order => order.Id + "-retry");
    /// var differs = placed != other;
    ///
    /// // Ok("ord-1001") != Ok("ord-1001-retry") because the payloads differ,
    /// // Err(E_X) != Err(E_Y) as soon as the two errors are not equal.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(Result<TOk, TErr> left, Result<TOk, TErr> right) => !left.Equals(right);
}
