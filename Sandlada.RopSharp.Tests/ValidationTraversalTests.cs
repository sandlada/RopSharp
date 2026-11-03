using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ValidationTraversalTests
{
    [Test]
    public async Task TraverseValidation_AllSuccess_ReturnsValuesInOrder()
    {
        var result = Result.TraverseValidation([1, 2, 3], value => Result.Ok(value * 10));

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 10, 20, 30);
    }

    [Test]
    public async Task TraverseValidation_CollectsEveryErrorInElementOrder()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = Result.TraverseValidation(
            [1, 2, 3],
            value => value == 2 ? Result.Ok<int, RopError>(value) : Result.Err<int>(value == 1 ? first : second));

        await TestSupport.AssertSequenceAsync(result.ExpectErr(), first, second);
    }

    [Test]
    public async Task TraverseValidation_MapsEveryElementEvenAfterFailure()
    {
        var calls = 0;

        var result = Result.TraverseValidation(
            [1, 2, 3],
            value =>
            {
                calls++;
                return Result.Err<int>(Sample.Error());
            });

        await Assert.That(calls).IsEqualTo(3);
        await Assert.That(result.ExpectErr().Count).IsEqualTo(3);
    }

    [Test]
    public async Task TraverseValidation_MixedTracks_KeepsOnlySuccesses()
    {
        var result = Result.TraverseValidation(
            [1, 2, 3, 4],
            value => value % 2 == 0 ? Result.Ok(value) : Result.Err<int>(Sample.Error()));

        await TestSupport.AssertSequenceAsync(result.ExpectErr().Select(error => error.Code), "E_TEST", "E_TEST");
        await Assert.That(result.IsErr).IsTrue();
    }

    [Test]
    public async Task TraverseValidation_EmptySource_ReturnsOkEmpty()
    {
        var result = Result.TraverseValidation(Array.Empty<int>(), value => Result.Ok(value));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseValidation_WithNullSource_ThrowsArgumentNull()
    {
        var exception = TestSupport.Catch(() =>
            Result.TraverseValidation<int, int, RopError>(null!, value => Result.Ok(value)));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidation_WithNullMapper_ThrowsArgumentNull()
    {
        var exception = TestSupport.Catch(() =>
            Result.TraverseValidation<int, int, RopError>([1], null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task SequenceValidationAsync_AllOk_ReturnsValuesInSourceOrder()
    {
        var result = await Result.SequenceValidationAsync(new[]
        {
            Task.FromResult(Result.Ok(1)),
            Task.FromResult(Result.Ok(2)),
            Task.FromResult(Result.Ok(3)),
        });

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 1, 2, 3);
    }

    [Test]
    public async Task SequenceValidationAsync_CollectsEveryErrorInSourceOrder()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = await Result.SequenceValidationAsync(new[]
        {
            Task.FromResult(Result.Err<int>(first)),
            Task.FromResult(Result.Ok(2)),
            Task.FromResult(Result.Err<int>(second)),
        });

        await TestSupport.AssertSequenceAsync(result.ExpectErr(), first, second);
    }

    [Test]
    public async Task SequenceValidationAsync_AwaitsEveryTaskBeforeDeciding()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = Result.SequenceValidationAsync(new[]
        {
            Task.FromResult(Result.Err<int>(first)),
            pending.Task,
        });

        await Assert.That(operation.IsCompleted).IsFalse();
        pending.SetResult(Result.Err<int>(second));

        await TestSupport.AssertSequenceAsync((await operation).ExpectErr(), first, second);
    }

    [Test]
    public async Task SequenceValidationAsync_EmptySource_ReturnsOkEmpty()
    {
        var result = await Result.SequenceValidationAsync(Array.Empty<Task<Result<int, RopError>>>());

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceValidationAsync_WithNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.SequenceValidationAsync(new Task<Result<int, RopError>>[] { null! }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task SequenceValidationAsync_WithCancelledToken_ThrowsBeforeAwaiting()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() =>
            Result.SequenceValidationAsync([pending.Task], cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task SequenceValidationAsync_WithCancelledTokenWhilePending_Throws()
    {
        using var cts = new CancellationTokenSource();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = Result.SequenceValidationAsync([pending.Task], cts.Token);
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => operation);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task SequenceValidationAsync_WithFaultedTask_PropagatesFault()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.SequenceValidationAsync(new[]
            {
                Task.FromException<Result<int, RopError>>(new InvalidOperationException("boom")),
            }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_AllOk_ReturnsValuesInSourceOrder()
    {
        var result = await Result.TraverseValidationAsync(
            [1, 2, 3],
            value => Task.FromResult(Result.Ok(value * 10)));

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 10, 20, 30);
    }

    [Test]
    public async Task TraverseValidationAsync_CollectsEveryErrorAndMapsEveryElement()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");
        var calls = 0;

        var result = await Result.TraverseValidationAsync(
            [1, 2, 3],
            value =>
            {
                calls++;
                return Task.FromResult(
                    value == 2 ? Result.Ok<int, RopError>(value) : Result.Err<int>(value == 1 ? first : second));
            });

        await Assert.That(calls).IsEqualTo(3);
        await TestSupport.AssertSequenceAsync(result.ExpectErr(), first, second);
    }

    [Test]
    public async Task TraverseValidationAsync_EmptySource_ReturnsOkEmpty()
    {
        var result = await Result.TraverseValidationAsync(
            Array.Empty<int>(),
            value => Task.FromResult(Result.Ok(value)));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TraverseValidationAsync<int, int, RopError>([1], _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullSource_ThrowsArgumentNull()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TraverseValidationAsync<int, int, RopError>(null!, value => Task.FromResult(Result.Ok(value))));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullMapper_ThrowsArgumentNull()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TraverseValidationAsync<int, int, RopError>(
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
            Result.TraverseValidationAsync(
                [1, 2],
                value =>
                {
                    calls++;
                    return Task.FromResult(Result.Ok(value));
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

        var operation = Result.TraverseValidationAsync([1], _ => pending.Task, cts.Token);
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => operation);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task TraverseValidationAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = default(CancellationToken);

        var result = await Result.TraverseValidationAsync(
            [1, 2],
            (value, token) =>
            {
                observed = token;
                return Task.FromResult(Result.Ok(value));
            },
            cts.Token);

        await Assert.That(observed).IsEqualTo(cts.Token);
        await TestSupport.AssertSequenceAsync(result.Expect("test"), 1, 2);
    }

    [Test]
    public async Task TraverseValidationAsync_WithCancellationAwareMapperAndNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TraverseValidationAsync<int, int, RopError>([1], (_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }
}
