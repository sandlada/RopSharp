using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class M5RegressionTests
{
    [Test]
    public async Task DEF06_DefaultAwaiters_ThrowInvalidOperationExceptionOnAllMembers()
    {
        ResultTaskAwaiter<int, RopError> rtAwaiter = default;
        OptionTaskAwaiter<int> otAwaiter = default;
        ValidationTaskAwaiter<int, RopError> vtAwaiter = default;

        await Assert.That(TestSupport.Catch(() => _ = rtAwaiter.IsCompleted) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => _ = rtAwaiter.GetResult()) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => rtAwaiter.OnCompleted(static () => { })) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => rtAwaiter.UnsafeOnCompleted(static () => { })) is InvalidOperationException).IsTrue();

        await Assert.That(TestSupport.Catch(() => _ = otAwaiter.IsCompleted) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => _ = otAwaiter.GetResult()) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => otAwaiter.OnCompleted(static () => { })) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => otAwaiter.UnsafeOnCompleted(static () => { })) is InvalidOperationException).IsTrue();

        await Assert.That(TestSupport.Catch(() => _ = vtAwaiter.IsCompleted) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => _ = vtAwaiter.GetResult()) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => vtAwaiter.OnCompleted(static () => { })) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => vtAwaiter.UnsafeOnCompleted(static () => { })) is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task DEF08_PerElementCancellationToken_IsPreservedInZipSequenceAndPartition()
    {
        using var innerCts = new CancellationTokenSource();
        var pendingResult = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pendingOption = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var innerCanceledRt = ResultTask.From(pendingResult.Task).WithCancellation(innerCts.Token);
        var innerCanceledOt = OptionTask.From(pendingOption.Task).WithCancellation(innerCts.Token);

        innerCts.Cancel();

        var zipR2 = await TestSupport.CatchAsync(() => ResultTask.Zip(ResultTask.From(Result.Ok(1)), innerCanceledRt, static (a, b) => a + b));
        await Assert.That(zipR2 is OperationCanceledException).IsTrue();

        var zipR3 = await TestSupport.CatchAsync(() => ResultTask.Zip(ResultTask.From(Result.Ok(1)), innerCanceledRt, ResultTask.From(Result.Ok(3)), static (a, b, c) => a + b + c));
        await Assert.That(zipR3 is OperationCanceledException).IsTrue();

        var seqR = await TestSupport.CatchAsync(() => ResultTask.Sequence([ResultTask.From(Result.Ok(1)), innerCanceledRt]));
        await Assert.That(seqR is OperationCanceledException).IsTrue();

        var partR = await TestSupport.CatchAsync(() => ResultTask.Partition([ResultTask.From(Result.Ok(1)), innerCanceledRt]));
        await Assert.That(partR is OperationCanceledException).IsTrue();

        var zipV2 = await TestSupport.CatchAsync(() => ValidationTask.Zip(ResultTask.From(Result.Ok(1)), innerCanceledRt, static (a, b) => a + b));
        await Assert.That(zipV2 is OperationCanceledException).IsTrue();

        var zipV3 = await TestSupport.CatchAsync(() => ValidationTask.Zip(ResultTask.From(Result.Ok(1)), innerCanceledRt, ResultTask.From(Result.Ok(3)), static (a, b, c) => a + b + c));
        await Assert.That(zipV3 is OperationCanceledException).IsTrue();

        var seqV = await TestSupport.CatchAsync(() => ValidationTask.Sequence([ResultTask.From(Result.Ok(1)), innerCanceledRt]));
        await Assert.That(seqV is OperationCanceledException).IsTrue();

        var zipO2 = await TestSupport.CatchAsync(() => OptionTask.Zip(OptionTask.From(Option.Some(1)), innerCanceledOt, static (a, b) => a + b));
        await Assert.That(zipO2 is OperationCanceledException).IsTrue();

        var zipO3 = await TestSupport.CatchAsync(() => OptionTask.Zip(OptionTask.From(Option.Some(1)), innerCanceledOt, OptionTask.From(Option.Some(3)), static (a, b, c) => a + b + c));
        await Assert.That(zipO3 is OperationCanceledException).IsTrue();

        var seqO = await TestSupport.CatchAsync(() => OptionTask.Sequence([OptionTask.From(Option.Some(1)), innerCanceledOt]));
        await Assert.That(seqO is OperationCanceledException).IsTrue();

        pendingResult.SetResult(Result.Ok(2));
        pendingOption.SetResult(Option.Some(2));
    }

    [Test]
    public async Task DEF09_LinqSelectMany_PreservesBothInnerAndOuterCancellation_AndRejectsTaskLike()
    {
        using var innerCts = new CancellationTokenSource();
        var pendingRt = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pendingOt = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        innerCts.Cancel();

        var rtEx2 = await TestSupport.CatchAsync(() =>
            ResultTask.From(Result.Ok(1))
                .SelectMany(_ => ResultTask.From(pendingRt.Task).WithCancellation(innerCts.Token)));
        await Assert.That(rtEx2 is OperationCanceledException).IsTrue();

        var rtEx3 = await TestSupport.CatchAsync(() =>
            ResultTask.From(Result.Ok(1))
                .SelectMany(
                    _ => ResultTask.From(pendingRt.Task).WithCancellation(innerCts.Token),
                    static (a, b) => a + b));
        await Assert.That(rtEx3 is OperationCanceledException).IsTrue();

        var otEx2 = await TestSupport.CatchAsync(() =>
            OptionTask.From(Option.Some(1))
                .SelectMany(_ => OptionTask.From(pendingOt.Task).WithCancellation(innerCts.Token)));
        await Assert.That(otEx2 is OperationCanceledException).IsTrue();

        var otEx3 = await TestSupport.CatchAsync(() =>
            OptionTask.From(Option.Some(1))
                .SelectMany(
                    _ => OptionTask.From(pendingOt.Task).WithCancellation(innerCts.Token),
                    static (a, b) => a + b));
        await Assert.That(otEx3 is OperationCanceledException).IsTrue();

        pendingRt.SetResult(Result.Ok(2));
        pendingOt.SetResult(Option.Some(2));

        var invoked = false;
        var taskLike2 = TestSupport.Catch(() =>
            ResultTask.From(Result.Ok(1)).SelectMany<int, Task<int>, RopError>(_ =>
            {
                invoked = true;
                return ResultTask.From(Result.Ok(Task.FromResult(1)));
            }));
        await Assert.That(taskLike2 is InvalidOperationException).IsTrue();
        await Assert.That(invoked).IsFalse();

        var taskLike3 = TestSupport.Catch(() =>
            ResultTask.From(Result.Ok(1)).SelectMany(
                _ => ResultTask.From(Result.Ok(2)),
                static (_, b) => Task.FromResult(b)));
        await Assert.That(taskLike3 is InvalidOperationException).IsTrue();

        var optTaskLike2 = TestSupport.Catch(() =>
            OptionTask.From(Option.Some(1)).SelectMany(_ => OptionTask.From(Option.Some(Task.FromResult(1)))));
        await Assert.That(optTaskLike2 is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task DEF10_ValidationTaskSequence_EnumeratesInsideTask()
    {
        static IEnumerable<ResultTask<int, RopError>> ThrowingSequence()
        {
            yield return ResultTask.From(Result.Ok(1));
            throw new InvalidOperationException("iterator boom");
        }

        // Constructing the ValidationTask must not synchronously exhaust or fault from the iterator.
        var task = ValidationTask.Sequence(ThrowingSequence());
        var ex = await TestSupport.CatchAsync(() => task);
        await Assert.That(ex is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task DEF11Async_ResultTryAsyncAndSwitchAsync_RejectTaskLikeBeforeInvocation()
    {
        var invoked = false;
        var tryEx = TestSupport.Catch(() => Result.TryAsync(
            () =>
            {
                invoked = true;
                return Task.FromResult(Task.FromResult(1));
            },
            "E_TASK"));
        await Assert.That(tryEx is InvalidOperationException).IsTrue();
        await Assert.That(invoked).IsFalse();

        var tryErrEx = TestSupport.Catch(() => Result.TryAsync(
            () =>
            {
                invoked = true;
                return Task.FromResult(Task.FromResult(1));
            },
            static _ => Sample.Error(),
            static () => Sample.Error()));
        await Assert.That(tryErrEx is InvalidOperationException).IsTrue();
        await Assert.That(invoked).IsFalse();

        await Assert.That(TestSupport.Catch(() => Result.SwitchAsync<int, Task<int>, RopError>(static x => Task.FromResult(Task.FromResult(x)))) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Result.TrySwitchAsync<int, Task<int>>(static x => Task.FromResult(Task.FromResult(x)), static _ => Sample.Error())) is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task DEF12_ResultTryAsync_WithCancellationReplacesObservedToken()
    {
        using var initialCts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var computation = Result.TryAsync(() => tcs.Task, "E_TASK", initialCts.Token);

        initialCts.Cancel();
        tcs.SetResult(42);

        var replaced = computation.WithCancellation(CancellationToken.None);
        var result = await replaced;
        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.Expect("test")).IsEqualTo(42);

        using var initialUnitCts = new CancellationTokenSource();
        var unitTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var unitComp = Result.TryAsync(() => unitTcs.Task, "E_UNIT", initialUnitCts.Token);

        initialUnitCts.Cancel();
        unitTcs.SetResult();

        var replacedUnit = unitComp.WithCancellation(CancellationToken.None);
        var unitResult = await replacedUnit;
        await Assert.That(unitResult.IsOk).IsTrue();
    }
}
