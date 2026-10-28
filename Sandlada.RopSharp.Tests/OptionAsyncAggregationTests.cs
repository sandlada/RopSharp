using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncAggregationTests
{
    [Test]
    public async Task CombineAsync_TwoSome_Combines()
    {
        var option = await Opt.CombineAsync(
            Task.FromResult(Opt.Some(1)),
            Task.FromResult(Opt.Some(2)),
            static (first, second) => first + second);

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task CombineAsync_TwoWithNone_ReturnsNoneAndSkipsMerger()
    {
        var called = false;

        var option = await Opt.CombineAsync(
            Task.FromResult(Opt.Some(1)),
            Task.FromResult(Opt.None<int>()),
            (first, second) =>
            {
                called = true;
                return first + second;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task CombineAsync_ThreeSome_Combines()
    {
        var option = await Opt.CombineAsync(
            Task.FromResult(Opt.Some(1)),
            Task.FromResult(Opt.Some(2)),
            Task.FromResult(Opt.Some(3)),
            static (first, second, third) => first + second + third);

        await Assert.That(option.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task CombineAsync_ThreeWithNone_ReturnsNone()
    {
        var option = await Opt.CombineAsync(
            Task.FromResult(Opt.Some(1)),
            Task.FromResult(Opt.None<int>()),
            Task.FromResult(Opt.Some(3)),
            static (first, second, third) => first + second + third);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task CombineAsync_WithCancelledToken_ThrowsBeforeMerger()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() =>
            Opt.CombineAsync(
                Task.FromResult(Opt.Some(1)),
                Task.FromResult(Opt.Some(2)),
                static (first, second) => first + second,
                cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task ZipAsync_TwoSome_Pairs()
    {
        var option = await Opt.ZipAsync(Task.FromResult(Opt.Some(1)), Task.FromResult(Opt.Some("a")));

        await Assert.That(option.OrThrow()).IsEqualTo((1, "a"));
    }

    [Test]
    public async Task ZipAsync_ThreeSome_Pairs()
    {
        var option = await Opt.ZipAsync(
            Task.FromResult(Opt.Some(1)),
            Task.FromResult(Opt.Some("a")),
            Task.FromResult(Opt.Some(true)));

        await Assert.That(option.OrThrow()).IsEqualTo((1, "a", true));
    }

    [Test]
    public async Task ZipAsync_WithNone_ReturnsNone()
    {
        var option = await Opt.ZipAsync(Task.FromResult(Opt.Some(1)), Task.FromResult(Opt.None<int>()));

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SequenceAsync_AllSome_PreservesOrder()
    {
        var option = await Opt.SequenceAsync(
        [
            Task.FromResult(Opt.Some(1)),
            Task.FromResult(Opt.Some(2)),
            Task.FromResult(Opt.Some(3)),
        ]);

        await Assert.That(option.IsSome).IsTrue();
        await TestSupport.AssertSequenceAsync(option.OrThrow(), [1, 2, 3]);
    }

    [Test]
    public async Task SequenceAsync_Empty_ReturnsSomeEmpty()
    {
        var option = await Opt.SequenceAsync<int>([]);

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceAsync_WithNone_ShortCircuitsAndDoesNotAwaitLaterTasks()
    {
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var option = await Opt.SequenceAsync(
        [
            Task.FromResult(Opt.Some(1)),
            Task.FromResult(Opt.None<int>()),
            pending.Task,
        ]);

        await Assert.That(option.IsNone).IsTrue();
        pending.SetResult(Opt.Some(3));
    }

    [Test]
    public async Task SequenceAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Opt.SequenceAsync(new Task<Option<int>>[] { null! }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task SequenceAsync_WithCancelledTokenAndPendingTask_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() => Opt.SequenceAsync([pending.Task], cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Opt.Some(1));
    }

    [Test]
    public async Task TraverseAsync_AllSome_PreservesOrder()
    {
        var option = await Opt.TraverseAsync(
            [1, 2, 3],
            value => Task.FromResult(Opt.Some(value * 10)));

        await Assert.That(option.IsSome).IsTrue();
        await TestSupport.AssertSequenceAsync(option.OrThrow(), [10, 20, 30]);
    }

    [Test]
    public async Task TraverseAsync_Empty_ReturnsSomeEmpty()
    {
        var option = await Opt.TraverseAsync(
            Array.Empty<int>(),
            value => Task.FromResult(Opt.Some(value)));

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseAsync_WithNone_StopsMapping()
    {
        var calls = 0;

        var option = await Opt.TraverseAsync(
            [1, 2, 3],
            value =>
            {
                calls++;
                return Task.FromResult(value == 2 ? Opt.None<int>() : Opt.Some(value));
            });

        await Assert.That(option.IsNone).IsTrue();
        await Assert.That(calls).IsEqualTo(2);
    }

    [Test]
    public async Task TraverseAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();

        var option = await Opt.TraverseAsync(
            [1, 2],
            (value, token) => Task.FromResult(token == cts.Token ? Opt.Some(value * 2) : Opt.None<int>()),
            cts.Token);

        await Assert.That(option.IsSome).IsTrue();
        await TestSupport.AssertSequenceAsync(option.OrThrow(), [2, 4]);
    }

    [Test]
    public async Task TraverseAsync_WithCancellationAwareMapperAndNone_ReturnsNone()
    {
        var option = await Opt.TraverseAsync(
            [1],
            (value, token) => Task.FromResult(Opt.None<int>()));

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithCancellationAwareMapperAndNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Opt.TraverseAsync<int, int>([1], (_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Opt.TraverseAsync<int, int>([1], _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithCancelledTokenAndPendingTask_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() =>
            Opt.TraverseAsync([1], _ => pending.Task, cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Opt.Some(1));
    }
}
