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

        var pending = ResultTask.From(source.Task).MapOkAsync((value, _) =>
        {
            mapperCalled = true;
            return Task.FromResult(value + 1);
        });

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        await Assert.That(mapperCalled).IsFalse();

        source.SetResult(Result.Ok(1));
        var result = await pending;

        await Assert.That(mapperCalled).IsTrue();
        await Assert.That(result.Expect("test")).IsEqualTo(2);
    }

    [Test]
    public async Task MapOkAsync_PendingMapperTask_CompletesWithMapperResult()
    {
        var mapper = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Result.Ok(1).MapOkAsync((_, _) => mapper.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        mapper.SetResult(5);
        var result = await pending;

        await Assert.That(result.Expect("test")).IsEqualTo(5);
    }

    [Test]
    public async Task BindOkAsync_PendingBinderTask_ResumesWithBoundResult()
    {
        var binder = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = ResultTask.From(Result.Ok(1)).BindOkAsync((_, _) => binder.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        binder.SetResult(Result.Ok(9));
        var result = await pending;

        await Assert.That(result.Expect("test")).IsEqualTo(9);
    }

    [Test]
    public async Task BindOkAsync_OnSyncResult_WithCancellationAwareBinder_PendingTask()
    {
        var binder = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Result.Ok(1).BindOkAsync((_, _) => binder.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        binder.SetResult(Result.Ok(7));
        var result = await pending;

        await Assert.That(result.Expect("test")).IsEqualTo(7);
    }

    [Test]
    public async Task MapErrAsync_PendingAntecedent_MapsErrorAfterCompletion()
    {
        var source = PendingResult();

        var pending = ResultTask.From(source.Task).MapErrAsync((error, _) => Task.FromResult(error.WithCode("E_LATE")));

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        source.SetResult(Result.Err<int>(Sample.Error("E_X")));
        var result = await pending;

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_LATE");
    }

    [Test]
    public async Task BindErrAsync_PendingFallbackTask_ResumesWithFallback()
    {
        var fallback = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = ResultTask.From(Result.Err<int>(Sample.Error())).BindErrAsync((_, _) => fallback.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        fallback.SetResult(Result.Ok(11));
        var result = await pending;

        await Assert.That(result.Expect("test")).IsEqualTo(11);
    }

    [Test]
    public async Task TeeOkAsync_PendingAntecedent_ObservesAfterCompletion()
    {
        var source = PendingResult();
        var observed = 0;

        var pending = ResultTask.From(source.Task).TeeOkAsync((value, _) =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        await Assert.That(observed).IsEqualTo(0);

        source.SetResult(Result.Ok(3));
        await pending;

        await Assert.That(observed).IsEqualTo(3);
    }

    [Test]
    public async Task MapSomeAsync_PendingAntecedent_MapsAfterCompletion()
    {
        var source = PendingOption();

        var pending = OptionTask.From(source.Task).MapSomeAsync((value, _) => Task.FromResult(value * 2));

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        source.SetResult(Option.Some(4));
        var option = await pending;

        await Assert.That(option.Expect("test")).IsEqualTo(8);
    }

    [Test]
    public async Task BindSomeAsync_PendingBinderTask_ResumesWithBoundOption()
    {
        var binder = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = OptionTask.From(Task.FromResult(Option.Some(1))).BindSomeAsync((_, _) => binder.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        binder.SetResult(Option.Some(6));
        var option = await pending;

        await Assert.That(option.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task BindNoneAsync_CancellationAware_PendingFallbackTask_Resumes()
    {
        var fallback = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = OptionTask.From(Task.FromResult(Option.None<int>())).BindNone(_ => fallback.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        fallback.SetResult(Option.Some(3));
        var option = await pending;

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task FilterAsync_PendingAntecedent_FiltersAfterCompletion()
    {
        var source = PendingOption();

        var pending = OptionTask.From(source.Task).Filter(value => value > 0);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        source.SetResult(Option.Some(1));
        var option = await pending;

        await Assert.That(option.IsSome).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_PendingAntecedent_ObservesAfterCompletion()
    {
        var source = PendingOption();
        var observed = false;

        var pending = OptionTask.From(source.Task).TeeNoneAsync(_ =>
        {
            observed = true;
            return Task.CompletedTask;
        });

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        await Assert.That(observed).IsFalse();

        source.SetResult(Option.None<int>());
        await pending;

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TryAsync_PendingBoundaryTask_Resumes()
    {
        var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Result.TryAsync(() => source.Task, _ => Sample.Error());

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        source.SetResult(42);
        var result = await pending;

        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task TraverseAsync_PendingTask_Resumes()
    {
        var source = PendingResult();

        var pending = ResultTask.Traverse([1], (_, _) => source.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        source.SetResult(Result.Ok(5));
        var result = await pending;

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 5);
    }
}
