namespace Sandlada.RopSharp;

/// <summary>
/// Represents the absence of a meaningful value (the functional unit type).
/// </summary>
/// <remarks>
/// Use <see cref="Unit.Value"/> when an operation only succeeds or fails and no payload is required,
/// for example <c>Result&lt;Unit, RopError&gt;</c> combined with <c>MapOkValue(Unit.Value)</c>.
/// </remarks>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Gets the single value of <see cref="Unit"/>.
    /// </summary>
    public static Unit Value => default;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public override string ToString() => "()";

    /// <summary>Determines whether two <see cref="Unit"/> values are equal. Always <see langword="true"/>.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>Determines whether two <see cref="Unit"/> values are different. Always <see langword="false"/>.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>Always <see langword="false"/>.</returns>
    public static bool operator !=(Unit left, Unit right) => false;
}
