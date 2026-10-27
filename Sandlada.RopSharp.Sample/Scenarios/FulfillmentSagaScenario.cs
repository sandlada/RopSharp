namespace Sandlada.RopSharp.Sample.Scenarios;

/// <summary>
/// Scenario 1: a multi-service fulfillment saga.
/// <para>
/// Each step is a separate track: validation aggregates, tax can fail over to a fallback, inventory and
/// payment can fail after earlier steps already committed, and cancellation must not run compensation.
/// When a step fails, every committed step is compensated in reverse order. Compensation failures are
/// collected next to the original failure instead of replacing or hiding it.
/// </para>
/// </summary>
internal static class FulfillmentSagaScenario
{
    private static readonly Dictionary<string, CatalogEntry> Catalog = new(StringComparer.Ordinal)
    {
        ["SKU-1"] = new(Price: 19.99m, Available: 5),
        ["SKU-2"] = new(Price: 7.50m, Available: 1),
        ["SKU-3"] = new(Price: 3.25m, Available: 0),
    };

    private static readonly Dictionary<string, decimal> Coupons = new(StringComparer.Ordinal)
    {
        ["SAVE10"] = 0.10m,
    };

    private static readonly Dictionary<string, decimal> TaxRates = new(StringComparer.Ordinal)
    {
        ["TW"] = 0.05m,
        ["US"] = 0.07m,
        ["EU"] = 0.20m,
    };

    public static async Task RunAsync()
    {
        await ExecuteAsync(
            "happy path: live tax, payment timeout retried once, all steps commit",
            Request("ord-1", coupon: "SAVE10", token: "tok_timeout_once", lines: [new("SKU-1", 2)]));

        await ExecuteAsync(
            "payment declined after inventory was reserved",
            Request("ord-2", token: "tok_declined", lines: [new("SKU-1", 1)]));

        await ExecuteAsync(
            "shipment rejected and the payment void also fails",
            Request("ord-3", token: "tok_void_fails", address: "invalid address", lines: [new("SKU-2", 1)]));

        await ExecuteAsync(
            "tax service outage recovered by the fallback rate",
            Request("ord-4", country: "EU", lines: [new("SKU-1", 1)]));

        await ExecuteAsync(
            "invalid request: header and every line aggregate into one failure",
            Request("ord-5", customer: " ", coupon: "BAD", lines: [new("SKU-3", 0), new("SKU-404", 1)]));

        await ExecuteAsync(
            "tax outage with no fallback",
            Request("ord-6", country: "ZZ", lines: [new("SKU-1", 1)]));

        Console.WriteLine("-- cancellation before the first remote call");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        try
        {
            await FulfillAsync(Request("ord-7", lines: [new("SKU-1", 1)]), cancellation.Token);
            Console.WriteLine("   => unexpected success");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("   => OperationCanceledException, no compensation ran");
        }
    }

    private static async Task ExecuteAsync(string label, FulfillmentRequest request)
    {
        Console.WriteLine($"-- {label}");
        var result = await FulfillAsync(request, CancellationToken.None);
        Console.WriteLine(result.Match(
            ok: static fulfillment => $"   => OK {fulfillment.OrderId}: {fulfillment.Total:0.00}, {fulfillment.Carrier} {fulfillment.Tracking}, {fulfillment.CompensationCount} compensation(s) armed",
            err: static failure => $"   => ERR {Describe(failure)}"));
        Console.WriteLine();
    }

