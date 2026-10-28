using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncParityTests
{
    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithAsyncMapper()
    {
        var option = await Opt.Some(2).MapSomeAsync(value => Task.FromResult(value * 10));

        await Assert.That(option.OrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithCancellationAwareMapper()
    {
        using var cts = new CancellationTokenSource();

        var option = await Opt.Some(2)
            .MapSomeAsync((value, token) => Task.FromResult(token == cts.Token ? value + 1 : -1), cts.Token);

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_DoesNotInvokeMapperOnNone()
    {
        var called = false;

        var option = await Opt.None<int>().MapSomeAsync(value =>
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
            Opt.Some(1).MapSomeAsync(
                value =>
                {
                    called = true;
                    return Task.FromResult(value);
                },
                cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithCancelledTokenAndNone_PassesThrough()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var option = await Opt.None<int>().MapSomeAsync(value => Task.FromResult(value + 1), cts.Token);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_FromSyncOption_WithCancellationAwareMapper_OnNone_PassesThrough()
    {
        var called = false;

        var option = await Opt.None<int>().MapSomeAsync((value, token) =>
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

        var option = await Opt.Some(2)
            .BindSomeAsync((value, token) => Task.FromResult(Opt.Some(token == cts.Token ? value + 1 : 0)), cts.Token);

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task BindSomeAsync_FromSyncOption_WithCancellationAwareBinder_OnNone_PassesThrough()
    {
        var called = false;

        var option = await Opt.None<int>().BindSomeAsync((value, token) =>
        {
            called = true;
            return Task.FromResult(Opt.Some(value));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_Recovers()
    {
        var option = await Opt.None<int>().BindNoneAsync(() => Task.FromResult(Opt.Some(7)));

        await Assert.That(option.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_DoesNotInvokeFallbackOnSome()
    {
        var called = false;

        var option = await Opt.Some(1).BindNoneAsync(() =>
        {
            called = true;
            return Task.FromResult(Opt.Some(9));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_WithCancellationAwareFallback()
    {
        using var cts = new CancellationTokenSource();

        var option = await Opt.None<int>()
            .BindNoneAsync(token => Task.FromResult(Opt.Some(token == cts.Token ? 1 : 0)), cts.Token);

        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_WithCancellationAwareFallback_OnSome_PassesThrough()
    {
        var called = false;

        var option = await Opt.Some(5).BindNoneAsync(token =>
        {
            called = true;
            return Task.FromResult(Opt.Some(9));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_ObservesPresentValue()
    {
        var observed = 0;

        var option = await Opt.Some(3).TeeSomeAsync(value =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsEqualTo(3);
        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_DoesNotInvokeActionOnNone()
    {
        var called = false;

        var option = await Opt.None<int>().TeeSomeAsync(value =>
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

        var option = await Opt.Some(4).TeeSomeAsync(
            (value, token) =>
            {
                observed = token == cts.Token ? value : -1;
                return Task.CompletedTask;
            },
            cts.Token);

        await Assert.That(observed).IsEqualTo(4);
        await Assert.That(option.OrThrow()).IsEqualTo(4);
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_WithCancellationAwareAction_OnNone_DoesNotInvoke()
    {
        var called = false;

        var option = await Opt.None<int>().TeeSomeAsync((value, token) =>
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

        var option = await Opt.None<int>().TeeNoneAsync(() =>
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

        var option = await Opt.Some(1).TeeNoneAsync(() =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task TeeNoneAsync_FromSyncOption_WithCancellationAwareAction()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        var option = await Opt.None<int>().TeeNoneAsync(
            token =>
            {
                observed = token == cts.Token;
                return Task.CompletedTask;
            },
            cts.Token);

        await Assert.That(observed).IsTrue();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_FromSyncOption_WithCancellationAwareAction_OnSome_DoesNotInvoke()
    {
        var called = false;

        var option = await Opt.Some(1).TeeNoneAsync(token =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithAsyncPredicate()
    {
        var kept = await Opt.Some(2).FilterAsync(value => Task.FromResult(value % 2 == 0));
        var dropped = await Opt.Some(3).FilterAsync(value => Task.FromResult(value % 2 == 0));

        await Assert.That(kept.OrThrow()).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithCancellationAwarePredicate()
    {
        using var cts = new CancellationTokenSource();

        var kept = await Opt.Some(2)
            .FilterAsync((value, token) => Task.FromResult(token == cts.Token && value % 2 == 0), cts.Token);

        await Assert.That(kept.OrThrow()).IsEqualTo(2);
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithCancellationAwarePredicate_OnNone_PassesThrough()
    {
        var called = false;

        var option = await Opt.None<int>().FilterAsync((value, token) =>
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

        var option = await Opt.None<int>().FilterAsync(value =>
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

        var kept = await Task.FromResult(Opt.Some(2))
            .FilterAsync((value, token) => Task.FromResult(token == cts.Token && value % 2 == 0), cts.Token);

        await Assert.That(kept.OrThrow()).IsEqualTo(2);
    }

    [Test]
    public async Task FilterAsync_FromTask_WithCancellationAwarePredicate_OnNone_PassesThrough()
    {
        var called = false;

        var option = await Task.FromResult(Opt.None<int>()).FilterAsync((value, token) =>
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
            Opt.Some(1).MapSomeAsync<int, int>(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindNoneAsync_FromSyncOption_WithNullFallbackTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Opt.None<int>().BindNoneAsync(() => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TeeSomeAsync_FromSyncOption_WithNullActionTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Opt.Some(1).TeeSomeAsync(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task FilterAsync_FromSyncOption_WithNullPredicateTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Opt.Some(1).FilterAsync(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }
}
