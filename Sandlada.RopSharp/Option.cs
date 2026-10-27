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
    public bool IsSome => _isSome;

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    public bool IsNone => !_isSome;

    /// <summary>
    /// Attempts to read the contained value without throwing.
    /// </summary>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, the contained value; otherwise the default of
    /// <typeparamref name="T"/> (which may be <see langword="null"/>).
    /// </param>
    /// <returns><see langword="true"/> when the option is <c>Some</c>; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _isSome;
    }

    /// <inheritdoc />
    public bool Equals(Option<T> other) =>
        _isSome == other._isSome
        && (!_isSome || EqualityComparer<T>.Default.Equals(_value, other._value));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        _isSome && _value is not null
            ? HashCode.Combine(true, EqualityComparer<T>.Default.GetHashCode(_value))
            : 0;

    /// <inheritdoc />
    public override string ToString() => _isSome ? $"Some({_value})" : "None";

    /// <summary>Determines whether two options are equal.</summary>
    /// <param name="left">The left option.</param>
    /// <param name="right">The right option.</param>
    /// <returns><see langword="true"/> when both are <c>None</c>, or both are <c>Some</c> with equal values.</returns>
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    /// <summary>Determines whether two options are different.</summary>
    /// <param name="left">The left option.</param>
    /// <param name="right">The right option.</param>
    /// <returns><see langword="true"/> when they differ.</returns>
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}
