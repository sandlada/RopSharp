using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultAsyncTests
{
    [Test]
    public async Task AsTask_WrapsResult()
    {
        var result = await Result.Ok(5).AsTask();

        await Assert.That(result.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task MapOkAsync_WithSyncMapper()
    {
        var result = await Task.FromResult(Result.Ok(2)).MapOkAsync(value => value * 10);

        await Assert.That(result.OrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task MapOkAsync_WithAsyncMapper_PrefersAsyncOverload()
    {
        var result = await Task.FromResult(Result.Ok(2)).MapOkAsync(value => Task.FromResult(value * 10));

        await Assert.That(result.OrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task MapOkAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();

        var result = await Task.FromResult(Result.Ok(2))
            .MapOkAsync((value, token) => Task.FromResult(token == cts.Token ? value + 1 : -1), cts.Token);

        await Assert.That(result.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task MapOkAsync_FromSyncResult_WithAsyncMapper()
    {
        var result = await Result.Ok(2).MapOkAsync(value => Task.FromResult(value * 3));

        await Assert.That(result.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task MapOkAsync_PropagatesErrorWithoutCallingMapper()
    {
        var called = false;
        var error = Sample.Error("E_X");

        var result = await Task.FromResult(Result.Err<int>(error)).MapOkAsync(value =>
        {
            called = true;
            return value;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task MapErrAsync_MapsErrorOnly()
    {
        var mapped = await Task.FromResult(Result.Err<int>(Sample.Error("E_X")))
            .MapErrAsync(error => error.WithCode("E_Y"));
        var untouched = await Task.FromResult(Result.Ok(1)).MapErrAsync(error => error.WithCode("E_Y"));

        await Assert.That(mapped.ErrOrThrow().Code).IsEqualTo("E_Y");
        await Assert.That(untouched.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task MapErrAsync_WithAsyncMapper()
    {
        var mapped = await Task.FromResult(Result.Err<int>(Sample.Error("E_X")))
            .MapErrAsync(error => Task.FromResult(error.WithCode("E_ASYNC")));

        await Assert.That(mapped.ErrOrThrow().Code).IsEqualTo("E_ASYNC");
    }

    [Test]
    public async Task BindOkAsync_ChainsTaskResults()
    {
        var result = await Task.FromResult(Result.Ok(2))
            .BindOkAsync(value => Task.FromResult(Result.Ok(value * 5)))
            .BindOkAsync(value => Task.FromResult(Result.Ok(value + 1)));

        await Assert.That(result.OrThrow()).IsEqualTo(11);
    }

    [Test]
    public async Task BindOkAsync_WithCancellationAwareBinder()
    {
        using var cts = new CancellationTokenSource();

        var result = await Task.FromResult(Result.Ok(2))
            .BindOkAsync((value, token) => Task.FromResult(Result.Ok(value + (token == cts.Token ? 1 : 0))), cts.Token);

        await Assert.That(result.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task BindOkAsync_FromSyncResult()
    {
        var result = await Result.Ok(2).BindOkAsync(value => Task.FromResult(Result.Ok(value * 4)));

        await Assert.That(result.OrThrow()).IsEqualTo(8);
    }

    [Test]
    public async Task BindErrAsync_Recovers()
    {
        var result = await Task.FromResult(Result.Err<int>(Sample.Error()))
            .BindErrAsync(_ => Task.FromResult(Result.Ok(7)));

        await Assert.That(result.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task BindErrAsync_WithCancellationAwareFallback()
    {
        using var cts = new CancellationTokenSource();

        var result = await Task.FromResult(Result.Err<int>(Sample.Error()))
            .BindErrAsync((error, token) => Task.FromResult(Result.Ok(token == cts.Token ? 1 : 0)), cts.Token);

        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task BindErrAsync_FromSyncResult()
    {
        var result = await Result.Err<int>(Sample.Error()).BindErrAsync(_ => Task.FromResult(Result.Ok(9)));

        await Assert.That(result.OrThrow()).IsEqualTo(9);
    }

    [Test]
    public async Task TeeOkAsync_ObservesSuccess()
    {
        var observed = 0;

        var result = await Task.FromResult(Result.Ok(3)).TeeOkAsync(value =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsEqualTo(3);
        await Assert.That(result.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task TeeErrAsync_ObservesError()
    {
        var observed = string.Empty;

        var result = await Task.FromResult(Result.Err<int>(Sample.Error("E_X"))).TeeErrAsync(error =>
        {
            observed = error.Code;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsEqualTo("E_X");
        await Assert.That(result.IsErr).IsTrue();
    }

    [Test]
    public async Task MapOkAsync_WithCancelledTokenAndPendingTask_ThrowsAndSkipsMapper()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pending = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var called = false;

        var task = pending.Task.MapOkAsync(
            value =>
            {
                called = true;
                return value;
            },
            cts.Token);

        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(called).IsFalse();
        pending.SetResult(Result.Ok(1));
    }

    [Test]
    public async Task MapOkAsync_WithCancelledTokenAndCompletedSuccess_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = Task.FromResult(Result.Ok(1)).MapOkAsync(value => value + 1, cts.Token);

        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task MapOkAsync_WithCancelledTokenAndCompletedError_PassesThrough()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await Task.FromResult(Result.Err<int>(Sample.Error("E_X")))
            .MapOkAsync(value => value + 1, cts.Token);

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task SequenceAsync_AwaitsInOrder()
    {
        var tasks = new[]
        {
            Task.FromResult(Result.Ok(1)),
            Task.FromResult(Result.Ok(2)),
        };

        var result = await Result.SequenceAsync(tasks);

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 1, 2);
    }

    [Test]
    public async Task SequenceAsync_WithEmptySource_ReturnsEmptyOk()
    {
        var result = await Result.SequenceAsync(Array.Empty<Task<Result<int, RopError>>>());

        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceAsync_ShortCircuitsOnFirstError()
    {
        var error = Sample.Error("E_X");
        var laterRequested = false;

        var result = await Result.SequenceAsync(Source());

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(laterRequested).IsFalse();

        IEnumerable<Task<Result<int, RopError>>> Source()
        {
            yield return Task.FromResult(Result.Err<int>(error));
            laterRequested = true;
            yield return Task.FromResult(Result.Ok(2));
        }
    }

    [Test]
    public async Task TraverseAsync_MapsAndSequences()
    {
        var result = await Result.TraverseAsync([1, 2, 3], value => Task.FromResult(Result.Ok(value * 2)));

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 2, 4, 6);
    }

    [Test]
    public async Task TryAsync_ReturnsOkWhenFunctionSucceeds()
    {
        var result = await Result.TryAsync(() => Task.FromResult(42), _ => Sample.Error());

        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task TryAsync_ConvertsExceptionToError()
    {
        var result = await Result.TryAsync<int>(
            () => Task.FromException<int>(new InvalidOperationException("bad")),
            exception => RopError.FromException(exception, "E_ASYNC"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_ASYNC");
        await Assert.That(result.ErrOrThrow().Cause is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsync_ConvertsSynchronousThrowToError()
    {
        var result = await Result.TryAsync<int>(
            () => throw new InvalidOperationException("sync"),
            exception => RopError.FromException(exception, "E_SYNC"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_SYNC");
    }

    [Test]
    public async Task TryAsync_RethrowsOperationCanceledException()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync<int>(() => Task.FromException<int>(new OperationCanceledException()), _ => Sample.Error()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithNullResult_ReturnsNullResultError()
    {
        var result = await Result.TryAsync<string?>(() => Task.FromResult<string?>(null), _ => Sample.Error());

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_NULL_RESULT");
    }

    [Test]
    public async Task TryAsync_WithCancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync(() => Task.FromResult(1), _ => Sample.Error(), cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCodeOverload_UsesProvidedCode()
    {
        var result = await Result.TryAsync<int>(
            () => Task.FromException<int>(new InvalidOperationException("bad")),
            "E_CODE");

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_CODE");
    }
}