    private static async Task<Result<Fulfillment, SagaFailure>> FulfillAsync(
        FulfillmentRequest request,
        CancellationToken cancellationToken)
    {
        var compensations = new CompensationStack();
        var validationErrors = new List<RopError>();

        // Header and lines fail independently; flatten both groups into one result.
        var headerResult = ValidateHeader(request);
        if (!headerResult.TryGetOk(out var header)
            && headerResult.TryGetErr(out var headerErrors))
        {
            validationErrors.AddRange(headerErrors);
        }

        var linesResult = PriceLines(request.Lines);
        if (!linesResult.TryGetOk(out var lines)
            && linesResult.TryGetErr(out var lineErrors))
        {
            validationErrors.AddRange(lineErrors);
        }

        if (validationErrors.Count > 0)
        {
            return await AbortAsync(RopError.Combine(validationErrors), compensations);
        }

        var validHeader = header!;
        var validLines = lines!;
        Console.WriteLine($"   validate: {validHeader.Customer}, {validLines.Count} line(s)");

        // Tax is a remote call with a local fallback: only the recovery path decides whether to abort.
        var tax = await TaxRateAsync(request.Country, cancellationToken);
        if (!tax.TryGetOk(out var taxRate))
        {
            return await AbortAsync(ErrorOrThrow(tax), compensations);
        }

        var subtotal = validLines.Sum(static line => line.LineTotal);
        var discount = validHeader.Coupon.Match(
            some: rate => subtotal * rate,
            none: static () => 0m);
        var total = (subtotal - discount) * (1m + taxRate);
        var priced = new PricedOrder(request.OrderId, validHeader.Customer, validLines, subtotal, discount, taxRate, total);
        Console.WriteLine($"   price: subtotal {subtotal:0.00}, discount {discount:0.00}, tax {taxRate * 100m:0}%, total {total:0.00}");

        var reservation = await ReserveAsync(priced, cancellationToken);
        if (!reservation.TryGetOk(out var reserved))
        {
            return await AbortAsync(ErrorOrThrow(reservation), compensations);
        }

        compensations.Defer(token => ReleaseAsync(reserved, token));
        Console.WriteLine($"   reserve: {reserved.Warehouse} for {request.OrderId}");

        var authorization = await AuthorizeAsync(priced, request, cancellationToken);
        if (!authorization.TryGetOk(out var authorized))
        {
            return await AbortAsync(ErrorOrThrow(authorization), compensations);
        }

        compensations.Defer(token => VoidAsync(authorized, token));
        Console.WriteLine($"   authorize: {authorized.Reference} for {authorized.Amount:0.00}");

        var shipment = await ShipAsync(request, cancellationToken);
        if (!shipment.TryGetOk(out var shipped))
        {
            return await AbortAsync(ErrorOrThrow(shipment), compensations);
        }

        Console.WriteLine($"   ship: {shipped.Carrier} {shipped.Tracking}");
        return Rop.Ok<Fulfillment, SagaFailure>(new Fulfillment(
            request.OrderId,
            total,
            shipped.Tracking,
            shipped.Carrier,
            compensations.Count));
    }

    /// <summary>
    /// Runs the compensation stack in reverse order. Compensation deliberately ignores the request
    /// cancellation token: a cancelled caller must still roll back committed work.
    /// </summary>
    private static async Task<Result<Fulfillment, SagaFailure>> AbortAsync(RopError primary, CompensationStack compensations)
    {
        var compensationErrors = await compensations.RunAllAsync();
        return Rop.Err<Fulfillment, SagaFailure>(new SagaFailure(primary, compensationErrors));
    }

    private static RopError ErrorOrThrow<T>(Result<T, RopError> result) =>
        result.TryGetErr(out var error)
            ? error
            : throw new InvalidOperationException("Expected the error track.");

    private static Result<Header, IReadOnlyList<RopError>> ValidateHeader(FulfillmentRequest request) =>
        Rop.Validate(
            RequireCustomer(request.Customer),
            RequireAddress(request.Address),
            ParseCoupon(request.CouponCode),
            static (customer, address, coupon) => new Header(customer, address, coupon));

    private static Result<string, RopError> RequireCustomer(string customer) =>
        Rop.FromNullable(customer.Trim(), static () => RopError.Create("E_CUSTOMER_REQUIRED", "customer is required"))
            .Ensure(
                static value => value.Length >= 2,
                static value => RopError.Create("E_CUSTOMER_TOO_SHORT", $"customer '{value}' is too short"));

    private static Result<string, RopError> RequireAddress(string address) =>
        Rop.FromNullable(address.Trim(), static () => RopError.Create("E_ADDRESS_REQUIRED", "address is required"))
            .Ensure(
                static value => value.Length >= 5,
                static value => RopError.Create("E_ADDRESS_TOO_SHORT", $"address '{value}' is too short"));

    /// <summary>
    /// Absence of a coupon is <c>Ok(None)</c>; a present but unknown coupon is an error. Transpose keeps
    /// those two outcomes apart without using null or a sentinel discount.
    /// </summary>
    private static Result<Option<decimal>, RopError> ParseCoupon(string? code) =>
        Opt.FromNullable(code?.Trim())
            .MapSome(static value => value.ToUpperInvariant())
            .MapSome(static value => Coupons.TryGetValue(value, out var rate)
                ? Rop.Ok(rate)
                : Rop.Err<decimal>(RopError.Create("E_COUPON_UNKNOWN", $"unknown coupon: {value}")))
            .Transpose();

