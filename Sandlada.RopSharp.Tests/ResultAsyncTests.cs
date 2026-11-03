using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultAsyncTests
{
    [Test]
    public async Task AsTask_WrapsResult()
    {
        var result = await ResultTask.From(Result.Ok(5));

        await Assert.That(result.Expect("test")).IsEqualTo(5);
    }

    [Test]
    public async Task MapOkAsync_WithSyncMapper()
    {
        var result = await ResultTask.From(Result.Ok(2)).MapOk(value => value * 10);

        await Assert.That(result.Expect("test")).IsEqualTo(20);
    }

    [Test]
    public async Task MapOkAsync_WithAsyncMapper_PrefersAsyncOverload()
    {
        var result = await ResultTask.From(Result.Ok(2)).MapOkAsync((value, _) => Task.FromResult(value * 10));

        await Assert.That(result.Expect("test")).IsEqualTo(20);
    }

    [Test]
    public async Task MapOkAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();

        var result = await ResultTask.From(Result.Ok(2))
            .WithCancellation(cts.Token).MapOkAsync((value, token) => Task.FromResult(token == cts.Token ? value + 1 : -1));

        await Assert.That(result.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task MapOkAsync_FromSyncResult_WithAsyncMapper()
    {
        var result = await Result.Ok(2).MapOkAsync((value, _) => Task.FromResult(value * 3));

        await Assert.That(result.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task MapOkAsync_PropagatesErrorWithoutCallingMapper()
    {
        var called = false;
        var error = Sample.Error("E_X");

        var result = await ResultTask.From(Result.Err<int>(error)).MapOkAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(value);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task MapErrAsync_MapsErrorOnly()
    {
        var mapped = await ResultTask.From(Result.Err<int>(Sample.Error("E_X")))
            .MapErrAsync((error, _) => Task.FromResult(error.WithCode("E_Y")));
        var untouched = await ResultTask.From(Result.Ok(1)).MapErrAsync((error, _) => Task.FromResult(error.WithCode("E_Y")));

        await Assert.That(mapped.ExpectErr().Code).IsEqualTo("E_Y");
        await Assert.That(untouched.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task MapErrAsync_WithAsyncMapper()
    {
        var mapped = await ResultTask.From(Result.Err<int>(Sample.Error("E_X")))
            .MapErrAsync((error, _) => Task.FromResult(error.WithCode("E_ASYNC")));

        await Assert.That(mapped.ExpectErr().Code).IsEqualTo("E_ASYNC");
    }

    [Test]
    public async Task BindOkAsync_ChainsTaskResults()
    {
        var result = await ResultTask.From(Result.Ok(2))
            .BindOkAsync((value, _) => Task.FromResult(Result.Ok(value * 5)))
            .BindOkAsync((value, _) => Task.FromResult(Result.Ok(value + 1)));

        await Assert.That(result.Expect("test")).IsEqualTo(11);
    }

    [Test]
    public async Task BindOkAsync_WithCancellationAwareBinder()
    {
        using var cts = new CancellationTokenSource();

        var result = await ResultTask.From(Result.Ok(2)).WithCancellation(cts.Token)
            .BindOkAsync((value, token) => Task.FromResult(Result.Ok(value + (token == cts.Token ? 1 : 0))));

        await Assert.That(result.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task BindOkAsync_FromSyncResult()
    {
        var result = await Result.Ok(2).BindOkAsync((value, _) => Task.FromResult(Result.Ok(value * 4)));

        await Assert.That(result.Expect("test")).IsEqualTo(8);
    }

    [Test]
    public async Task BindErrAsync_Recovers()
    {
        var result = await ResultTask.From(Result.Err<int>(Sample.Error()))
            .BindErrAsync((_, _) => Task.FromResult(Result.Ok(7)));

        await Assert.That(result.Expect("test")).IsEqualTo(7);
    }

    [Test]
    public async Task BindErrAsync_WithCancellationAwareFallback()
    {
        using var cts = new CancellationTokenSource();

        var result = await ResultTask.From(Result.Err<int>(Sample.Error()))
            .WithCancellation(cts.Token)
            .BindErrAsync((error, token) => Task.FromResult(Result.Ok(token == cts.Token ? 1 : 0)));

        await Assert.That(result.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task BindErrAsync_FromSyncResult()
    {
        var result = await Result.Err<int>(Sample.Error()).BindErrAsync((_, _) => Task.FromResult(Result.Ok(9)));

        await Assert.That(result.Expect("test")).IsEqualTo(9);
    }

    [Test]
    public async Task TeeOkAsync_ObservesSuccess()
    {
        var observed = 0;

        var result = await ResultTask.From(Result.Ok(3)).TeeOkAsync((value, _) =>
        {
            observed = value;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsEqualTo(3);
        await Assert.That(result.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task TeeErrAsync_ObservesError()
    {
        var observed = string.Empty;

        var result = await ResultTask.From(Result.Err<int>(Sample.Error("E_X"))).TeeErrAsync((error, _) =>
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

        var task = ResultTask.From(pending.Task).WithCancellation(cts.Token).MapOkAsync(
            (value, _) =>
            {
                called = true;
                return Task.FromResult(value);
            });

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

        var task = ResultTask.From(Result.Ok(1)).WithCancellation(cts.Token).MapOkAsync((value, _) => Task.FromResult(value + 1));

        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task MapOkAsync_WithCancelledTokenAndCompletedError_PassesThrough()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await ResultTask.From(Result.Err<int>(Sample.Error("E_X")))
            .WithCancellation(cts.Token).MapOkAsync((value, _) => Task.FromResult(value + 1));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task SequenceAsync_AwaitsInOrder()
    {
        var tasks = new[]
        {
            ResultTask.From(Result.Ok(1)),
            ResultTask.From(Result.Ok(2)),
        };

        var result = await ResultTask.Sequence(tasks);

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 1, 2);
    }

    [Test]
    public async Task SequenceAsync_WithEmptySource_ReturnsEmptyOk()
    {
        var result = await ResultTask.Sequence(Array.Empty<ResultTask<int, RopError>>());

        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceAsync_ShortCircuitsOnFirstError()
    {
        var error = Sample.Error("E_X");
        var laterRequested = false;

        var result = await ResultTask.Sequence(Source());

        await Assert.That(result.ExpectErr()).IsSameReferenceAs(error);
        await Assert.That(laterRequested).IsFalse();

        IEnumerable<ResultTask<int, RopError>> Source()
        {
            yield return ResultTask.From(Result.Err<int>(error));
            laterRequested = true;
            yield return ResultTask.From(Result.Ok(2));
        }
    }

    [Test]
    public async Task TraverseAsync_MapsAndSequences()
    {
        var result = await ResultTask.Traverse([1, 2, 3], (value, _) => Task.FromResult(Result.Ok(value * 2)));

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 2, 4, 6);
    }

    [Test]
    public async Task TryAsync_ReturnsOkWhenFunctionSucceeds()
    {
        var result = await Result.TryAsync(() => Task.FromResult(42), _ => Sample.Error());

        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task TryAsync_ConvertsExceptionToError()
    {
        var result = await Result.TryAsync<int>(
            () => Task.FromException<int>(new InvalidOperationException("bad")),
            exception => RopError.FromException(exception, "E_ASYNC"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_ASYNC");
        await Assert.That(result.ExpectErr().Cause is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsync_ConvertsSynchronousThrowToError()
    {
        var result = await Result.TryAsync<int>(
            () => throw new InvalidOperationException("sync"),
            exception => RopError.FromException(exception, "E_SYNC"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_SYNC");
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

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_NULL_RESULT");
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

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_CODE");
    }
}
