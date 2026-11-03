using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncParityTests
{
    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithAsyncMapper()
    {
        var option = await OptionTask.From(Option.Some(2)).MapSomeAsync((value, _) => Task.FromResult(value * 10));

        await Assert.That(option.Expect("test")).IsEqualTo(20);
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithCancellationAwareMapper()
    {
        using var cts = new CancellationTokenSource();

        var option = await OptionTask.From(Option.Some(2))
            .WithCancellation(cts.Token).MapSomeAsync((value, token) => Task.FromResult(token == cts.Token ? value + 1 : -1));

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_DoesNotInvokeMapperOnNone()
    {
        var called = false;

        var option = await OptionTask.From(Option.None<int>()).MapSomeAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(value);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithCancelledTokenAndSome_ThrowsAndSkipsMapper()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var called = false;

        var exception = await TestSupport.CatchAsync(() =>
            OptionTask.From(Option.Some(1)).WithCancellation(cts.Token).MapSomeAsync(
                (value, _) =>
                {
                    called = true;
                    return Task.FromResult(value);
                }));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithCancelledTokenAndNone_PassesThrough()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var option = await OptionTask.From(Option.None<int>()).MapSomeAsync((value, _) => Task.FromResult(value + 1));

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithCancellationAwareMapper_OnNone_PassesThrough()
    {
        var called = false;

        var option = await OptionTask.From(Option.None<int>()).MapSomeAsync((value, token) =>
        {
            called = true;
            return Task.FromResult(value);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSomeAsync_FromSyncOption_WithCancellationAwareBinder()
    {
        using var cts = new CancellationTokenSource();

        var option = await OptionTask.From(Option.Some(2))
            .WithCancellation(cts.Token)
            .BindSomeAsync((value, token) => Task.FromResult(Option.Some(token == cts.Token ? value + 1 : 0)));

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task BindSomeAsync_FromSyncOption_WithCancellationAwareBinder_OnNone_PassesThrough()
    {
        var called = false;

        var option = await OptionTask.From(Option.None<int>()).BindSomeAsync((value, token) =>
        {
            called = true;
            return Task.FromResult(Option.Some(value));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_Recovers()
    {
        var option = await OptionTask.From(Option.None<int>()).BindNone(_ => Task.FromResult(Option.Some(7)));

        await Assert.That(option.Expect("test")).IsEqualTo(7);
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_DoesNotInvokeFallbackOnSome()
    {
        var called = false;

        var option = await OptionTask.From(Option.Some(1)).BindNone(_ =>
        {
            called = true;
            return Task.FromResult(Option.Some(9));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_WithCancellationAwareFallback()
    {
        using var cts = new CancellationTokenSource();

        var option = await OptionTask.From(Option.None<int>())
            .WithCancellation(cts.Token).BindNone(token => Task.FromResult(Option.Some(token == cts.Token ? 1 : 0)));

        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_WithCancellationAwareFallback_OnSome_PassesThrough()
    {
        var called = false;

        var option = await OptionTask.From(Option.Some(5)).BindNone(token =>
        {
            called = true;
            return Task.FromResult(Option.Some(9));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.Expect("test")).IsEqualTo(5);
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_ObservesPresentValue()
    {
        var observed = 0;

        var option = await OptionTask.From(Option.Some(3)).TeeSomeAsync((value, _) =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsEqualTo(3);
        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_DoesNotInvokeActionOnNone()
    {
        var called = false;

        var option = await OptionTask.From(Option.None<int>()).TeeSomeAsync((value, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_WithCancellationAwareAction()
    {
        using var cts = new CancellationTokenSource();
        var observed = 0;

        var option = await OptionTask.From(Option.Some(4)).WithCancellation(cts.Token).TeeSomeAsync(
            (value, token) =>
            {
                observed = token == cts.Token ? value : -1;
                return Task.CompletedTask;
            });

        await Assert.That(observed).IsEqualTo(4);
        await Assert.That(option.Expect("test")).IsEqualTo(4);
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_WithCancellationAwareAction_OnNone_DoesNotInvoke()
    {
        var called = false;

        var option = await OptionTask.From(Option.None<int>()).TeeSomeAsync((value, token) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_FromSyncOption_ObservesEmptyTrack()
    {
        var observed = false;

        var option = await OptionTask.From(Option.None<int>()).TeeNoneAsync(_ =>
        {
            observed = true;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsTrue();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_FromSyncOption_DoesNotInvokeActionOnSome()
    {
        var called = false;

        var option = await OptionTask.From(Option.Some(1)).TeeNoneAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task TeeNoneAsync_FromSyncOption_WithCancellationAwareAction()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        var option = await OptionTask.From(Option.None<int>()).WithCancellation(cts.Token).TeeNoneAsync(
            token =>
            {
                observed = token == cts.Token;
                return Task.CompletedTask;
            });

        await Assert.That(observed).IsTrue();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_FromSyncOption_WithCancellationAwareAction_OnSome_DoesNotInvoke()
    {
        var called = false;

        var option = await OptionTask.From(Option.Some(1)).TeeNoneAsync(token =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithAsyncPredicate()
    {
        var kept = await OptionTask.From(Option.Some(2)).FilterAsync((value, _) => Task.FromResult(value % 2 == 0));
        var dropped = await OptionTask.From(Option.Some(3)).FilterAsync((value, _) => Task.FromResult(value % 2 == 0));

        await Assert.That(kept.Expect("test")).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithCancellationAwarePredicate()
    {
        using var cts = new CancellationTokenSource();

        var kept = await OptionTask.From(Option.Some(2))
            .WithCancellation(cts.Token)
            .FilterAsync((value, token) => Task.FromResult(token == cts.Token && value % 2 == 0));

        await Assert.That(kept.Expect("test")).IsEqualTo(2);
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithCancellationAwarePredicate_OnNone_PassesThrough()
    {
        var called = false;

        var option = await OptionTask.From(Option.None<int>()).FilterAsync((value, token) =>
        {
            called = true;
            return Task.FromResult(true);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_DoesNotInvokePredicateOnNone()
    {
        var called = false;

        var option = await OptionTask.From(Option.None<int>()).FilterAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(value > 0);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_FromTask_WithCancellationAwarePredicate()
    {
        using var cts = new CancellationTokenSource();

        var kept = await OptionTask.From(Task.FromResult(Option.Some(2)))
            .WithCancellation(cts.Token)
            .FilterAsync((value, token) => Task.FromResult(token == cts.Token && value % 2 == 0));

        await Assert.That(kept.Expect("test")).IsEqualTo(2);
    }

    [Test]
    public async Task FilterAsync_FromTask_WithCancellationAwarePredicate_OnNone_PassesThrough()
    {
        var called = false;

        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).FilterAsync((value, token) =>
        {
            called = true;
            return Task.FromResult(true);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithNullMapperTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            OptionTask.From(Option.Some(1)).MapSomeAsync((_, _) => (Task<int>)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_WithNullFallbackTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            OptionTask.From(Option.None<int>()).BindNone(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_WithNullActionTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            OptionTask.From(Option.Some(1)).TeeSomeAsync((_, _) => (Task)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithNullPredicateTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(async () =>
            await OptionTask.From(Option.Some(1)).FilterAsync((_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }
}