    private static Result<IReadOnlyList<PricedLine>, IReadOnlyList<RopError>> PriceLines(IReadOnlyList<OrderLine> lines) =>
        lines.Count == 0
            ? Rop.Err<IReadOnlyList<PricedLine>, IReadOnlyList<RopError>>(
                [RopError.Create("E_NO_LINES", "at least one order line is required")])
            : Rop.SequenceValidation(lines.Select(PriceLine));

    private static Result<PricedLine, RopError> PriceLine(OrderLine line) =>
        Catalog.TryGetValue(line.Sku, out var entry)
            ? Rop.Ok(line.Quantity)
                .Ensure(
                    static quantity => quantity > 0,
                    static quantity => RopError.Create("E_QUANTITY", $"quantity {quantity} must be positive"))
                .MapOk(quantity => new PricedLine(line.Sku, quantity, entry.Price, entry.Price * quantity))
            : Rop.Err<PricedLine>(RopError.Create("E_SKU_UNKNOWN", $"unknown sku: {line.Sku}"));

    private static async Task<Result<decimal, RopError>> TaxRateAsync(string country, CancellationToken cancellationToken)
    {
        var live = await Rop.TryAsync(
            () => LiveTaxRateAsync(country, cancellationToken),
            "E_TAX_SERVICE",
            cancellationToken);

        // The fallback is still part of the error track: it can succeed or fail on its own.
        return await live.BindErrAsync(
            (error, _) =>
            {
                if (TaxRates.TryGetValue(country, out var fallback))
                {
                    Console.WriteLine($"   tax: {error.Code}, fallback {fallback * 100m:0}%");
                    return Task.FromResult(Rop.Ok<decimal, RopError>(fallback));
                }

                return Task.FromResult(Rop.Err<decimal, RopError>(
                    error.WithDetails($"no fallback for country={country}")));
            },
            cancellationToken);
    }

    private static async Task<decimal> LiveTaxRateAsync(string country, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        return country switch
        {
            "TW" => 0.05m,
            "US" => 0.07m,
            _ => throw new HttpRequestException("tax service has no route for this country"),
        };
    }

    private static async Task<Result<InventoryReservation, RopError>> ReserveAsync(
        PricedOrder order,
        CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        var shortages = order.Lines
            .Where(static line => Catalog[line.Sku].Available < line.Quantity)
            .Select(static line => RopError.Create(
                "E_OUT_OF_STOCK",
                $"{line.Sku}: requested {line.Quantity}, available {Catalog[line.Sku].Available}"))
            .ToList();

        return shortages.Count == 0
            ? Rop.Ok(new InventoryReservation(order.OrderId, "WH-A"))
            : Rop.Err<InventoryReservation>(RopError.Combine(shortages));
    }

    private static async Task<Result<PaymentAuthorization, RopError>> AuthorizeAsync(
        PricedOrder order,
        FulfillmentRequest request,
        CancellationToken cancellationToken)
    {
        var first = await ChargeAsync(order.Total, request.PaymentToken, attempt: 1, cancellationToken);

        // Only transport timeouts are retried; a decline is final and must not be attempted twice.
        return await first.BindErrAsync(
            (error, token) =>
            {
                if (error.Code != "E_GATEWAY_TIMEOUT")
                {
                    return Task.FromResult(Rop.Err<PaymentAuthorization>(error));
                }

                Console.WriteLine("   payment: gateway timeout, retrying once");
                return ChargeAsync(order.Total, request.PaymentToken, attempt: 2, token);
            },
            cancellationToken);
    }

    private static async Task<Result<PaymentAuthorization, RopError>> ChargeAsync(
        decimal amount,
        string token,
        int attempt,
        CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        return token switch
        {
            "tok_declined" => Rop.Err<PaymentAuthorization>(
                RopError.Create("E_DECLINED", "issuer declined the card")),
            "tok_timeout_once" when attempt == 1 => Rop.Err<PaymentAuthorization>(
                RopError.Create("E_GATEWAY_TIMEOUT", "gateway timed out")),
            "tok_void_fails" => Rop.Ok(new PaymentAuthorization(
                "auth-void-fails",
                amount,
                CancellationWillFail: true)),
            "tok_timeout_once" => Rop.Ok(new PaymentAuthorization(
                $"auth-retry-{attempt}",
                amount,
                CancellationWillFail: false)),
            _ => Rop.Ok(new PaymentAuthorization("auth-primary", amount, CancellationWillFail: false)),
        };
    }

