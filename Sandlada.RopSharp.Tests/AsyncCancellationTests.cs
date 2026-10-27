using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class AsyncCancellationTests
{
    [Test]
    public async Task SequenceAsync_WithCancelledTokenAndPendingTask_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() => Rop.SequenceAsync([pending.Task], cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Rop.Ok(1));
    }

    [Test]
    public async Task TraverseAsync_WithCancelledTokenAndPendingTask_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseAsync([1], _ => pending.Task, cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Rop.Ok(1));
    }

    [Test]
    public async Task SequenceAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.SequenceAsync(new Task<Result<int, RopError>>[] { null! }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseAsync<int, int, RopError>([1], _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindOkAsync_WithNullBinderResult_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.Ok(1).BindOkAsync<int, int, RopError>(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindErrAsync_WithNullFallbackResult_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.Err<int>(Sample.Error()).BindErrAsync<int, RopError>(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindNoneAsync_WithNullFallbackResult_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.None<int>()).BindNoneAsync<int>(() => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task ToValidation_RoundTripsThroughToSingleError()
    {
        var error = Sample.Error("E_ROUND");

        var roundTrip = Rop.Err<int>(error).ToValidation().ToSingleError();

        await Assert.That(roundTrip.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task RopError_Combine_DeduplicatesMessagesAndCodes()
    {
        var combined = RopError.Combine(
        [
            RopError.Create("E_A", "same message"),
            RopError.Create("E_A", "same message"),
            RopError.Create("E_B", "other message"),
        ]);

        await Assert.That(combined.Message).IsEqualTo("same message; other message");
        await Assert.That(combined.Details).IsEqualTo("E_A, E_B");
    }

    [Test]
    public async Task Partition_PreservesRelativeOrder()
    {
        var items = Enumerable.Range(0, 20)
            .Select(value => value % 3 == 0
                ? Rop.Err<int>(Sample.Error($"E_{value}"))
                : Rop.Ok(value))
            .ToArray();

        var (oks, errs) = Rop.Partition(items);

        await TestSupport.AssertSequenceAsync(oks, [1, 2, 4, 5, 7, 8, 10, 11, 13, 14, 16, 17, 19]);
        await Assert.That(errs.Count).IsEqualTo(7);
    }

    [Test]
    [NotInParallel]
    public async Task CoreTypes_AreSafeForConcurrentUse()
    {
        var failures = 0;

        Parallel.For(0, 2_000, index =>
        {
            var result = Rop.Ok(index)
                .MapOk(static value => value + 1)
                .BindOk(static value => Rop.Ok(value * 2))
                .Ensure(static value => value >= 0, Sample.Error());

            if (!result.TryGetOk(out var value) || value != (index + 1) * 2)
            {
                Interlocked.Increment(ref failures);
            }

            var option = Opt.Some(index).MapSome(static value => value + 1).Filter(static value => value > 0);

            if (!option.TryGetValue(out var optionValue) || optionValue != index + 1)
            {
                Interlocked.Increment(ref failures);
            }
        });

        await Assert.That(failures).IsEqualTo(0);
    }
}
