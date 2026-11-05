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
            OptionTask.From(Task.FromResult(Option.None<int>())).BindNoneAsync(_ => null!));

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

    [Test]
    public async Task ResultTask_ChainedCombinators_PreserveCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken seenToken1 = default;
        CancellationToken seenToken2 = default;

        var result = await ResultTask.From(Result.Ok(10))
            .WithCancellation(cts.Token)
            .MapOk(x => x + 1)
            .MapOkAsync((x, ct) =>
            {
                seenToken1 = ct;
                return Task.FromResult(x * 2);
            })
            .TeeOk(x => { })
            .BindOkAsync((x, ct) =>
            {
                seenToken2 = ct;
                return Task.FromResult(Result.Ok(x));
            });

        await Assert.That(result.Expect("ok")).IsEqualTo(22);
        await Assert.That(seenToken1).IsEqualTo(cts.Token);
        await Assert.That(seenToken2).IsEqualTo(cts.Token);
    }

    [Test]
    public async Task ValidationTask_ChainedCombinators_PreserveCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken seenToken = default;

        var validation = await ValidationTask<int, RopError>.From(Validation.Success<int, RopError>(10))
            .WithCancellation(cts.Token)
            .Map(x => x + 1)
            .MapAsync((x, ct) =>
            {
                seenToken = ct;
                return Task.FromResult(x * 2);
            });

        await Assert.That(validation.Expect("ok")).IsEqualTo(22);
        await Assert.That(seenToken).IsEqualTo(cts.Token);
    }

    [Test]
    public async Task TaskAggregations_WithPreCancelledToken_ThrowSynchronously()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.That(() => OptionTask.Zip(
            OptionTask.From(Option.Some(1)),
            OptionTask.From(Option.Some(2)),
            static (a, b) => a + b,
            cts.Token)).Throws<OperationCanceledException>();

        await Assert.That(() => OptionTask.Sequence([OptionTask.From(Option.Some(1))], cts.Token))
            .Throws<OperationCanceledException>();

        await Assert.That(() => OptionTask.Traverse([1], static (x, ct) => Task.FromResult(Option.Some(x)), cts.Token))
            .Throws<OperationCanceledException>();

        await Assert.That(() => ValidationTask.Zip(
            ResultTask.From(Result.Ok(1)),
            ResultTask.From(Result.Ok(2)),
            static (a, b) => a + b,
            cts.Token)).Throws<OperationCanceledException>();

        await Assert.That(() => ValidationTask.Sequence([ResultTask.From(Result.Ok(1))], cts.Token))
            .Throws<OperationCanceledException>();

        await Assert.That(() => ValidationTask.Traverse([1], static (x, ct) => Task.FromResult(Result.Ok(x)), cts.Token))
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task ValidationTask_FromUninitializedValidation_Throws()
    {
        Validation<int, RopError> uninitialized = default;
        await Assert.That(() => ValidationTask<int, RopError>.From(uninitialized)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Validation_FailureList_DefensivelySnapshots()
    {
        var err1 = Sample.Error("E_1");
        var err2 = Sample.Error("E_2");
        var list = new List<RopError> { err1, err2 };

        var val = Validation.Failure<int, RopError>(list);
        list[0] = Sample.Error("E_MUTATED");

        await Assert.That(val.ExpectErrors()[0].Code).IsEqualTo("E_1");
    }

    [Test]
    public async Task ResultTask_Tee_OnUninitializedTaskResult_Throws()
    {
        var task = ResultTask.From(Task.FromResult(default(Result<int, RopError>)));

        await Assert.That(async () => await task.TeeOk(static _ => { })).Throws<InvalidOperationException>();
        await Assert.That(async () => await task.TeeOkAsync(static (_, _) => Task.CompletedTask)).Throws<InvalidOperationException>();
        await Assert.That(async () => await task.TeeErr(static _ => { })).Throws<InvalidOperationException>();
        await Assert.That(async () => await task.TeeErrAsync(static (_, _) => Task.CompletedTask)).Throws<InvalidOperationException>();
    }
}
