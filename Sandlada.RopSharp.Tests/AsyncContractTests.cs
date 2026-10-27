using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Covers the completed map matrix (Result receiver, async and cancellation-aware mappers) and the shared
/// callback contract: a mapper, action or predicate must return a task, and a null task faults with
/// <see cref="InvalidOperationException"/> instead of a null-reference failure.
/// </summary>
public sealed class AsyncContractTests
{
    [Test]
    public async Task MapOkAsync_OnSyncResult_WithCancellationAwareMapper_PassesToken()
    {
        using var cts = new CancellationTokenSource();

        var result = await Rop.Ok(2)
            .MapOkAsync((value, token) => Task.FromResult(value + (token == cts.Token ? 1 : 0)), cts.Token);

        await Assert.That(result.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task MapOkAsync_OnSyncResult_CancellationAware_PropagatesError()
    {
        var error = Sample.Error("E_MAP");

        var result = await Rop.Err<int>(error)
            .MapOkAsync((value, _) => Task.FromResult(value + 1));

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task MapOkAsync_OnSyncResult_CancellationAware_PendingMapper_Resumes()
    {
        var mapper = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Rop.Ok(1).MapOkAsync((_, _) => mapper.Task, CancellationToken.None);

        await Assert.That(pending.IsCompleted).IsFalse();
        mapper.SetResult(4);

        await Assert.That((await pending).OrThrow()).IsEqualTo(4);
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_WithAsyncMapper_MapsError()
    {
        var result = await Rop.Err<int>(Sample.Error("E_X"))
            .MapErrAsync(error => Task.FromResult(error.WithCode("E_ASYNC")));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_ASYNC");
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_KeepsSuccess()
    {
        var called = false;

        var result = await Rop.Ok(1).MapErrAsync(error =>
        {
            called = true;
            return Task.FromResult(error);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_WithCancellationAwareMapper_PassesToken()
    {
        using var cts = new CancellationTokenSource();

        var result = await Rop.Err<int>(Sample.Error("E_X"))
            .MapErrAsync((error, token) => Task.FromResult(error.WithCode(token == cts.Token ? "E_CT" : "E_NO")), cts.Token);

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_CT");
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_CancellationAware_KeepsSuccess()
    {
        var result = await Rop.Ok(1).MapErrAsync((error, _) => Task.FromResult(error));

        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_CancellationAware_PendingMapper_Resumes()
    {
        var mapper = new TaskCompletionSource<RopError>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Rop.Err<int>(Sample.Error("E_X")).MapErrAsync((_, _) => mapper.Task, CancellationToken.None);

        await Assert.That(pending.IsCompleted).IsFalse();
        mapper.SetResult(Sample.Error("E_LATE"));

        await Assert.That((await pending).ErrOrThrow().Code).IsEqualTo("E_LATE");
    }

    [Test]
    public async Task AsyncCallbacks_ReturningNullTask_FaultWithInvalidOperation()
    {
        var mapperTaskReceiver = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Ok(1)).MapOkAsync(value => (Task<int>)null!));
        var mapperTaskReceiverCt = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Ok(1)).MapOkAsync((value, _) => (Task<int>)null!));
        var mapperResultReceiver = await TestSupport.CatchAsync(() =>
            Rop.Ok(1).MapOkAsync(value => (Task<int>)null!));
        var mapperResultReceiverCt = await TestSupport.CatchAsync(() =>
            Rop.Ok(1).MapOkAsync((value, _) => (Task<int>)null!));

        var errMapperResult = await TestSupport.CatchAsync(() =>
            Rop.Err<int>(Sample.Error()).MapErrAsync(error => (Task<RopError>)null!));
        var errMapperCt = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Err<int>(Sample.Error())).MapErrAsync((error, _) => (Task<RopError>)null!));

        var optionMapper = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.Some(1)).MapSomeAsync(value => (Task<int>)null!));
        var optionPredicate = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.Some(1)).FilterAsync(value => (Task<bool>)null!));

        var teeOk = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Ok(1)).TeeOkAsync(value => (Task)null!));
        var teeSome = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.Some(1)).TeeSomeAsync(value => (Task)null!));
        var teeNone = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.None<int>()).TeeNoneAsync(() => null!));

        foreach (var exception in new[]
                 {
                     mapperTaskReceiver,
                     mapperTaskReceiverCt,
                     mapperResultReceiver,
                     mapperResultReceiverCt,
                     errMapperResult,
                     errMapperCt,
                     optionMapper,
                     optionPredicate,
                     teeOk,
                     teeSome,
                     teeNone,
                 })
        {
            await Assert.That(exception is InvalidOperationException).IsTrue();
        }
    }
}
