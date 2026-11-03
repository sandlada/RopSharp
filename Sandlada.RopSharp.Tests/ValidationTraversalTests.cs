using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ValidationTraversalTests
{
    [Test]
    public async Task TraverseValidation_AllSuccess_ReturnsValuesInOrder()
    {
        var result = Validation.Traverse([1, 2, 3], value => Result.Ok(value * 10));

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 10, 20, 30);
    }

    [Test]
    public async Task TraverseValidation_CollectsEveryErrorInElementOrder()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = Validation.Traverse(
            [1, 2, 3],
            value => value == 2 ? Result.Ok<int, RopError>(value) : Result.Err<int>(value == 1 ? first : second));

        await TestSupport.AssertSequenceAsync(result.ExpectErrors(), first, second);
    }

    [Test]
    public async Task TraverseValidation_MapsEveryElementEvenAfterFailure()
    {
        var calls = 0;

        var result = Validation.Traverse(
            [1, 2, 3],
            value =>
            {
                calls++;
                return Result.Err<int>(Sample.Error());
            });

        await Assert.That(calls).IsEqualTo(3);
        await Assert.That(result.ExpectErrors().Count).IsEqualTo(3);
    }

    [Test]
    public async Task TraverseValidation_MixedTracks_KeepsOnlySuccesses()
    {
        var result = Validation.Traverse(
            [1, 2, 3, 4],
            value => value % 2 == 0 ? Result.Ok(value) : Result.Err<int>(Sample.Error()));

        await TestSupport.AssertSequenceAsync(result.ExpectErrors().Select(error => error.Code), "E_TEST", "E_TEST");
        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task TraverseValidation_EmptySource_ReturnsOkEmpty()
    {
        var result = Validation.Traverse(Array.Empty<int>(), value => Result.Ok(value));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseValidation_WithNullSource_ThrowsArgumentNull()
    {
        var exception = TestSupport.Catch(() =>
            Validation.Traverse<int, int, RopError>(null!, value => Result.Ok(value)));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidation_WithNullMapper_ThrowsArgumentNull()
    {
        var exception = TestSupport.Catch(() =>
            Validation.Traverse<int, int, RopError>([1], null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task SequenceValidationAsync_AllOk_ReturnsValuesInSourceOrder()
    {
        var result = await ValidationTask.Sequence(new[]
        {
            ResultTask.From(Result.Ok(1)),
            ResultTask.From(Result.Ok(2)),
            ResultTask.From(Result.Ok(3)),
        });

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 1, 2, 3);
    }

    [Test]
    public async Task SequenceValidationAsync_CollectsEveryErrorInSourceOrder()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = await ValidationTask.Sequence(new[]
        {
            ResultTask.From(Result.Err<int>(first)),
            ResultTask.From(Result.Ok(2)),
            ResultTask.From(Result.Err<int>(second)),
        });

        await TestSupport.AssertSequenceAsync(result.ExpectErrors(), first, second);
    }

    [Test]
    public async Task SequenceValidationAsync_AwaitsEveryTaskBeforeDeciding()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = ValidationTask.Sequence(new[]
        {
            ResultTask.From(Result.Err<int>(first)),
            ResultTask.From(pending.Task),
        });

        await Assert.That(operation.TaskUnsafe.IsCompleted).IsFalse();
        pending.SetResult(Result.Err<int>(second));

        await TestSupport.AssertSequenceAsync((await operation).ExpectErrors(), first, second);
    }

    [Test]
    public async Task SequenceValidationAsync_EmptySource_ReturnsOkEmpty()
    {
        var result = await ValidationTask.Sequence(Array.Empty<ResultTask<int, RopError>>());

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceValidationAsync_WithNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            ValidationTask.Sequence(new ResultTask<int, RopError>[] { default }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task SequenceValidationAsync_WithCancelledToken_ThrowsBeforeAwaiting()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var exception = await TestSupport.CatchAsync(() =>
            ValidationTask.Sequence([ResultTask.From(pending.Task)], cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task SequenceValidationAsync_WithCancelledTokenWhilePending_Throws()
    {
        using var cts = new CancellationTokenSource();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = ValidationTask.Sequence([ResultTask.From(pending.Task)], cts.Token);
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => operation);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task SequenceValidationAsync_WithFaultedTask_PropagatesFault()
    {
        var exception = await TestSupport.CatchAsync(() =>
            ValidationTask.Sequence(new[]
            {
                ResultTask.From(Task.FromException<Result<int, RopError>>(new InvalidOperationException("boom"))),
            }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_AllOk_ReturnsValuesInSourceOrder()
    {
        var result = await ValidationTask.Traverse([1, 2, 3], (value, _) => Task.FromResult(Result.Ok(value * 10)));

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 10, 20, 30);
    }

    [Test]
    public async Task TraverseValidationAsync_CollectsEveryErrorAndMapsEveryElement()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");
        var calls = 0;

        var result = await ValidationTask.Traverse(
            [1, 2, 3],
            (value, _) =>
            {
                calls++;
                return Task.FromResult(
                    value == 2 ? Result.Ok<int, RopError>(value) : Result.Err<int>(value == 1 ? first : second));
            });

        await Assert.That(calls).IsEqualTo(3);
        await TestSupport.AssertSequenceAsync(result.ExpectErrors(), first, second);
    }

    [Test]
    public async Task TraverseValidationAsync_EmptySource_ReturnsOkEmpty()
    {
        var result = await ValidationTask.Traverse(
            Array.Empty<int>(), (value, _) => Task.FromResult(Result.Ok(value)));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            ValidationTask.Traverse<int, int, RopError>([1], (_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullSource_ThrowsArgumentNull()
    {
        var exception = await TestSupport.CatchAsync(() =>
            ValidationTask.Traverse<int, int, RopError>(null!, (value, _) => Task.FromResult(Result.Ok(value))));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithNullMapper_ThrowsArgumentNull()
    {
        var exception = await TestSupport.CatchAsync(() =>
            ValidationTask.Traverse<int, int, RopError>(
                [1],
                (Func<int, CancellationToken, Task<Result<int, RopError>>>)null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_WithCancelledToken_DoesNotInvokeMapper()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var calls = 0;

        var exception = await TestSupport.CatchAsync(() =>
            ValidationTask.Traverse(
                [1, 2],
                (value, _) =>
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

        var operation = ValidationTask.Traverse([1], (_, _) => pending.Task, cts.Token);
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

        var result = await ValidationTask.Traverse(
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
            ValidationTask.Traverse<int, int, RopError>([1], (_, _) => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }
}
