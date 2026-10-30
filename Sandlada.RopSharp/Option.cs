using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// An immutable value that either contains a value (<c>Some</c>) or does not (<c>None</c>).
/// </summary>
/// <typeparam name="T">The contained value type.</typeparam>
/// <remarks>
/// <para>
/// Absence of a value is not an error: use <see cref="Option{T}"/> for optional data and
/// <see cref="Result{TOk,TErr}"/> when the reason for failure matters.
/// </para>
/// <para>
/// <see langword="default"/>(<see cref="Option{T}"/>) is contractually <c>None</c>, so options are safe to store
/// in arrays, fields and generic containers. An <see cref="Option{T}"/> instance itself is never <see langword="null"/>.
/// </para>
/// <para>
/// An option answers the question "is there a value?" and nothing else. It is produced where a value may be
/// missing - a lookup that may not find anything, a field that may be unset - and is consumed either by a
/// recovery such as <see cref="OptionExtensions.BindNone{T}(Option{T},Func{Option{T}})"/> or by a crossing into
/// the result world through <see cref="OptionBridgeExtensions.RequireSome{T,TErr}(Option{T},Func{TErr})"/>.
/// </para>
/// <code><![CDATA[
/// var city = Opt.FromNullable(OrderFixtures.Sample.Customer.Address)
///     .BindSome(static shipping => Opt.Some(shipping.City))
///     .MapSome(static name => name.ToUpperInvariant());
///
/// // Some("LONDON") when the customer has a shipping address,
/// // None when the address is missing - Option carries no reason for the absence.
/// ]]></code>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Option is the established functional name for this concept; the public API is C#-only by design.")]
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T _value;
    private readonly bool _isSome;

    internal Option(T value, bool isSome)
    {
        _value = value;
        _isSome = isSome;
    }

    /// <summary>
    /// Gets a value indicating whether this instance contains a value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The flag tells you which track the option sits on without reading the value. It is the cheapest way to
    /// branch before deciding how to continue.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.Sample.Customer.Address);
    /// var present = address.IsSome;
    ///
    /// // Some(address) makes IsSome true,
    /// // None makes IsSome false - the absence itself carries no reason.
    /// ]]></code>
    /// </remarks>
    public bool IsSome => _isSome;

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the exact complement of <see cref="IsSome"/>. It is the signal that a pipeline step should be
    /// skipped rather than that something went wrong.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address);
    /// var missing = address.IsNone;
    ///
    /// // None makes IsNone true,
    /// // Some(address) makes IsNone false - the value is read with TryGetValue.
    /// ]]></code>
    /// </remarks>
    public bool IsNone => !_isSome;

    /// <summary>
    /// Attempts to read the contained value without throwing.
    /// </summary>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, the contained value; otherwise the default of
    /// <typeparamref name="T"/> (which may be <see langword="null"/>).
    /// </param>
    /// <returns><see langword="true"/> when the option is <c>Some</c>; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The probe never throws. On the <c>None</c> track the output stays at its default, so the return value has
    /// to be checked before the value is used.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.Sample.Customer.Address);
    /// if (address.TryGetValue(out var shipping))
    /// {
    ///     var city = shipping.City;
    /// }
    ///
    /// // Some(address) is read into 'shipping' when the address exists,
    /// // None leaves 'shipping' at its default - TryGetValue never throws.
    /// ]]></code>
    /// </remarks>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _isSome;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Two options are equal when both are empty, or both contain equal values. Equality ignores nothing else,
    /// because an option has nothing else.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.Sample.Customer.Address);
    /// var same = address.Equals(Opt.Some(OrderFixtures.Sample.Customer.Address!));
    ///
    /// // Some(address) equals Some(address) because the values are equal,
    /// // None equals None because both are empty.
    /// ]]></code>
    /// </remarks>
    public bool Equals(Option<T> other) =>
        _isSome == other._isSome
        && (!_isSome || EqualityComparer<T>.Default.Equals(_value, other._value));

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The boxed overload defers to the strongly typed comparison, so an option stored in an
    /// <see cref="object"/> still compares by value.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.Sample.Customer.Address);
    /// var same = address.Equals((object)Opt.Some(OrderFixtures.Sample.Customer.Address!));
    ///
    /// // Some(address) equals its own boxed copy,
    /// // None equals its own boxed copy - the boxed form compares by value.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The hash code is derived from the contained value, so equal options share a hash code and can be used as
    /// dictionary keys.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.Sample.Customer.Address);
    /// var bucket = address.GetHashCode() % 16;
    ///
    /// // Some(address) hashes by its value,
    /// // None always hashes to 0 - every empty option shares one bucket.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() =>
        _isSome && _value is not null
            ? HashCode.Combine(true, EqualityComparer<T>.Default.GetHashCode(_value))
            : 0;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The text is a debugging aid, not a wire format. Branch on <see cref="IsSome"/> and read the value through
    /// the combinators instead of parsing this string.
    /// </para>
    /// <code><![CDATA[
    /// var city = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address)
    ///     .MapSome(static shipping => shipping.City);
    /// var text = city.ToString();
    ///
    /// // Some(city) when the address exists,
    /// // None when it is missing - the text is meant for logs, not for branching.
    /// ]]></code>
    /// </remarks>
    public override string ToString() => _isSome ? $"Some({_value})" : "None";

    /// <summary>Determines whether two options are equal.</summary>
    /// <param name="left">The left option.</param>
    /// <param name="right">The right option.</param>
    /// <returns><see langword="true"/> when both are <c>None</c>, or both are <c>Some</c> with equal values.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of <see cref="Equals(Option{T})"/> and is what a cache lookup or a
    /// test assertion naturally reaches for.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.Sample.Customer.Address);
    /// var same = address == Opt.Some(OrderFixtures.Sample.Customer.Address!);
    ///
    /// // Some(address) == Some(address) because the values are equal,
    /// // None == None because both are empty.
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    /// <summary>Determines whether two options are different.</summary>
    /// <param name="left">The left option.</param>
    /// <param name="right">The right option.</param>
    /// <returns><see langword="true"/> when they differ.</returns>
    /// <remarks>
    /// <para>
    /// The operator is the ergonomic form of the negated equality, and is how an option is tested against a
    /// known expected value.
    /// </para>
    /// <code><![CDATA[
    /// var address = Opt.FromNullable(OrderFixtures.CustomerWithoutAddress.Address);
    /// var differs = address != Opt.Some(OrderFixtures.Sample.Customer.Address!);
    ///
    /// // None != Some(address) because one is empty and the other is not,
    /// // Some(a) != Some(b) as soon as the two values are not equal.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}
