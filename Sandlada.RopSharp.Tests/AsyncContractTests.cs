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

        var result = await ResultTask.From(Result.Ok(2)).WithCancellation(cts.Token)
            .MapOkAsync((value, token) => Task.FromResult(value + (token == cts.Token ? 1 : 0)));

        await Assert.That(result.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task MapOkAsync_OnSyncResult_CancellationAware_PropagatesError()
    {
        var error = Sample.Error("E_MAP");

        var result = await Result.Err<int>(error)
            .MapOkAsync((value, _) => Task.FromResult(value + 1));

        await Assert.That(result.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task MapOkAsync_OnSyncResult_CancellationAware_PendingMapper_Resumes()
    {
        var mapper = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Result.Ok(1).MapOkAsync((_, _) => mapper.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        mapper.SetResult(4);

        await Assert.That((await pending).Expect("test")).IsEqualTo(4);
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_WithAsyncMapper_MapsError()
    {
        var result = await Result.Err<int>(Sample.Error("E_X"))
            .MapErrAsync((error, _) => Task.FromResult(error.WithCode("E_ASYNC")));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_ASYNC");
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_KeepsSuccess()
    {
        var called = false;

        var result = await Result.Ok(1).MapErrAsync((error, _) =>
        {
            called = true;
            return Task.FromResult(error);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_WithCancellationAwareMapper_PassesToken()
    {
        using var cts = new CancellationTokenSource();

        var result = await ResultTask.From(Result.Err<int>(Sample.Error("E_X")))
            .WithCancellation(cts.Token)
            .MapErrAsync((error, token) => Task.FromResult(error.WithCode(token == cts.Token ? "E_CT" : "E_NO")));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_CT");
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_CancellationAware_KeepsSuccess()
    {
        var result = await Result.Ok(1).MapErrAsync((error, _) => Task.FromResult(error));

        await Assert.That(result.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task MapErrAsync_OnSyncResult_CancellationAware_PendingMapper_Resumes()
    {
        var mapper = new TaskCompletionSource<RopError>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Result.Err<int>(Sample.Error("E_X")).MapErrAsync((_, _) => mapper.Task);

        await Assert.That(pending.TaskUnsafe.IsCompleted).IsFalse();
        mapper.SetResult(Sample.Error("E_LATE"));

        await Assert.That((await pending).ExpectErr().Code).IsEqualTo("E_LATE");
    }

    [Test]
    public async Task AsyncCallbacks_ReturningNullTask_FaultWithInvalidOperation()
    {
        var mapperTaskReceiver = await TestSupport.CatchAsync(() => ResultTask.From(Result.Ok(1)).MapOkAsync((value, _) => (Task<int>)null!));
        var mapperTaskReceiverCt = await TestSupport.CatchAsync(() => ResultTask.From(Result.Ok(1)).MapOkAsync((value, _) => (Task<int>)null!));
        var mapperResultReceiver = await TestSupport.CatchAsync(() =>
            Result.Ok(1).MapOkAsync((value, _) => (Task<int>)null!));
        var mapperResultReceiverCt = await TestSupport.CatchAsync(() =>
            Result.Ok(1).MapOkAsync((value, _) => (Task<int>)null!));

        var errMapperResult = await TestSupport.CatchAsync(() =>
            Result.Err<int>(Sample.Error()).MapErrAsync((error, _) => (Task<RopError>)null!));
        var errMapperCt = await TestSupport.CatchAsync(() => ResultTask.From(Result.Err<int>(Sample.Error())).MapErrAsync((error, _) => (Task<RopError>)null!));

        var optionMapper = await TestSupport.CatchAsync(() =>
            OptionTask.From(Task.FromResult(Option.Some(1))).MapSomeAsync((value, _) => (Task<int>)null!));
        var optionPredicate = await TestSupport.CatchAsync(() =>
            OptionTask.From(Task.FromResult(Option.Some(1))).FilterAsync((value, _) => (Task<bool>)null!));

        var teeOk = await TestSupport.CatchAsync(() => ResultTask.From(Result.Ok(1)).TeeOkAsync((value, _) => (Task)null!));
        var teeSome = await TestSupport.CatchAsync(() =>
            OptionTask.From(Task.FromResult(Option.Some(1))).TeeSomeAsync((value, _) => (Task)null!));
        var teeNone = await TestSupport.CatchAsync(() =>
            OptionTask.From(Task.FromResult(Option.None<int>())).TeeNoneAsync(_ => null!));

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

    [Test]
    public async Task ResultTask_QuerySyntax_TwoFromClauses()
    {
        var query =
            from a in ResultTask.From(Result.Ok<int, RopError>(10))
            from b in ResultTask.From(Result.Ok<int, RopError>(20))
            select a + b;

        var result = await query;
        await Assert.That(result.Expect("ok")).IsEqualTo(30);
    }

    [Test]
    public async Task Validation_MatchAndSwitch_FullErrorList()
    {
        var err1 = Sample.Error("E_1");
        var err2 = Sample.Error("E_2");
        var fail = Validation.Failure<int, RopError>([err1, err2]);

        var errorCount = fail.Match(
            ok: _ => -1,
            errors: errs => errs.Count);
        await Assert.That(errorCount).IsEqualTo(2);

        var seenCount = 0;
        fail.Switch(
            ok: _ => { },
            errors: errs => seenCount = errs.Count);
        await Assert.That(seenCount).IsEqualTo(2);

        var success = Validation.Success<int, RopError>(42);
        var successVal = success.Match(
            ok: v => v,
            errors: _ => -1);
        await Assert.That(successVal).IsEqualTo(42);

        var seenSuccess = 0;
        success.Switch(
            ok: v => seenSuccess = v,
            errors: _ => { });
        await Assert.That(seenSuccess).IsEqualTo(42);
    }

    [Test]
    public async Task ValidationTask_StaticFrom_And_BridgeExtensions()
    {
        var val = Validation.Success<int, RopError>(100);
        var taskVal = ValidationTask.From(val);
        await Assert.That((await taskVal).Expect("ok")).IsEqualTo(100);

        var mapped = await val.ToValidationTask().MapAsync((v, _) => Task.FromResult(v + 1));
        await Assert.That(mapped.Expect("ok")).IsEqualTo(101);

        var fromTask = await Task.FromResult(val).ToValidationTask();
        await Assert.That(fromTask.Expect("ok")).IsEqualTo(100);

        var directBridge = await val.MapAsync((v, _) => Task.FromResult(v * 2));
        await Assert.That(directBridge.Expect("ok")).IsEqualTo(200);
    }

    [Test]
    public async Task OptionTask_BindNone_Variants()
    {
        var none = OptionTask.From(Option.None<int>());

        var recoveredAsync = await none.BindNoneAsync(_ => Task.FromResult(Option.Some(10)));
        await Assert.That(recoveredAsync.Expect("ok")).IsEqualTo(10);

        var recoveredSync = await none.BindNone(() => Option.Some(20));
        await Assert.That(recoveredSync.Expect("ok")).IsEqualTo(20);

        var recoveredValue = await none.BindNoneValue(30);
        await Assert.That(recoveredValue.Expect("ok")).IsEqualTo(30);

        var recoveredFactory = await none.BindNoneValue(() => 40);
        await Assert.That(recoveredFactory.Expect("ok")).IsEqualTo(40);
    }

    [Test]
    public async Task BridgeExtensions_OptionAndResult_TeeAndMore()
    {
        var ok = Result.Ok<int, RopError>(1);
        var seenOk = false;
        var okTee = await ok.TeeOkAsync((_, _) => { seenOk = true; return Task.CompletedTask; });
        await Assert.That(seenOk).IsTrue();
        await Assert.That(okTee.Expect("ok")).IsEqualTo(1);

        var err = Result.Err<int>(Sample.Error("E_TEE"));
        var seenErr = false;
        var errTee = await err.TeeErrAsync((_, _) => { seenErr = true; return Task.CompletedTask; });
        await Assert.That(seenErr).IsTrue();
        await Assert.That(errTee.ExpectErr().Code).IsEqualTo("E_TEE");

        var some = Option.Some(5);
        var seenSome = false;
        var someTee = await some.TeeSomeAsync((_, _) => { seenSome = true; return Task.CompletedTask; });
        await Assert.That(seenSome).IsTrue();
        await Assert.That(someTee.Expect("ok")).IsEqualTo(5);

        var optNone = Option.None<int>();
        var seenNone = false;
        var noneTee = await optNone.TeeNoneAsync(_ => { seenNone = true; return Task.CompletedTask; });
        await Assert.That(seenNone).IsTrue();
        await Assert.That(noneTee.IsNone).IsTrue();

        var filterBridge = await some.FilterAsync((v, _) => Task.FromResult(v > 10));
        await Assert.That(filterBridge.IsNone).IsTrue();

        var mapSomeBridge = await some.MapSomeAsync((v, _) => Task.FromResult(v * 2));
        await Assert.That(mapSomeBridge.Expect("ok")).IsEqualTo(10);

        var bindNoneBridge = await optNone.BindNoneAsync(_ => Task.FromResult(Option.Some(99)));
        await Assert.That(bindNoneBridge.Expect("ok")).IsEqualTo(99);
    }
}
