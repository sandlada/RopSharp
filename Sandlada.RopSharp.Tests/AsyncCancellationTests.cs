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

        var exception = await TestSupport.CatchAsync(() => ResultTask.Sequence([ResultTask.From(pending.Task)], cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task TraverseAsync_WithCancelledTokenAndPendingTask_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() =>
            ResultTask.Traverse([1], (_, _) => pending.Task, cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task SequenceAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            ResultTask.Sequence(new ResultTask<int, RopError>[] { default }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            ResultTask.Traverse<int, int, RopError>([1], (_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindOkAsync_WithNullBinderResult_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.Ok(1).BindOkAsync<int, int, RopError>((_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindErrAsync_WithNullFallbackResult_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.Err<int>(Sample.Error()).BindErrAsync<int, RopError>((_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindNoneAsync_WithNullFallbackResult_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            OptionTask.From(Task.FromResult(Option.None<int>())).BindNone(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task ToValidation_RoundTripsThroughToSingleError()
    {
        var error = Sample.Error("E_ROUND");

        var roundTrip = Validation.From(Result.Err<int>(error)).Collapse(static errors => errors[0]);

        await Assert.That(roundTrip.ExpectErr()).IsSameReferenceAs(error);
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
                ? Result.Err<int>(Sample.Error($"E_{value}"))
                : Result.Ok(value))
            .ToArray();

        var (oks, errs) = Result.Partition(items);

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
            var result = Result.Ok(index)
                .MapOk(static value => value + 1)
                .BindOk(static value => Result.Ok(value * 2))
                .Ensure(static value => value >= 0, Sample.Error());

            if (!result.TryGetOk(out var value) || value != (index + 1) * 2)
            {
                Interlocked.Increment(ref failures);
            }

            var option = Option.Some(index).MapSome(static value => value + 1).Filter(static value => value > 0);

            if (!option.TryGetValue(out var optionValue) || optionValue != index + 1)
            {
                Interlocked.Increment(ref failures);
            }
        });

        await Assert.That(failures).IsEqualTo(0);
    }
}
