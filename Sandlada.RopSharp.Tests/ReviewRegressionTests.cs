using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Regression tests for defects found by the independent adversarial review: a legal nullable success payload must
/// survive every pass-through, cancellation must be observed uniformly by the operation itself, and invalid
/// arguments must be rejected deterministically.
/// </summary>
public sealed class ReviewRegressionTests
{
    // A success carrying null is legal after a nullable mapping (MapOk deliberately preserves it).
    private static Result<string?, RopError> NullSuccess =>
        Rop.Ok<string?, RopError>("bob").MapOk(static _ => (string?)null);

    [Test]
    public async Task MapErr_PreservesNullSuccessPayload()
    {
        var result = NullSuccess.MapErr(static error => error.WithCode("E_OTHER"));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task MapErrAsync_OnTaskWithSyncMapper_PreservesNullSuccessPayload()
    {
        var result = await Task.FromResult(NullSuccess).MapErrAsync(static error => error);

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task MapErrAsync_OnTaskWithAsyncMapper_PreservesNullSuccessPayload()
    {
        var result = await Task.FromResult(NullSuccess).MapErrAsync(static error => Task.FromResult(error));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task MapErrAsync_OnTaskWithCancellationAwareMapper_PreservesNullSuccessPayload()
    {
        var result = await Task.FromResult(NullSuccess)
            .MapErrAsync(static (error, _) => Task.FromResult(error));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task MapErrAsync_OnResultWithAsyncMapper_PreservesNullSuccessPayload()
    {
        var result = await NullSuccess.MapErrAsync(static error => Task.FromResult(error));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task MapErrAsync_OnResultWithCancellationAwareMapper_PreservesNullSuccessPayload()
    {
        var result = await NullSuccess.MapErrAsync(static (error, _) => Task.FromResult(error));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task ToValidation_PreservesNullSuccessPayload()
    {
        var lifted = NullSuccess.ToValidation();

        await Assert.That(lifted.IsOk).IsTrue();
        await Assert.That(lifted.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task ToSingleError_PreservesNullSuccessPayload()
    {
        var collapsed = NullSuccess.ToValidation().ToSingleError();

        await Assert.That(collapsed.IsOk).IsTrue();
        await Assert.That(collapsed.OkOrThrow()).IsNull();
    }

    [Test]
    public async Task Combine_WithSingleNullElement_ThrowsArgumentNull()
    {
        var exception = TestSupport.Catch(() => RopError.Combine(new RopError[] { null! }));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task SequenceAsync_WithCancelledTokenAndCompletedTasks_Throws()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        var exception = await TestSupport.CatchAsync(() =>
            Rop.SequenceAsync([Task.FromResult(Rop.Ok(1))], source.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TraverseAsync_WithCancelledToken_DoesNotInvokeMapper()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        var calls = 0;

        var exception = await TestSupport.CatchAsync(() =>
            Rop.TraverseAsync<int, int, RopError>(
                [1, 2],
                value =>
                {
                    calls++;
                    return Task.FromResult(Rop.Ok(value));
                },
                source.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(calls).IsEqualTo(0);
    }

    [Test]
    public async Task MapErrAsync_WithCancelledTokenOnErrorTrack_DoesNotInvokeMapper()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        var calls = 0;

        var exception = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Err<int>(Sample.Error())).MapErrAsync(
                error =>
                {
                    calls++;
                    return error;
                },
                source.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(calls).IsEqualTo(0);
    }

    [Test]
    public async Task MapOkAsync_WithCancelledTokenOnErrorTrack_PassesThrough()
    {
        // Documented rule: the pass-through track never observes the token, so an already-completed antecedent
        // returns its value even when the token is cancelled.
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        var result = await Task.FromResult(Rop.Err<int>(Sample.Error()))
            .MapOkAsync(static value => value + 1, source.Token);

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_TEST");
    }

    [Test]
    public async Task CombineAsync_WithCancelledTokenOnErrorTrack_PassesThrough()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        var result = await Task.FromResult(Rop.Err<int>(Sample.Error()))
            .TeeOkAsync(static _ => Task.CompletedTask, source.Token);

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_TEST");
    }

    [Test]
    public async Task GetHashCode_WithNullSuccessPayload_IsStableAndEqual()
    {
        // A nullable success is a legal state, so it must participate in equality and hashing.
        var first = NullSuccess;
        var second = NullSuccess;

        await Assert.That(first.Equals(second)).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task GetHashCode_IsConsistentOnEveryTrack()
    {
        var ok = Rop.Ok<int, RopError>(1);
        var nullOk = NullSuccess;
        var err = Rop.Err<int>(Sample.Error());

        await Assert.That(ok.GetHashCode()).IsEqualTo(Rop.Ok<int, RopError>(1).GetHashCode());
        await Assert.That(nullOk.GetHashCode()).IsEqualTo(NullSuccess.GetHashCode());
        await Assert.That(err.GetHashCode()).IsEqualTo(Rop.Err<int>(Sample.Error()).GetHashCode());
    }

    [Test]
    public async Task Validate3_WithFailures_CollectsEveryErrorInOrder()
    {
        var result = Rop.Validate(
            Rop.Ok<int, RopError>(1),
            Rop.Err<int>(Sample.Error("E_B", "second")),
            Rop.Err<int>(Sample.Error("E_C", "third")),
            static (a, b, c) => a + b + c);

        var errors = result.ErrOrThrow();

        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors[0].Code).IsEqualTo("E_B");
        await Assert.That(errors[1].Code).IsEqualTo("E_C");
    }
}
