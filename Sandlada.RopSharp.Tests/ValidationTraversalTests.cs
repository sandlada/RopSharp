using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ValidationTraversalTests
{
    [Test]
    public async Task TraverseValidation_AllSuccess_ReturnsValuesInOrder()
    {
        var result = Rop.TraverseValidation([1, 2, 3], value => Rop.Ok(value * 10));

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 10, 20, 30);
    }

    [Test]
    public async Task TraverseValidation_CollectsEveryErrorInElementOrder()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = Rop.TraverseValidation(
            [1, 2, 3],
            value => value == 2 ? Rop.Ok<int, RopError>(value) : Rop.Err<int>(value == 1 ? first : second));

        await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), first, second);
    }

    [Test]
    public async Task TraverseValidation_MapsEveryElementEvenAfterFailure()
    {
        var calls = 0;

        var result = Rop.TraverseValidation(
            [1, 2, 3],
            value =>
            {
                calls++;
                return Rop.Err<int>(Sample.Error());
            });

        await Assert.That(calls).IsEqualTo(3);
        await Assert.That(result.ErrOrThrow().Count).IsEqualTo(3);
    }

    [Test]
    public async Task TraverseValidation_MixedTracks_KeepsOnlySuccesses()
    {
        var result = Rop.TraverseValidation(
            [1, 2, 3, 4],
            value => value % 2 == 0 ? Rop.Ok(value) : Rop.Err<int>(Sample.Error()));

        await TestSupport.AssertSequenceAsync(result.ErrOrThrow().Select(error => error.Code), "E_TEST", "E_TEST");
        await Assert.That(result.IsErr).IsTrue();
    }

    [Test]
    public async Task TraverseValidation_EmptySource_ReturnsOkEmpty()
    {
        var result = Rop.TraverseValidation(Array.Empty<int>(), value => Rop.Ok(value));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseValidation_WithNullSource_ThrowsArgumentNull()
    {
        var exception = TestSupport.Catch(() =>
            Rop.TraverseValidation<int, int, RopError>(null!, value => Rop.Ok(value)));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidation_WithNullMapper_ThrowsArgumentNull()
    {
        var exception = TestSupport.Catch(() =>
            Rop.TraverseValidation<int, int, RopError>([1], null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task SequenceValidationAsync_AllOk_ReturnsValuesInSourceOrder()
    {
        var result = await Rop.SequenceValidationAsync(new[]
        {
            Task.FromResult(Rop.Ok(1)),
            Task.FromResult(Rop.Ok(2)),
            Task.FromResult(Rop.Ok(3)),
        });

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 1, 2, 3);
    }

    [Test]
    public async Task SequenceValidationAsync_CollectsEveryErrorInSourceOrder()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = await Rop.SequenceValidationAsync(new[]
        {
            Task.FromResult(Rop.Err<int>(first)),
            Task.FromResult(Rop.Ok(2)),
            Task.FromResult(Rop.Err<int>(second)),
        });

        await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), first, second);
    }

    [Test]
    public async Task SequenceValidationAsync_AwaitsEveryTaskBeforeDeciding()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = Rop.SequenceValidationAsync(new[]
        {
            Task.FromResult(Rop.Err<int>(first)),
            pending.Task,
        });

        await Assert.That(operation.IsCompleted).IsFalse();
        pending.SetResult(Rop.Err<int>(second));

        await TestSupport.AssertSequenceAsync((await operation).ErrOrThrow(), first, second);
    }

    [Test]
    public async Task SequenceValidationAsync_EmptySource_ReturnsOkEmpty()
    {
        var result = await Rop.SequenceValidationAsync(Array.Empty<Task<Result<int, RopError>>>());

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceValidationAsync_WithNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.SequenceValidationAsync(new Task<Result<int, RopError>>[] { null! }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task SequenceValidationAsync_WithCancelledToken_ThrowsBeforeAwaiting()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() =>
            Rop.SequenceValidationAsync([pending.Task], cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Rop.Ok(1));
    }

    [Test]
    public async Task SequenceValidationAsync_WithCancelledTokenWhilePending_Throws()
    {
        using var cts = new CancellationTokenSource();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = Rop.SequenceValidationAsync([pending.Task], cts.Token);
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => operation);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Rop.Ok(1));
    }

    [Test]
    public async Task SequenceValidationAsync_WithFaultedTask_PropagatesFault()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.SequenceValidationAsync(new[]
            {
                Task.FromException<Result<int, RopError>>(new InvalidOperationException("boom")),
            }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_AllOk_ReturnsValuesInSourceOrder()
    {
        var result = await Rop.TraverseValidationAsync(
            [1, 2, 3],
            value => Task.FromResult(Rop.Ok(value * 10)));

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 10, 20, 30);
    }

    [Test]
    public async Task TraverseValidationAsync_CollectsEveryErrorAndMapsEveryElement()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");
        var calls = 0;

        var result = await Rop.TraverseValidationAsync(
            [1, 2, 3],
            value =>
            {
                calls++;
                return Task.FromResult(
                    value == 2 ? Rop.Ok<int, RopError>(value) : Rop.Err<int>(value == 1 ? first : second));
            });

        await Assert.That(calls).IsEqualTo(3);
        await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), first, second);
    }

    [Test]
    public async Task TraverseValidationAsync_EmptySource_ReturnsOkEmpty()
    {
        var result = await Rop.TraverseValidationAsync(
            Array.Empty<int>(),
            value => Task.FromResult(Rop.Ok(value)));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseValidationAsync<int, int, RopError>([1], _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullSource_ThrowsArgumentNull()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseValidationAsync<int, int, RopError>(null!, value => Task.FromResult(Rop.Ok(value))));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullMapper_ThrowsArgumentNull()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseValidationAsync<int, int, RopError>(
                [1],
                (Func<int, Task<Result<int, RopError>>>)null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithCancelledToken_DoesNotInvokeMapper()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var calls = 0;

        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseValidationAsync(
                [1, 2],
                value =>
                {
                    calls++;
                    return Task.FromResult(Rop.Ok(value));
                },
                cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(calls).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseValidationAsync_WithCancelledTokenWhilePending_Throws()
    {
        using var cts = new CancellationTokenSource();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = Rop.TraverseValidationAsync([1], _ => pending.Task, cts.Token);
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => operation);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Rop.Ok(1));
    }

    [Test]
    public async Task TraverseValidationAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = default(CancellationToken);

        var result = await Rop.TraverseValidationAsync(
            [1, 2],
            (value, token) =>
            {
                observed = token;
                return Task.FromResult(Rop.Ok(value));
            },
            cts.Token);

        await Assert.That(observed).IsEqualTo(cts.Token);
        await TestSupport.AssertSequenceAsync(result.OrThrow(), 1, 2);
    }

    [Test]
    public async Task TraverseValidationAsync_WithCancellationAwareMapperAndNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseValidationAsync<int, int, RopError>([1], (_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }
}