    private static async Task<Result<Shipment, RopError>> ShipAsync(
        FulfillmentRequest request,
        CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        return request.Address.Contains("invalid", StringComparison.OrdinalIgnoreCase)
            ? Rop.Err<Shipment>(RopError.Create("E_ADDRESS_REJECTED", "carrier rejected the address"))
            : Rop.Ok(new Shipment($"TRK-{request.OrderId}", "ground"));
    }

    private static Task<Result<Unit, RopError>> ReleaseAsync(
        InventoryReservation reservation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Console.WriteLine($"   compensate: release {reservation.OrderId} at {reservation.Warehouse}");
        return Task.FromResult(Rop.Ok<Unit, RopError>(Unit.Value));
    }

    private static Task<Result<Unit, RopError>> VoidAsync(
        PaymentAuthorization authorization,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (authorization.CancellationWillFail)
        {
            Console.WriteLine($"   compensate: void {authorization.Reference} FAILED");
            return Task.FromResult(Rop.Err<Unit>(RopError.Create(
                "E_VOID_FAILED",
                $"issuer refused to void {authorization.Reference}")));
        }

        Console.WriteLine($"   compensate: void {authorization.Reference}");
        return Task.FromResult(Rop.Ok<Unit, RopError>(Unit.Value));
    }

    private static string Describe(SagaFailure failure) =>
        failure.CompensationFailures.Count == 0
            ? $"{failure.Primary} | compensation: all succeeded"
            : $"{failure.Primary} | compensation failures: {SampleFormat.Errors(failure.CompensationFailures)}";

    private static FulfillmentRequest Request(
        string orderId,
        string customer = "Ada",
        string country = "TW",
        string? coupon = null,
        string token = "tok_ok",
        string address = "1 Main St",
        params OrderLine[] lines) =>
        new(orderId, customer, country, lines, coupon, token, address);
}

/// <summary>
/// LIFO stack of compensating actions. Nothing runs while the saga is healthy.
/// </summary>
internal sealed class CompensationStack
{
    private readonly List<Func<CancellationToken, Task<Result<Unit, RopError>>>> _steps = [];

    public int Count => _steps.Count;

    public void Defer(Func<CancellationToken, Task<Result<Unit, RopError>>> step) => _steps.Add(step);

    public async Task<IReadOnlyList<RopError>> RunAllAsync()
    {
        var errors = new List<RopError>();
        for (var index = _steps.Count - 1; index >= 0; index--)
        {
            var outcome = await _steps[index](CancellationToken.None);
            if (outcome.TryGetErr(out var error))
            {
                errors.Add(error);
            }
        }

        return errors;
    }
}

internal sealed record FulfillmentRequest(
    string OrderId,
    string Customer,
    string Country,
    IReadOnlyList<OrderLine> Lines,
    string? CouponCode,
    string PaymentToken,
    string Address);

internal sealed record OrderLine(string Sku, int Quantity);

internal sealed record CatalogEntry(decimal Price, int Available);

internal sealed record Header(string Customer, string Address, Option<decimal> Coupon);

internal sealed record PricedLine(string Sku, int Quantity, decimal UnitPrice, decimal LineTotal);

internal sealed record PricedOrder(
    string OrderId,
    string Customer,
    IReadOnlyList<PricedLine> Lines,
    decimal Subtotal,
    decimal Discount,
    decimal TaxRate,
    decimal Total);

internal sealed record InventoryReservation(string OrderId, string Warehouse);

internal sealed record PaymentAuthorization(string Reference, decimal Amount, bool CancellationWillFail);

internal sealed record Shipment(string Tracking, string Carrier);

internal sealed record Fulfillment(
    string OrderId,
    decimal Total,
    string Tracking,
    string Carrier,
    int CompensationCount);

internal sealed record SagaFailure(RopError Primary, IReadOnlyList<RopError> CompensationFailures);
