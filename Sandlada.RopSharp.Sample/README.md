# Sample scenarios

`Sandlada.RopSharp.Sample` is a runnable tour of the library under orchestration pressure. Each
scenario keeps several tracks alive at once instead of walking one `Ok` and one `Err` in a straight
line.

```bash
dotnet run --project Sandlada.RopSharp.Sample -c Release
```

## 1. Fulfillment saga

A single order crosses validation, tax, inventory, payment and shipment.

- Header and line validation aggregate independently and are flattened into one failure.
- A tax-service outage falls back to a static table; the fallback can still fail.
- Payment retries only transport timeouts; a decline is final.
- Each committed step pushes a compensation. On failure the stack unwinds in reverse order.
- A failing compensation is collected next to the original failure, not swallowed.
- Compensation runs without the caller's token, so cancellation still rolls back committed work.

## 2. Carrier quotes

Four carriers are contacted eagerly, so one failure cannot hide another carrier's bid.

- Domain rejections (`E_NO_ROUTE`) and transport exceptions (`E_CARRIER`) stay distinguishable.
- Only transient transport failures are retried.
- `Rop.Partition` keeps every successful bid and every failure visible.
- A late business rule (delivery deadline) can still fail after several carriers succeeded.
- When no bid survives, all carrier errors are combined.
- A pre-cancelled token is checked before any carrier is invoked.

## 3. Settlement batch

Eight rows run their own pipelines concurrently: parse, account lookup, account checks, charge.

- A frozen account can report frozen, insufficient balance and exceeded limit together.
- Rows are independent tracks; the batch partitions instead of short-circuiting.
- Channel failures retry once, permanently busy channels stay failed.
- Failures are grouped by error code.
- After topping up one account, only the funding failures are replayed; frozen and channel
  failures are not retried blindly.

## 4. New in 1.1

A compact tour of the additive API: `Rop.Parse`, asynchronous `CombineAsync`, `FailIf`/`Switch`, the
`Either<TLeft,TRight>` union and the `TryLast` option collection helper.

The exact console output is recorded in [`../docs/verification.md`](../docs/verification.md) and is
verified by the trimmed-publish gate.
