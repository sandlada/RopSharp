namespace Sandlada.RopSharp;

/// <summary>
/// Represents the absence of a meaningful value (the functional unit type).
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="Unit.Value"/> when an operation only succeeds or fails and no payload is required,
/// for example <c>Result&lt;Unit, RopError&gt;</c> combined with <c>MapOkValue(Unit.Value)</c>.
/// </para>
/// <code><![CDATA[
/// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
///     .MapOk(static shipment => Unit.Value);
///
/// // Ok(()) when the parcel was handed over,
/// // Err(E_SHIP) when the carrier refused - the payload is dropped, only the outcome matters.
/// ]]></code>
/// </remarks>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Gets the single value of <see cref="Unit"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Unit"/> has exactly one inhabitant, so the value carries no information. It exists so that a
    /// side-effecting boundary still returns a result that composes with every other combinator.
    /// </para>
    /// <code><![CDATA[
    /// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
    ///     .MapOk(static shipment => Unit.Value);
    ///
    /// // Ok(()) when the parcel was handed over,
    /// // Err(E_SHIP) when the carrier refused - the payload is dropped, only the outcome matters.
    /// ]]></code>
    /// </remarks>
    public static Unit Value => default;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// Every unit is equal to every other unit, because there is only one of them.
    /// </para>
    /// <code><![CDATA[
    /// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
    ///     .MapOk(static shipment => Unit.Value);
    /// var unit = shipped.Match(ok: static _ => Unit.Value, err: static _ => Unit.Value);
    /// var same = unit.Equals(Unit.Value);
    ///
    /// // Ok(()) always equals Ok(()),
    /// // Err(E_SHIP) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The boxed overload accepts any boxed <see cref="Unit"/>; there is no other inhabitant to compare against.
    /// </para>
    /// <code><![CDATA[
    /// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
    ///     .MapOk(static shipment => Unit.Value);
    /// var unit = shipped.Match(ok: static _ => Unit.Value, err: static _ => Unit.Value);
    /// var same = unit.Equals((object)Unit.Value);
    ///
    /// // Ok(()) equals its own boxed copy,
    /// // Err(E_SHIP) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The hash code is always zero, which keeps a unit cheap to use as a dictionary key.
    /// </para>
    /// <code><![CDATA[
    /// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
    ///     .MapOk(static shipment => Unit.Value);
    /// var unit = shipped.Match(ok: static _ => Unit.Value, err: static _ => Unit.Value);
    /// var bucket = unit.GetHashCode();
    ///
    /// // Ok(()) always hashes to 0,
    /// // Err(E_SHIP) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// The text is a debugging aid. It spells the empty tuple, which is the same idea as the value itself.
    /// </para>
    /// <code><![CDATA[
    /// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
    ///     .MapOk(static shipment => Unit.Value);
    /// var unit = shipped.Match(ok: static _ => Unit.Value, err: static _ => Unit.Value);
    /// var text = unit.ToString();
    ///
    /// // Ok(()) renders as "()",
    /// // Err(E_SHIP) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public override string ToString() => "()";

    /// <summary>Determines whether two <see cref="Unit"/> values are equal. Always <see langword="true"/>.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    /// <remarks>
    /// <para>
    /// The operator mirrors the always-true equality of the unit type, so a unit can be compared without a
    /// null check.
    /// </para>
    /// <code><![CDATA[
    /// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
    ///     .MapOk(static shipment => Unit.Value);
    /// var unit = shipped.Match(ok: static _ => Unit.Value, err: static _ => Unit.Value);
    /// var same = unit == Unit.Value;
    ///
    /// // Ok(()) == Ok(()),
    /// // Err(E_SHIP) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>Determines whether two <see cref="Unit"/> values are different. Always <see langword="false"/>.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>Always <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The operator mirrors the always-false inequality of the unit type, so a unit can be compared without a
    /// null check.
    /// </para>
    /// <code><![CDATA[
    /// var shipped = Rop.Try(() => OrderFixtures.Ship(OrderFixtures.Sample, OrderFixtures.Charge(OrderFixtures.Sample)), "E_SHIP")
    ///     .MapOk(static shipment => Unit.Value);
    /// var unit = shipped.Match(ok: static _ => Unit.Value, err: static _ => Unit.Value);
    /// var differs = unit != Unit.Value;
    ///
    /// // Ok(()) != Ok(()) is always false,
    /// // Err(E_SHIP) is the other track of the same result.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(Unit left, Unit right) => false;
}
