namespace Sandlada.RopSharp.Sample.Scenarios;

/// <summary>
/// Scenario 2: a concurrent carrier bid.
/// <para>
/// All carriers are contacted eagerly, so one failure cannot hide another carrier's result. The
/// outcomes are partitioned while both tracks stay visible, transient transport failures are retried,
/// and the final business rule (delivery deadline) can still fail after several carriers succeeded.
/// </para>
/// </summary>
internal static class CarrierQuoteScenario
{
    private static int _callCount;

    public static async Task RunAsync()
    {
        await ExecuteAsync(
            "domestic: one rejection, one timeout, the timeout is retried",
            Request(carriers: ["ground", "air", "budget", "flaky"]));

        await ExecuteAsync(
            "express with a one-day deadline: bids exist but none is eligible",
            Request(express: true, deadlineDays: 1, carriers: ["ground", "air", "budget"]));

        await ExecuteAsync(
            "remote lane: every carrier fails, all errors are combined",
            Request(lane: "remote", carriers: ["ground", "air", "budget", "flaky"]));

        await ExecuteAsync(
            "gold member: the winning bid is discounted",
            Request(membership: "gold", carriers: ["ground", "air"]));

        Console.WriteLine("-- cancellation before any carrier is called");
        _callCount = 0;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        try
        {
            await QuoteAsync(Request(carriers: ["ground", "air"]), cancellation.Token);
            Console.WriteLine("   => unexpected success");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"   => OperationCanceledException, carrier calls: {_callCount}");
        }
    }

    private static async Task ExecuteAsync(string label, QuoteRequest request)
    {
        Console.WriteLine($"-- {label}");
        _callCount = 0;

        var result = await QuoteAsync(request, CancellationToken.None);
        Console.WriteLine(result.Match(
            ok: static quote => $"   => OK {quote.Carrier} {quote.Total:0.00} in {quote.Days}d ({quote.FailedCarriers} carrier(s) failed)",
            err: static error => $"   => ERR {error}"));
        Console.WriteLine($"   carrier calls: {_callCount}");
        Console.WriteLine();
    }

    private static async Task<Result<Quote, RopError>> QuoteAsync(
        QuoteRequest request,
        CancellationToken cancellationToken)
    {
        // Eager fan-out: every task is created before the first await, so a failure in one carrier
        // does not prevent the others from being contacted.
        var pending = request.Carriers
            .Select(carrier => BidAsync(carrier, request, retry: false, cancellationToken))
            .ToArray();

        var firstPass = await Task.WhenAll(pending);
        var outcomes = request.Carriers
            .Select((carrier, index) => (Carrier: carrier, Outcome: firstPass[index]))
            .ToList();

        WriteOutcomes(outcomes);

        if (request.AllowRetry)
        {
            for (var index = 0; index < outcomes.Count; index++)
            {
                if (!outcomes[index].Outcome.TryGetErr(out var error) || error.Code != "E_CARRIER")
                {
                    continue;
                }

                Console.WriteLine($"   retry {outcomes[index].Carrier} after {error.Code}");
                outcomes[index] = (outcomes[index].Carrier, await BidAsync(
                    outcomes[index].Carrier,
                    request,
                    retry: true,
                    cancellationToken));
            }
        }

        // Partition keeps every successful bid and every failure: the short-circuit track is not used
        // here because a single bad carrier must not discard the others.
        var (bids, failures) = Rop.Partition(outcomes.Select(static outcome => outcome.Outcome));
        if (bids.Count == 0)
        {
            Console.WriteLine($"   no bid survived ({failures.Count} error(s))");
            return Rop.Err<Quote>(RopError.Combine(failures));
        }

        // Result LINQ keeps the late deadline and pricing rules on the same failure track.
        return from bid in ChooseBid(bids, request.DeadlineDays)
               from total in Price(bid, request.Membership)
               select new Quote(bid.Carrier, total, bid.Days, failures.Count);
    }

    private static void WriteOutcomes(
        IReadOnlyList<(string Carrier, Result<CarrierBid, RopError> Outcome)> outcomes)
    {
        foreach (var (carrier, outcome) in outcomes)
        {
            Console.WriteLine(outcome.Match(
                ok: bid => $"   bid {carrier,-6} {bid.Price:0.00} in {bid.Days}d",
                err: error => $"   bid {carrier,-6} ERR {error.Code}: {error.Message}"));
        }
    }

    private static Result<CarrierBid, RopError> ChooseBid(
        IReadOnlyList<CarrierBid> bids,
        int? deadlineDays)
    {
        var eligible = deadlineDays is null
            ? bids
            : bids.Where(bid => bid.Days <= deadlineDays.Value).ToList();

        return eligible.Count == 0
            ? Rop.Err<CarrierBid>(RopError.Create(
                "E_DEADLINE",
                $"no carrier meets the {deadlineDays}-day deadline"))
            : Rop.Ok(eligible.MinBy(static bid => bid.Price)!);
    }

    private static Result<decimal, RopError> Price(CarrierBid bid, string? membership)
    {
        var discount = Opt.FromNullable(membership)
            .MapSome(static tier => tier.ToLowerInvariant())
            .MapSome(static tier => tier switch
            {
                "gold" => 0.15m,
                "silver" => 0.05m,
                _ => 0m,
            })
            .Match(some: static value => value, none: static () => 0m);

        return Rop.Ok(bid.Price).MapOk(price => price * (1m - discount));
    }

    private static Task<Result<CarrierBid, RopError>> BidAsync(
        string carrier,
        QuoteRequest request,
        bool retry,
        CancellationToken cancellationToken)
    {
        // A carrier that does not serve the lane is a domain rejection, not a transport failure.
        if (carrier == "budget")
        {
            return Task.FromResult(Rop.Err<CarrierBid>(RopError.Create(
                "E_NO_ROUTE",
                "budget carrier does not serve this lane")));
        }

        // Everything that can throw crosses the boundary exactly once.
        return Rop.TryAsync(
            () => CarrierAsync(carrier, request, retry, cancellationToken),
            "E_CARRIER",
            cancellationToken);
    }

    private static async Task<CarrierBid> CarrierAsync(
        string carrier,
        QuoteRequest request,
        bool retry,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        if (carrier == "flaky" && !retry)
        {
            throw new TimeoutException("flaky carrier timed out");
        }

        if (request.Lane == "remote")
        {
            throw new HttpRequestException($"{carrier} does not serve remote lanes");
        }

        return carrier switch
        {
            "ground" => new CarrierBid("ground", 10m + (request.WeightKg * 2m), 3),
            "air" => new CarrierBid("air", 25m + (request.WeightKg * 3m), 2),
            "flaky" => new CarrierBid("flaky", 55m, 1),
            _ => new CarrierBid(carrier, 30m, 2),
        };
    }

    private static QuoteRequest Request(
        string lane = "domestic",
        decimal weightKg = 4.5m,
        bool express = false,
        string? membership = null,
        int? deadlineDays = null,
        bool allowRetry = true,
        params string[] carriers) =>
        new(lane, weightKg, express, carriers, allowRetry, deadlineDays, membership);
}

internal sealed record QuoteRequest(
    string Lane,
    decimal WeightKg,
    bool Express,
    IReadOnlyList<string> Carriers,
    bool AllowRetry,
    int? DeadlineDays,
    string? Membership);

internal sealed record CarrierBid(string Carrier, decimal Price, int Days);

internal sealed record Quote(string Carrier, decimal Total, int Days, int FailedCarriers);
