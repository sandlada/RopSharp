using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Tests added after the first coverage run to exercise paths that were only partially covered:
/// the cancellation-aware binder overloads on a synchronous result receiver, the two-argument LINQ
/// <c>SelectMany</c> overloads, opposite-track async lambda overloads, and a few guard/invariant branches.
/// </summary>
public sealed class CoverageGapTests
{
    [Test]
    public async Task BindOkAsync_OnSyncResult_WithCancellationAwareBinder()
    {
        using var cts = new CancellationTokenSource();

        var result = await Rop.Ok(2)
            .BindOkAsync((value, token) => Task.FromResult(Rop.Ok(value + (token == cts.Token ? 1 : 0))), cts.Token);

        await Assert.That(result.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task BindOkAsync_OnSyncResult_WithCancellationAwareBinder_PropagatesError()
    {
        var error = Sample.Error("E_BIND");
        var called = false;

        var result = await Rop.Err<int>(error).BindOkAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(Rop.Ok(value));
        }, CancellationToken.None);

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task BindOkAsync_OnSyncResult_WithCancellationAwareBinder_HonorsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var called = false;

        var exception = await TestSupport.CatchAsync(() => Rop.Ok(1).BindOkAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(Rop.Ok(value));
        }, cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task BindErrAsync_OnSyncResult_WithCancellationAwareFallback()
    {
        using var cts = new CancellationTokenSource();

        var result = await Rop.Err<int>(Sample.Error())
            .BindErrAsync((_, token) => Task.FromResult(Rop.Ok(token == cts.Token ? 1 : 0)), cts.Token);

        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task BindErrAsync_OnSyncResult_WithCancellationAwareFallback_KeepsSuccess()
    {
        var result = await Rop.Ok(1)
            .BindErrAsync((_, _) => Task.FromResult(Rop.Ok(2)), CancellationToken.None);

        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task ResultLinq_SelectManyTwoArgument_Composes()
    {
        var query = Rop.Ok(2).SelectMany(value => Rop.Ok(value * 3));

        await Assert.That(query.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task OptionLinq_SelectAndTwoArgumentSelectMany_Compose()
    {
        var projected = Opt.Some(2).Select(value => value + 1);
        var bound = Opt.Some(2).SelectMany(value => Opt.Some(value * 3));

        await Assert.That(projected.OrThrow()).IsEqualTo(3);
        await Assert.That(bound.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task Unit_ObjectEqualityAndHashCode()
    {
        await Assert.That(Unit.Value.Equals((object)Unit.Value)).IsTrue();
        await Assert.That(Unit.Value.Equals((object)"not a unit")).IsFalse();
        await Assert.That(Unit.Value.Equals(null)).IsFalse();
        await Assert.That(Unit.Value.GetHashCode()).IsEqualTo(0);
    }

    [Test]
    public async Task Option_NoneHashCodeAndInequality()
    {
        await Assert.That(Opt.None<int>().GetHashCode()).IsEqualTo(0);
        await Assert.That(Opt.None<int>() != Opt.Some(1)).IsTrue();
        await Assert.That(Opt.None<string>().Equals((object)Opt.None<string>())).IsTrue();
    }

    [Test]
    public async Task BindSomeNullableValue_OnNone_DoesNotInvokeBinder()
    {
        var called = false;

        var option = Opt.None<Sample.Profile>().BindSomeNullableValue(value =>
        {
            called = true;
            return value.Score;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_OnNone_WithAsyncAndCancellationAwareMappers_SkipsThem()
    {
        var called = false;

        var asyncResult = await Task.FromResult(Opt.None<int>()).MapSomeAsync(value =>
        {
            called = true;
            return Task.FromResult(value + 1);
        });

        var ctResult = await Task.FromResult(Opt.None<int>()).MapSomeAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(value + 1);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(asyncResult.IsNone).IsTrue();
        await Assert.That(ctResult.IsNone).IsTrue();
    }

    [Test]
    public async Task MapErrAsync_OnSuccess_WithAsyncAndCancellationAwareMappers_PassesThrough()
    {
        var called = false;

        var asyncResult = await Task.FromResult(Rop.Ok(1)).MapErrAsync(error =>
        {
            called = true;
            return Task.FromResult(error);
        });

        var ctResult = await Task.FromResult(Rop.Ok(1)).MapErrAsync((error, _) =>
        {
            called = true;
            return Task.FromResult(error);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(asyncResult.OrThrow()).IsEqualTo(1);
        await Assert.That(ctResult.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task TeeAsync_WithCancellationAwareActions_SkipTheOppositeTrack()
    {
        var called = false;

        var teeOk = await Task.FromResult(Rop.Err<int>(Sample.Error())).TeeOkAsync((_, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        var teeErr = await Task.FromResult(Rop.Ok(1)).TeeErrAsync((_, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(teeOk.IsErr).IsTrue();
        await Assert.That(teeErr.IsOk).IsTrue();
    }

    [Test]
    public async Task OptionTeeAsync_WithCancellationAwareActions_SkipTheOppositeTrack()
    {
        var called = false;

        var teeSome = await Task.FromResult(Opt.None<int>()).TeeSomeAsync((_, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        var teeNone = await Task.FromResult(Opt.Some(1)).TeeNoneAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(teeSome.IsNone).IsTrue();
        await Assert.That(teeNone.IsSome).IsTrue();
    }

    [Test]
    public async Task FilterAsync_WithAsyncPredicateOnNone_SkipsPredicate()
    {
        var called = false;

        var option = await Task.FromResult(Opt.None<int>()).FilterAsync(value =>
        {
            called = true;
            return Task.FromResult(value > 0);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithEmptySource_ReturnsEmptyOk()
    {
        var result = await Rop.TraverseAsync(Array.Empty<int>(), value => Task.FromResult(Rop.Ok(value)));

        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task TryAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<int>(() => null!, _ => Sample.Error()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task Try_WithNullErrorFactoryResult_ThrowsInvalidOperation()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Try<int>(() => throw new InvalidOperationException("boom"), _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithNullErrorFactoryResult_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<int>(
                () => Task.FromException<int>(new InvalidOperationException("boom")),
                _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindOkAsync_TaskReceiver_WithAsyncBinder_PropagatesError()
    {
        var error = Sample.Error("E_TASK");
        var called = false;

        var result = await Task.FromResult(Rop.Err<int>(error)).BindOkAsync(value =>
        {
            called = true;
            return Task.FromResult(Rop.Ok(value));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task BindOkAsync_SyncReceiver_NonCancellationAware_PropagatesError()
    {
        var error = Sample.Error("E_SYNC");
        var called = false;

        var result = await Rop.Err<int>(error).BindOkAsync(value =>
        {
            called = true;
            return Task.FromResult(Rop.Ok(value));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task BindErrAsync_SyncReceiver_NonCancellationAware_KeepsSuccess()
    {
        var result = await Rop.Ok(1).BindErrAsync(_ => Task.FromResult(Rop.Ok(9)));

        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task BindAsync_NullReturnedTask_FaultsWithInvalidOperation()
    {
        var bindOk = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Ok(1)).BindOkAsync<int, int, RopError>(_ => null!));
        var bindErr = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Err<int>(Sample.Error())).BindErrAsync<int, RopError>(_ => null!));
        var bindSome = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.Some(1)).BindSomeAsync<int, int>(_ => null!));
        var bindNone = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.None<int>()).BindNoneAsync<int>(_ => null!));

        await Assert.That(bindOk is InvalidOperationException).IsTrue();
        await Assert.That(bindErr is InvalidOperationException).IsTrue();
        await Assert.That(bindSome is InvalidOperationException).IsTrue();
        await Assert.That(bindNone is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindNoneAsync_BothOverloads_HandleNoneAndSome()
    {
        var noArgNone = await Task.FromResult(Opt.None<int>()).BindNoneAsync(() => Task.FromResult(Opt.Some(5)));
        var noArgSome = await Task.FromResult(Opt.Some(1)).BindNoneAsync(() => Task.FromResult(Opt.Some(5)));
        var ctNone = await Task.FromResult(Opt.None<int>()).BindNoneAsync(_ => Task.FromResult(Opt.Some(7)));
        var ctSome = await Task.FromResult(Opt.Some(1)).BindNoneAsync(_ => Task.FromResult(Opt.Some(9)));

        await Assert.That(noArgNone.OrThrow()).IsEqualTo(5);
        await Assert.That(noArgSome.OrThrow()).IsEqualTo(1);
        await Assert.That(ctNone.OrThrow()).IsEqualTo(7);
        await Assert.That(ctSome.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task TryAsync_SynchronousOperationCanceled_Rethrows()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<int>(() => throw new OperationCanceledException(), _ => Sample.Error()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task MapOkAsync_AllAsyncMapperOverloads_PropagateError()
    {
        var error = Sample.Error("E_MAP");

        var taskReceiver = await Task.FromResult(Rop.Err<int>(error)).MapOkAsync(value => Task.FromResult(value + 1));
        var taskReceiverCt = await Task.FromResult(Rop.Err<int>(error))
            .MapOkAsync((value, _) => Task.FromResult(value + 1));
        var resultReceiver = await Rop.Err<int>(error).MapOkAsync(value => Task.FromResult(value + 1));

        await Assert.That(taskReceiver.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(taskReceiverCt.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(resultReceiver.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task BindOkAsync_CancellationAwareOverloads_PropagateError()
    {
        var error = Sample.Error("E_BIND");

        var taskReceiver = await Task.FromResult(Rop.Err<int>(error))
            .BindOkAsync((value, _) => Task.FromResult(Rop.Ok(value)));

        await Assert.That(taskReceiver.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task BindOkAsync_ResultReceiver_CancellationAware_NullBinderTask_Faults()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.Ok(1).BindOkAsync<int, int, RopError>((_, _) => null!, CancellationToken.None));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindErrAsync_AsyncFallbacks_KeepSuccess()
    {
        var taskReceiver = await Task.FromResult(Rop.Ok(1)).BindErrAsync(_ => Task.FromResult(Rop.Ok(2)));
        var taskReceiverCt = await Task.FromResult(Rop.Ok(1)).BindErrAsync((_, _) => Task.FromResult(Rop.Ok(3)));

        await Assert.That(taskReceiver.OrThrow()).IsEqualTo(1);
        await Assert.That(taskReceiverCt.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task BindErrAsync_ResultReceiver_CancellationAware_NullFallbackTask_Faults()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.Err<int>(Sample.Error())
                .BindErrAsync<int, RopError>((_, _) => null!, CancellationToken.None));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindSomeAsync_AllBinderOverloads_HandleNone()
    {
        var taskReceiver = await Task.FromResult(Opt.None<int>()).BindSomeAsync(value => Task.FromResult(Opt.Some(value)));
        var taskReceiverCt = await Task.FromResult(Opt.None<int>())
            .BindSomeAsync((value, _) => Task.FromResult(Opt.Some(value)));
        var optionReceiver = await Opt.None<int>().BindSomeAsync(value => Task.FromResult(Opt.Some(value)));

        await Assert.That(taskReceiver.IsNone).IsTrue();
        await Assert.That(taskReceiverCt.IsNone).IsTrue();
        await Assert.That(optionReceiver.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_SyncPredicate_OnNone_ReturnsNone()
    {
        var option = await Task.FromResult(Opt.None<int>()).FilterAsync(value => value > 0);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Combine3_WithFirstError_ReturnsFirstError()
    {
        var error = Sample.Error("E_FIRST");
        var mergerCalled = false;

        var result = Rop.Combine(
            Rop.Err<int>(error),
            Rop.Ok(2),
            Rop.Ok(3),
            (int a, int b, int c) =>
            {
                mergerCalled = true;
                return a + b + c;
            });

        await Assert.That(mergerCalled).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Result_OkWithNullPayload_HasStableEqualityAndHashCode()
    {
        var first = Rop.Ok<string, RopError>("x").MapOk(_ => (string?)null);
        var second = Rop.Ok<string, RopError>("y").MapOk(_ => (string?)null);

        await Assert.That(first.IsOk).IsTrue();
        await Assert.That(first.Equals(second)).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task RopError_GetHashCode_IsStableWithAndWithoutDetails()
    {
        var withDetails = RopError.Create("E_A", "message", "details");
        var withoutDetails = RopError.Create("E_B", "message");

        await Assert.That(withDetails.GetHashCode()).IsEqualTo(RopError.Create("E_A", "message", "details").GetHashCode());
        await Assert.That(withoutDetails.GetHashCode()).IsEqualTo(RopError.Create("E_B", "message").GetHashCode());
        await Assert.That(withDetails.GetHashCode()).IsNotEqualTo(withoutDetails.GetHashCode());
    }

    [Test]
    public async Task Transpose_OptionOfResultWithNullPayload_BecomesOkNone()
    {
        var inner = Rop.Ok<string, RopError>("x").MapOk(_ => (string?)null);

        var transposed = Opt.Some(inner).Transpose();

        await Assert.That(transposed.IsOk).IsTrue();
        await Assert.That(transposed.OrThrow().IsNone).IsTrue();
    }
}
