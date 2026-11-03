using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncAggregationTests
{
    [Test]
    public async Task CombineAsync_TwoSome_Combines()
    {
        var option = await Option.CombineAsync(
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.Some(2)),
            static (first, second) => first + second);

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task CombineAsync_TwoWithNone_ReturnsNoneAndSkipsMerger()
    {
        var called = false;

        var option = await Option.CombineAsync(
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.None<int>()),
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
        var option = await Option.CombineAsync(
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.Some(2)),
            Task.FromResult(Option.Some(3)),
            static (first, second, third) => first + second + third);

        await Assert.That(option.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task CombineAsync_ThreeWithNone_ReturnsNone()
    {
        var option = await Option.CombineAsync(
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.None<int>()),
            Task.FromResult(Option.Some(3)),
            static (first, second, third) => first + second + third);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task CombineAsync_WithCancelledToken_ThrowsBeforeMerger()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() =>
            Option.CombineAsync(
                Task.FromResult(Option.Some(1)),
                Task.FromResult(Option.Some(2)),
                static (first, second) => first + second,
                cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task ZipAsync_TwoSome_Pairs()
    {
        var option = await Option.ZipAsync(Task.FromResult(Option.Some(1)), Task.FromResult(Option.Some("a")));

        await Assert.That(option.Expect("test")).IsEqualTo((1, "a"));
    }

    [Test]
    public async Task ZipAsync_ThreeSome_Pairs()
    {
        var option = await Option.ZipAsync(
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.Some("a")),
            Task.FromResult(Option.Some(true)));

        await Assert.That(option.Expect("test")).IsEqualTo((1, "a", true));
    }

    [Test]
    public async Task ZipAsync_WithNone_ReturnsNone()
    {
        var option = await Option.ZipAsync(Task.FromResult(Option.Some(1)), Task.FromResult(Option.None<int>()));

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SequenceAsync_AllSome_PreservesOrder()
    {
        var option = await Option.SequenceAsync(
        [
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.Some(2)),
            Task.FromResult(Option.Some(3)),
        ]);

        await Assert.That(option.IsSome).IsTrue();
        await TestSupport.AssertSequenceAsync(option.Expect("test"), [1, 2, 3]);
    }

    [Test]
    public async Task SequenceAsync_Empty_ReturnsSomeEmpty()
    {
        var option = await Option.SequenceAsync<int>([]);

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceAsync_WithNone_ShortCircuitsAndDoesNotAwaitLaterTasks()
    {
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var option = await Option.SequenceAsync(
        [
            Task.FromResult(Option.Some(1)),
            Task.FromResult(Option.None<int>()),
            pending.Task,
        ]);

        await Assert.That(option.IsNone).IsTrue();
        pending.SetResult(Option.Some(3));
    }

    [Test]
    public async Task SequenceAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Option.SequenceAsync(new Task<Option<int>>[] { null! }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task SequenceAsync_WithCancelledTokenAndPendingTask_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() => Option.SequenceAsync([pending.Task], cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Option.Some(1));
    }

    [Test]
    public async Task TraverseAsync_AllSome_PreservesOrder()
    {
        var option = await Option.TraverseAsync(
            [1, 2, 3],
            value => Task.FromResult(Option.Some(value * 10)));

        await Assert.That(option.IsSome).IsTrue();
        await TestSupport.AssertSequenceAsync(option.Expect("test"), [10, 20, 30]);
    }

    [Test]
    public async Task TraverseAsync_Empty_ReturnsSomeEmpty()
    {
        var option = await Option.TraverseAsync(
            Array.Empty<int>(),
            value => Task.FromResult(Option.Some(value)));

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseAsync_WithNone_StopsMapping()
    {
        var calls = 0;

        var option = await Option.TraverseAsync(
            [1, 2, 3],
            value =>
            {
                calls++;
                return Task.FromResult(value == 2 ? Option.None<int>() : Option.Some(value));
            });

        await Assert.That(option.IsNone).IsTrue();
        await Assert.That(calls).IsEqualTo(2);
    }

    [Test]
    public async Task TraverseAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();

        var option = await Option.TraverseAsync(
            [1, 2],
            (value, token) => Task.FromResult(token == cts.Token ? Option.Some(value * 2) : Option.None<int>()),
            cts.Token);

        await Assert.That(option.IsSome).IsTrue();
        await TestSupport.AssertSequenceAsync(option.Expect("test"), [2, 4]);
    }

    [Test]
    public async Task TraverseAsync_WithCancellationAwareMapperAndNone_ReturnsNone()
    {
        var option = await Option.TraverseAsync(
            [1],
            (value, token) => Task.FromResult(Option.None<int>()));

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithCancellationAwareMapperAndNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Option.TraverseAsync<int, int>([1], (_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithNullTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Option.TraverseAsync<int, int>([1], _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithCancelledTokenAndPendingTask_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Option<int>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() =>
            Option.TraverseAsync([1], _ => pending.Task, cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Option.Some(1));
    }
}
