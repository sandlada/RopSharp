using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class AsyncBoundaryTests
{
    [Test]
    public async Task TryAction_WhenActionSucceeds_ReturnsUnit()
    {
        var called = false;

        var result = Rop.Try(
            () =>
            {
                called = true;
            },
            _ => Sample.Error());

        await Assert.That(called).IsTrue();
        await Assert.That(result.OrThrow()).IsEqualTo(Unit.Value);
    }

    [Test]
    public async Task TryAction_WhenActionThrows_ReturnsErrorWithCause()
    {
        var result = Rop.Try(
            () => throw new FormatException("bad"),
            exception => RopError.FromException(exception, "E_PARSE"));

        var error = result.ErrOrThrow();

        await Assert.That(error.Code).IsEqualTo("E_PARSE");
        await Assert.That(error.Cause is FormatException).IsTrue();
    }

    [Test]
    public async Task TryAction_WithCodeOverload_UsesProvidedCode()
    {
        var result = Rop.Try(() => throw new InvalidOperationException("bad"), "E_CODE");

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_CODE");
    }

    [Test]
    public async Task TryAction_RethrowsOperationCanceledException()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Try(() => throw new OperationCanceledException(), _ => Sample.Error()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAction_WithNullArguments_Throws()
    {
        await Assert.That(TestSupport.Catch(() => Rop.Try((Action)null!, _ => Sample.Error())) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Rop.Try(() => { }, (Func<Exception, RopError>)null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TryAction_WithBlankCode_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Try(() => { }, "  "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task TryAction_WithFactoryReturningNull_Throws()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Try(() => throw new InvalidOperationException("bad"), _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WhenActionSucceeds_ReturnsUnit()
    {
        var called = false;

        var result = await Rop.TryAsync(
            async () =>
            {
                await Task.Yield();
                called = true;
            },
            _ => Sample.Error());

        await Assert.That(called).IsTrue();
        await Assert.That(result.OrThrow()).IsEqualTo(Unit.Value);
    }

    [Test]
    public async Task TryAsyncAction_WhenActionThrows_ReturnsErrorWithCause()
    {
        var result = await Rop.TryAsync(
            async () =>
            {
                await Task.Yield();
                throw new FormatException("bad");
            },
            exception => RopError.FromException(exception, "E_PARSE"));

        var error = result.ErrOrThrow();

        await Assert.That(error.Code).IsEqualTo("E_PARSE");
        await Assert.That(error.Cause is FormatException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WhenActionThrowsSynchronously_ReturnsError()
    {
        var result = await Rop.TryAsync(
            () => throw new InvalidOperationException("bad"),
            exception => RopError.FromException(exception, "E_SYNC"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_SYNC");
    }

    [Test]
    public async Task TryAsyncAction_WithCodeOverload_UsesProvidedCode()
    {
        var result = await Rop.TryAsync(
            async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("bad");
            },
            "E_CODE");

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_CODE");
    }

    [Test]
    public async Task TryAsyncAction_RethrowsOperationCanceledException()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync(
                () => throw new OperationCanceledException(),
                _ => Sample.Error()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCancelledToken_DoesNotInvokeAction()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var called = false;

        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync(
                () =>
                {
                    called = true;
                    return Task.CompletedTask;
                },
                _ => Sample.Error(),
                cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task TryAsyncAction_WithCancelledTokenWhilePending_Throws()
    {
        using var cts = new CancellationTokenSource();
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var operation = Rop.TryAsync(() => pending.Task, _ => Sample.Error(), cts.Token);
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => operation);

        await Assert.That(exception is OperationCanceledException).IsTrue();
        pending.SetResult();
    }

    [Test]
    public async Task TryAsyncAction_WithNullTask_ThrowsInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync(() => null!, _ => Sample.Error()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithNullArguments_Throws()
    {
        await Assert.That(
            await TestSupport.CatchAsync(() => Rop.TryAsync((Func<Task>)null!, _ => Sample.Error()))
                is ArgumentNullException).IsTrue();
        await Assert.That(
            await TestSupport.CatchAsync(() => Rop.TryAsync(() => Task.CompletedTask, (Func<Exception, RopError>)null!))
                is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithBlankCode_Throws()
    {
        var exception = await TestSupport.CatchAsync(() => Rop.TryAsync(() => Task.CompletedTask, " "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithFactoryReturningNull_Throws()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync(
                async () =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("bad");
                },
                _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task SwitchAsync_LiftsFunctionWithoutCatchingExceptions()
    {
        var lift = Rop.SwitchAsync<string, int, RopError>(async text =>
        {
            await Task.Yield();
            return int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
        });

        var ok = await lift("12");
        var exception = await TestSupport.CatchAsync(() => lift("nope"));

        await Assert.That(ok.OrThrow()).IsEqualTo(12);
        await Assert.That(exception is FormatException).IsTrue();
    }

    [Test]
    public async Task SwitchAsync_WithNullTask_ThrowsInvalidOperation()
    {
        var lift = Rop.SwitchAsync<string, int, RopError>(_ => null!);

        var exception = await TestSupport.CatchAsync(() => lift("12"));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task SwitchAsync_WithNullPayload_ThrowsArgumentNull()
    {
        var lift = Rop.SwitchAsync<string, string, RopError>(_ => Task.FromResult<string>(null!));

        var exception = await TestSupport.CatchAsync(() => lift("x"));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task SwitchAsync_WithNullFunction_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.SwitchAsync<string, int, RopError>(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TrySwitchAsync_LiftsFunctionAndCapturesExceptions()
    {
        var safeParse = Rop.TrySwitchAsync<string, int>(
            async text =>
            {
                await Task.Yield();
                return int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            },
            exception => RopError.FromException(exception, "E_PARSE"));

        var ok = await safeParse("12");
        var err = await safeParse("nope");

        await Assert.That(ok.OrThrow()).IsEqualTo(12);
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task TrySwitchAsync_WithNullPayload_ReturnsNullResultError()
    {
        var lift = Rop.TrySwitchAsync<string, string>(_ => Task.FromResult<string>(null!), _ => Sample.Error());

        var result = await lift("x");

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_NULL_RESULT");
    }

    [Test]
    public async Task TrySwitchAsync_WithNullArguments_Throws()
    {
        await Assert.That(
            TestSupport.Catch(() => Rop.TrySwitchAsync<string, int>(null!, _ => Sample.Error()))
                is ArgumentNullException).IsTrue();
        await Assert.That(
            TestSupport.Catch(() => Rop.TrySwitchAsync<string, int>(_ => Task.FromResult(1), null!))
                is ArgumentNullException).IsTrue();
    }
}
