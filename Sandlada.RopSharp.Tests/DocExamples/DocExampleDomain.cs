namespace Sandlada.RopSharp.DocExamples;

/// <summary>
/// The shared domain of the "order fulfillment" storyline used by every documentation example.
/// </summary>
/// <remarks>
/// <para>
/// The XML documentation of <c>Sandlada.RopSharp</c> shows each public API inside a small but realistic
/// order-fulfillment pipeline: an order is priced, validated, charged and finally shipped. Every example in the
/// library draws from these types, so a reader who moves from one API to the next keeps seeing the same program
/// grow instead of a series of unrelated snippets.
/// </para>
/// <para>
/// This file is compiled twice: once as part of the test assembly, and once - verbatim - as an extra syntax tree
/// inside <c>DocExampleCompilationTests</c>, so the documented snippets are type checked against the very same
/// declarations the documentation mentions.
/// </para>
/// </remarks>
public sealed record Address(string Line1, string City, string Country);

/// <summary>A buyer. The shipping address is optional so the absence path can be demonstrated.</summary>
public sealed record Customer(string Id, string Name, Address? Address);

/// <summary>A single ordered line. <see cref="Amount"/> derives the line total from quantity and unit price.</summary>
public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice)
{
    /// <summary>Gets the total price of this line.</summary>
    public decimal Amount => Quantity * UnitPrice;
}

/// <summary>An order to fulfill.</summary>
public sealed record Order(string Id, Customer Customer, IReadOnlyList<OrderLine> Lines)
{
    /// <summary>Gets the summed price of all lines.</summary>
    public decimal Total => Lines.Sum(static line => line.Amount);

    /// <summary>Gets the summed quantity of all lines.</summary>
    public int ItemCount => Lines.Sum(static line => line.Quantity);
}

/// <summary>The lifecycle state of a payment.</summary>
public enum PaymentStatus
{
    /// <summary>The payment has been created but not submitted.</summary>
    Pending = 0,

    /// <summary>The payment provider has reserved the funds.</summary>
    Authorized = 1,

    /// <summary>The funds have been taken.</summary>
    Captured = 2,

    /// <summary>The provider refused the payment.</summary>
    Declined = 3,
}

/// <summary>A payment attempt against an order.</summary>
public sealed record Payment(string Id, decimal Amount, PaymentStatus Status);

/// <summary>A parcel handed over to a carrier.</summary>
public sealed record Shipment(string Id, string Carrier, string TrackingNumber);

/// <summary>
/// Deterministic fixtures and stand-ins for the external systems of the order-fulfillment storyline.
/// </summary>
/// <remarks>
/// The examples never talk to a real network. They call these members so that both the success track and the
/// failure track of a boundary can be shown deterministically.
/// </remarks>
public static class OrderFixtures
{
    /// <summary>Gets an order that is charged successfully.</summary>
    public static Order Sample { get; } = new(
        "ord-1001",
        new Customer("cus-7", "Ada Lovelace", new Address("1 Analytical Engine Way", "London", "GB")),
        [new OrderLine("SKU-1", 2, 19.50m), new OrderLine("SKU-2", 1, 5.00m)]);

    /// <summary>Gets an order whose id makes <see cref="Charge"/> fail.</summary>
    public static Order Declined { get; } = Sample with { Id = "ord-1001-declined" };

    /// <summary>Gets a customer that has no shipping address.</summary>
    public static Customer CustomerWithoutAddress { get; } = new("cus-9", "Grace Hopper", null);

    /// <summary>
    /// Stands in for the payment provider. Throws for <see cref="Declined"/>, so a boundary built on top of it
    /// shows both an <c>Ok</c> and an <c>Err</c> outcome.
    /// </summary>
    /// <param name="order">The order to charge.</param>
    /// <returns>The captured payment.</returns>
    /// <exception cref="InvalidOperationException">The provider refuses the order.</exception>
    public static Payment Charge(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.Id.EndsWith("-declined", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"the payment provider declined order {order.Id}");
        }

        return new Payment($"pay-{order.Id}", order.Total, PaymentStatus.Captured);
    }

    /// <summary>Stands in for the carrier integration. It never fails.</summary>
    /// <param name="order">The order to ship.</param>
    /// <param name="payment">The captured payment that authorizes the shipment.</param>
    /// <returns>The created shipment.</returns>
    public static Shipment Ship(Order order, Payment payment)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(payment);

        return new Shipment($"shp-{order.Id}", "Acme Express", $"TRK-{order.Id}-{payment.Id}");
    }
}
