using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncTests
{
    [Test]
    public async Task AsTask_WrapsOption()
    {
        var option = await Opt.Some(5).AsTask();

        await Assert.That(option.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task MapSomeAsync_WithSyncMapper()
    {
        var option = await Task.FromResult(Opt.Some(2)).MapSomeAsync(value => value * 10);

        await Assert.That(option.OrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task MapSomeAsync_WithAsyncMapper()
    {
        var option = await Task.FromResult(Opt.Some(2)).MapSomeAsync(value => Task.FromResult(value * 10));

        await Assert.That(option.OrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task MapSomeAsync_WithCancellationAwareMapper()
    {
        using var cts = new CancellationTokenSource();

        var option = await Task.FromResult(Opt.Some(2))
            .MapSomeAsync((value, token) => Task.FromResult(token == cts.Token ? value + 1 : -1), cts.Token);

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task MapSomeAsync_DoesNotInvokeMapperOnNone()
    {
        var called = false;

        var option = await Task.FromResult(Opt.None<int>()).MapSomeAsync(value =>
        {
            called = true;
            return value;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSomeAsync_ChainsTaskOptions()
    {
        var option = await Task.FromResult(Opt.Some(2))
            .BindSomeAsync(value => Task.FromResult(Opt.Some(value * 3)))
            .BindSomeAsync(value => Task.FromResult(Opt.Some(value + 1)));

        await Assert.That(option.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task BindSomeAsync_WithCancellationAwareBinder()
    {
        using var cts = new CancellationTokenSource();

        var option = await Task.FromResult(Opt.Some(2))
            .BindSomeAsync((value, token) => Task.FromResult(Opt.Some(token == cts.Token ? value + 1 : 0)), cts.Token);

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task BindSomeAsync_FromSyncOption()
    {
        var option = await Opt.Some(2).BindSomeAsync(value => Task.FromResult(Opt.Some(value * 4)));

        await Assert.That(option.OrThrow()).IsEqualTo(8);
    }

    [Test]
    public async Task BindNoneAsync_Recovers()
    {
        var option = await Task.FromResult(Opt.None<int>()).BindNoneAsync(() => Task.FromResult(Opt.Some(7)));

        await Assert.That(option.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task BindNoneAsync_WithCancellationAwareFallback()
    {
        using var cts = new CancellationTokenSource();

        var option = await Task.FromResult(Opt.None<int>())
            .BindNoneAsync(token => Task.FromResult(Opt.Some(token == cts.Token ? 1 : 0)), cts.Token);

        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task BindNoneAsync_DoesNotInvokeFallbackOnSome()
    {
        var called = false;

        var option = await Task.FromResult(Opt.Some(1)).BindNoneAsync(() =>
        {
            called = true;
            return Task.FromResult(Opt.Some(9));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task TeeSomeAsync_ObservesPresentValue()
    {
        var observed = 0;

        var option = await Task.FromResult(Opt.Some(3)).TeeSomeAsync(value =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsEqualTo(3);
        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task TeeNoneAsync_ObservesEmptyTrack()
    {
        var observed = false;

        var option = await Task.FromResult(Opt.None<int>()).TeeNoneAsync(() =>
        {
            observed = true;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsTrue();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_WithSyncPredicate()
    {
        var kept = await Task.FromResult(Opt.Some(2)).FilterAsync(value => value % 2 == 0);
        var dropped = await Task.FromResult(Opt.Some(3)).FilterAsync(value => value % 2 == 0);

        await Assert.That(kept.OrThrow()).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_WithAsyncPredicate()
    {
        var kept = await Task.FromResult(Opt.Some(2)).FilterAsync(value => Task.FromResult(value % 2 == 0));
        var dropped = await Task.FromResult(Opt.Some(3)).FilterAsync(value => Task.FromResult(value % 2 == 0));

        await Assert.That(kept.OrThrow()).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_WithCancelledTokenAndPendingTask_ThrowsAndSkipsMapper()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var called = false;

        var task = pending.Task.MapSomeAsync(
            value =>
            {
                called = true;
                return value;
            },
            cts.Token);

        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(called).IsFalse();
        pending.SetResult(Opt.Some(1));
    }

    [Test]
    public async Task MapSomeAsync_WithCancelledTokenAndCompletedSome_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = Task.FromResult(Opt.Some(1)).MapSomeAsync(value => value + 1, cts.Token);

        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_WithCancelledTokenAndCompletedNone_PassesThrough()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var option = await Task.FromResult(Opt.None<int>()).MapSomeAsync(value => value + 1, cts.Token);

        await Assert.That(option.IsNone).IsTrue();
    }
}
