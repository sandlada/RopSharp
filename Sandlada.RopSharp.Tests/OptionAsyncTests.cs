using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncTests
{
    [Test]
    public async Task AsTask_WrapsOption()
    {
        var option = await OptionTask.From(Option.Some(5));

        await Assert.That(option.Expect("test")).IsEqualTo(5);
    }

    [Test]
    public async Task MapSomeAsync_WithSyncMapper()
    {
        var option = await OptionTask.From(Task.FromResult(Option.Some(2))).MapSome(value => value * 10);

        await Assert.That(option.Expect("test")).IsEqualTo(20);
    }

    [Test]
    public async Task MapSomeAsync_WithAsyncMapper()
    {
        var option = await OptionTask.From(Task.FromResult(Option.Some(2))).MapSomeAsync((value, _) => Task.FromResult(value * 10));

        await Assert.That(option.Expect("test")).IsEqualTo(20);
    }

    [Test]
    public async Task MapSomeAsync_WithCancellationAwareMapper()
    {
        using var cts = new CancellationTokenSource();

        var option = await OptionTask.From(Task.FromResult(Option.Some(2)))
            .WithCancellation(cts.Token).MapSomeAsync((value, token) => Task.FromResult(token == cts.Token ? value + 1 : -1));

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task MapSomeAsync_DoesNotInvokeMapperOnNone()
    {
        var called = false;

        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).MapSomeAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(value);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSomeAsync_ChainsTaskOptions()
    {
        var option = await OptionTask.From(Task.FromResult(Option.Some(2)))
            .BindSomeAsync((value, _) => Task.FromResult(Option.Some(value * 3)))
            .BindSomeAsync((value, _) => Task.FromResult(Option.Some(value + 1)));

        await Assert.That(option.Expect("test")).IsEqualTo(7);
    }

    [Test]
    public async Task BindSomeAsync_WithCancellationAwareBinder()
    {
        using var cts = new CancellationTokenSource();

        var option = await OptionTask.From(Task.FromResult(Option.Some(2)))
            .WithCancellation(cts.Token)
            .BindSomeAsync((value, token) => Task.FromResult(Option.Some(token == cts.Token ? value + 1 : 0)));

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task BindSomeAsync_FromSyncOption()
    {
        var option = await OptionTask.From(Option.Some(2)).BindSomeAsync((value, _) => Task.FromResult(Option.Some(value * 4)));

        await Assert.That(option.Expect("test")).IsEqualTo(8);
    }

    [Test]
    public async Task BindNoneAsync_Recovers()
    {
        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).BindNone(_ => Task.FromResult(Option.Some(7)));

        await Assert.That(option.Expect("test")).IsEqualTo(7);
    }

    [Test]
    public async Task BindNoneAsync_WithCancellationAwareFallback()
    {
        using var cts = new CancellationTokenSource();

        var option = await OptionTask.From(Task.FromResult(Option.None<int>()))
            .WithCancellation(cts.Token).BindNone(token => Task.FromResult(Option.Some(token == cts.Token ? 1 : 0)));

        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task BindNoneAsync_DoesNotInvokeFallbackOnSome()
    {
        var called = false;

        var option = await OptionTask.From(Task.FromResult(Option.Some(1))).BindNone(_ =>
        {
            called = true;
            return Task.FromResult(Option.Some(9));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task TeeSomeAsync_ObservesPresentValue()
    {
        var observed = 0;

        var option = await OptionTask.From(Task.FromResult(Option.Some(3))).TeeSomeAsync((value, _) =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsEqualTo(3);
        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task TeeNoneAsync_ObservesEmptyTrack()
    {
        var observed = false;

        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).TeeNoneAsync(_ =>
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
        var kept = await OptionTask.From(Task.FromResult(Option.Some(2))).Filter(value => value % 2 == 0);
        var dropped = await OptionTask.From(Task.FromResult(Option.Some(3))).Filter(value => value % 2 == 0);

        await Assert.That(kept.Expect("test")).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task FilterAsync_WithAsyncPredicate()
    {
        var kept = await OptionTask.From(Task.FromResult(Option.Some(2))).FilterAsync((value, _) => Task.FromResult(value % 2 == 0));
        var dropped = await OptionTask.From(Task.FromResult(Option.Some(3))).FilterAsync((value, _) => Task.FromResult(value % 2 == 0));

        await Assert.That(kept.Expect("test")).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_WithCancelledTokenAndPendingTask_ThrowsAndSkipsMapper()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var called = false;

        var task = OptionTask.From(pending.Task).WithCancellation(cts.Token).MapSomeAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(value);
        });

        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(called).IsFalse();
        pending.SetResult(Option.Some(1));
    }

    [Test]
    public async Task MapSomeAsync_WithCancelledTokenAndCompletedSome_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = OptionTask.From(Task.FromResult(Option.Some(1))).WithCancellation(cts.Token).MapSomeAsync((value, _) => Task.FromResult(value + 1));

        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task MapSomeAsync_WithCancelledTokenAndCompletedNone_PassesThrough()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).WithCancellation(cts.Token).MapSomeAsync((value, _) => Task.FromResult(value + 1));

        await Assert.That(option.IsNone).IsTrue();
    }
}
