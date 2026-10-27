using System.Globalization;

namespace Sandlada.RopSharp.Sample.Scenarios;

/// <summary>
/// Scenario 3: a settlement batch where every row runs its own multi-stage pipeline
/// (parse -> account lookup -> aggregate account checks -> charge with retry).
/// <para>
/// Rows are independent tracks running concurrently, so the batch partitions outcomes instead of
/// short-circuiting. Failures are grouped by code, and only the rows that failed on insufficient
/// funds are retried after the account is topped up.
/// </para>
/// </summary>
internal static class SettlementScenario
{
    private static readonly Dictionary<string, Account> Accounts = new(StringComparer.Ordinal)
    {
        ["acct-1"] = new(Balance: 100m, DailyLimit: 50m),
        ["acct-2"] = new(Balance: 10m, DailyLimit: 100m),
        ["acct-3"] = new(Balance: 100m, DailyLimit: 100m, Frozen: true),
        ["acct-4"] = new(Balance: 100m, DailyLimit: 100m),
    };

    private static readonly SettlementRow[] Rows =
    [
        new("row-1", "acct-1", "25.00", "wire"),
        new("row-2", "acct-2", "60.00", "wire"),
        new("row-3", "acct-3", "200.00", "wire"),
        new("row-4", "acct-4", "40.00", "busy"),
        new("row-5", "acct-9", "10.00", "wire"),
        new("row-6", "acct-4", "30.00", "busy-always"),
        new("row-7", "acct-1", "75.00", "wire"),
        new("row-8", "acct-2", "5.00", "wire"),
    ];

    private static int _chargeCount;

    public static async Task RunAsync()
    {
        // Concurrent rows: Task.WhenAll keeps every row's outcome, unlike a short-circuiting sequence.
        Console.WriteLine("-- first pass");
        var firstPass = await Task.WhenAll(Rows.Select(row => SettleAsync(row, CancellationToken.None)));
        ReportOutcomes(firstPass);

        // Only funding failures can be fixed by a state change; frozen/limit/channel failures stay.
        Accounts["acct-2"] = Accounts["acct-2"] with { Balance = 200m };
        var retryable = Rows
            .Select((row, index) => (Row: row, Outcome: firstPass[index]))
            .Where(static pair => pair.Outcome.TryGetErr(out var error) && error.Code == "E_INSUFFICIENT_FUNDS")
            .ToArray();

        Console.WriteLine($"-- retry after top-up ({retryable.Length} row(s))");
        var retried = await Task.WhenAll(retryable.Select(static pair => SettleAsync(pair.Row, CancellationToken.None)));
        ReportOutcomes(retried);

        Console.WriteLine("-- cancellation while the batch is in flight");
        _chargeCount = 0;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        try
        {
            await Task.WhenAll(Rows.Select(row => SettleAsync(row, cancellation.Token)));
            Console.WriteLine("   => unexpected success");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"   => OperationCanceledException before any charge attempt (attempts: {_chargeCount})");
        }
    }

    private static void ReportOutcomes(IReadOnlyList<Result<SettledPayment, RopError>> outcomes)
    {
        var (settled, failed) = Rop.Partition(outcomes);

        foreach (var payment in settled)
        {
            Console.WriteLine($"   settled {payment.RowId}: {payment.Amount:0.00} from {payment.Account}");
        }

        foreach (var error in failed)
        {
            Console.WriteLine($"   failed  {error}");
        }

        Console.WriteLine($"   summary: {settled.Count} settled, {failed.Count} failed");

        if (failed.Count > 0)
        {
            var codes = failed
                .GroupBy(static error => error.Code)
                .OrderBy(static group => group.Key, StringComparer.Ordinal)
                .Select(static group => $"{group.Key} x{group.Count()}");
            Console.WriteLine($"   codes: {string.Join(", ", codes)}");
        }

        Console.WriteLine();
    }

    private static async Task<Result<SettledPayment, RopError>> SettleAsync(
        SettlementRow row,
        CancellationToken cancellationToken)
    {
        return await ParseAmount(row.Amount)
            .BindOk(amount => Opt.FromNullable(Accounts.GetValueOrDefault(row.Account))
                .RequireSome(() => RopError.Create("E_ACCOUNT_UNKNOWN", $"no such account: {row.Account}"))
                .BindOk(account => ValidateAccount(account, amount)
                    .MapOk(_ => (Account: account, Amount: amount))))
            .BindOkAsync((pair, token) => ChargeAsync(row, pair.Account, pair.Amount, token), cancellationToken);
    }

    private static Result<decimal, RopError> ParseAmount(string raw) =>
        Rop.Try(
                () => decimal.Parse(raw, NumberStyles.Number, CultureInfo.InvariantCulture),
                "E_AMOUNT_PARSE")
            .Ensure(
                static amount => amount > 0m,
                static amount => RopError.Create("E_AMOUNT_RANGE", "amount must be positive"));

    /// <summary>
    /// Every account rule is checked; a frozen account can report the freeze, the insufficient balance
    /// and the exceeded limit in one aggregated failure.
    /// </summary>
    private static Result<Account, RopError> ValidateAccount(Account account, decimal amount) =>
        Rop.Validate(
                account.Frozen
                    ? Rop.Err<bool>(RopError.Create("E_ACCOUNT_FROZEN", "account is frozen"))
                    : Rop.Ok(true),
                amount <= account.Balance
                    ? Rop.Ok(amount)
                    : Rop.Err<decimal>(RopError.Create(
                        "E_INSUFFICIENT_FUNDS",
                        $"balance {account.Balance:0.00} < {amount:0.00}")),
                amount <= account.DailyLimit
                    ? Rop.Ok(amount)
                    : Rop.Err<decimal>(RopError.Create(
                        "E_LIMIT_EXCEEDED",
                        $"daily limit {account.DailyLimit:0.00} < {amount:0.00}")),
                (_, _, _) => account)
            .ToSingleError(RopError.Combine);

    private static async Task<Result<SettledPayment, RopError>> ChargeAsync(
        SettlementRow row,
        Account account,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var first = await ChargeAttemptAsync(row, amount, attempt: 1, cancellationToken);

        // Transient channel failures are retried once; every other failure is terminal.
        return await first.BindErrAsync(
            (error, token) =>
            {
                if (error.Code != "E_CHANNEL_BUSY")
                {
                    return Task.FromResult(Rop.Err<SettledPayment>(error));
                }

                Console.WriteLine($"   retry {row.Id}: {error.Code}");
                return ChargeAttemptAsync(row, amount, attempt: 2, token);
            },
            cancellationToken);
    }

    private static async Task<Result<SettledPayment, RopError>> ChargeAttemptAsync(
        SettlementRow row,
        decimal amount,
        int attempt,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _chargeCount);
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        if (row.Channel == "busy-always")
        {
            return Rop.Err<SettledPayment>(RopError.Create(
                "E_CHANNEL_BUSY",
                $"channel unavailable for {row.Id} (attempt {attempt})"));
        }

        if (row.Channel == "busy" && attempt == 1)
        {
            return Rop.Err<SettledPayment>(RopError.Create(
                "E_CHANNEL_BUSY",
                $"channel busy for {row.Id} (attempt {attempt})"));
        }

        return Rop.Ok(new SettledPayment(row.Id, row.Account, amount, row.Channel));
    }
}

internal sealed record SettlementRow(string Id, string Account, string Amount, string Channel);

internal sealed record Account(decimal Balance, decimal DailyLimit, bool Frozen = false);

internal sealed record SettledPayment(string RowId, string Account, decimal Amount, string Channel);
