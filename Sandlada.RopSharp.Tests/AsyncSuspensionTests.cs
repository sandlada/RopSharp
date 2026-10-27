using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Exercises the asynchronous continuation path: the antecedent (or the callback it produces) is still pending
/// when the combinator is invoked, so the state machine must resume after completion. This complements the other
/// async tests, which mostly use already-completed tasks.
/// </summary>
public sealed class AsyncSuspensionTests
{
    private static TaskCompletionSource<Result<int, RopError>> PendingResult() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static TaskCompletionSource<Option<int>> PendingOption() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Test]
    public async Task MapOkAsync_PendingAntecedent_InvokesMapperOnlyAfterCompletion()
    {
        var source = PendingResult();
        var mapperCalled = false;

        var pending = source.Task.MapOkAsync(value =>
        {
            mapperCalled = true;
            return value + 1;
        });

        await Assert.That(pending.IsCompleted).IsFalse();
        await Assert.That(mapperCalled).IsFalse();

        source.SetResult(Rop.Ok(1));
        var result = await pending;

        await Assert.That(mapperCalled).IsTrue();
        await Assert.That(result.OrThrow()).IsEqualTo(2);
    }

    [Test]
    public async Task MapOkAsync_PendingMapperTask_CompletesWithMapperResult()
    {
        var mapper = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Rop.Ok(1).AsTask().MapOkAsync(_ => mapper.Task);

        await Assert.That(pending.IsCompleted).IsFalse();
        mapper.SetResult(5);
        var result = await pending;

        await Assert.That(result.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task BindOkAsync_PendingBinderTask_ResumesWithBoundResult()
    {
        var binder = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Task.FromResult(Rop.Ok(1)).BindOkAsync(_ => binder.Task);

        await Assert.That(pending.IsCompleted).IsFalse();
        binder.SetResult(Rop.Ok(9));
        var result = await pending;

        await Assert.That(result.OrThrow()).IsEqualTo(9);
    }

    [Test]
    public async Task BindOkAsync_OnSyncResult_WithCancellationAwareBinder_PendingTask()
    {
        var binder = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Rop.Ok(1).BindOkAsync((_, _) => binder.Task, CancellationToken.None);

        await Assert.That(pending.IsCompleted).IsFalse();
        binder.SetResult(Rop.Ok(7));
        var result = await pending;

        await Assert.That(result.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task MapErrAsync_PendingAntecedent_MapsErrorAfterCompletion()
    {
        var source = PendingResult();

        var pending = source.Task.MapErrAsync(error => error.WithCode("E_LATE"));

        await Assert.That(pending.IsCompleted).IsFalse();
        source.SetResult(Rop.Err<int>(Sample.Error("E_X")));
        var result = await pending;

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_LATE");
    }

    [Test]
    public async Task BindErrAsync_PendingFallbackTask_ResumesWithFallback()
    {
        var fallback = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Task.FromResult(Rop.Err<int>(Sample.Error())).BindErrAsync(_ => fallback.Task);

        await Assert.That(pending.IsCompleted).IsFalse();
        fallback.SetResult(Rop.Ok(11));
        var result = await pending;

        await Assert.That(result.OrThrow()).IsEqualTo(11);
    }

    [Test]
    public async Task TeeOkAsync_PendingAntecedent_ObservesAfterCompletion()
    {
        var source = PendingResult();
        var observed = 0;

        var pending = source.Task.TeeOkAsync(value =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(pending.IsCompleted).IsFalse();
        await Assert.That(observed).IsEqualTo(0);

        source.SetResult(Rop.Ok(3));
        await pending;

        await Assert.That(observed).IsEqualTo(3);
    }

    [Test]
    public async Task MapSomeAsync_PendingAntecedent_MapsAfterCompletion()
    {
        var source = PendingOption();

        var pending = source.Task.MapSomeAsync(value => value * 2);

        await Assert.That(pending.IsCompleted).IsFalse();
        source.SetResult(Opt.Some(4));
        var option = await pending;

        await Assert.That(option.OrThrow()).IsEqualTo(8);
    }

    [Test]
    public async Task BindSomeAsync_PendingBinderTask_ResumesWithBoundOption()
    {
        var binder = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Task.FromResult(Opt.Some(1)).BindSomeAsync(_ => binder.Task);

        await Assert.That(pending.IsCompleted).IsFalse();
        binder.SetResult(Opt.Some(6));
        var option = await pending;

        await Assert.That(option.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task BindNoneAsync_CancellationAware_PendingFallbackTask_Resumes()
    {
        var fallback = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Task.FromResult(Opt.None<int>()).BindNoneAsync(_ => fallback.Task);

        await Assert.That(pending.IsCompleted).IsFalse();
        fallback.SetResult(Opt.Some(3));
        var option = await pending;

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task FilterAsync_PendingAntecedent_FiltersAfterCompletion()
    {
        var source = PendingOption();

        var pending = source.Task.FilterAsync(value => value > 0);

        await Assert.That(pending.IsCompleted).IsFalse();
        source.SetResult(Opt.Some(1));
        var option = await pending;

        await Assert.That(option.IsSome).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_PendingAntecedent_ObservesAfterCompletion()
    {
        var source = PendingOption();
        var observed = false;

        var pending = source.Task.TeeNoneAsync(() =>
        {
            observed = true;
            return Task.CompletedTask;
        });

        await Assert.That(pending.IsCompleted).IsFalse();
        await Assert.That(observed).IsFalse();

        source.SetResult(Opt.None<int>());
        await pending;

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TryAsync_PendingBoundaryTask_Resumes()
    {
        var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Rop.TryAsync(() => source.Task, _ => Sample.Error());

        await Assert.That(pending.IsCompleted).IsFalse();
        source.SetResult(42);
        var result = await pending;

        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task TraverseAsync_PendingTask_Resumes()
    {
        var source = PendingResult();

        var pending = Rop.TraverseAsync([1], _ => source.Task);

        await Assert.That(pending.IsCompleted).IsFalse();
        source.SetResult(Rop.Ok(5));
        var result = await pending;

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 5);
    }
}
