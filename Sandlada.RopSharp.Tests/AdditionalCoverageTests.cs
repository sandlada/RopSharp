using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class AdditionalCoverageTests
{
    [Test]
    public async Task TeeOkAsync_DoesNotRunOnError()
    {
        var called = false;

        var result = await Task.FromResult(Rop.Err<int>(Sample.Error("E_X"))).TeeOkAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task TeeErrAsync_DoesNotRunOnSuccess()
    {
        var called = false;

        var result = await Task.FromResult(Rop.Ok(1)).TeeErrAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task TeeSomeAsync_DoesNotRunOnNone()
    {
        var called = false;

        var option = await Task.FromResult(Opt.None<int>()).TeeSomeAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_DoesNotRunOnSome()
    {
        var called = false;

        var option = await Task.FromResult(Opt.Some(1)).TeeNoneAsync(() =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task TraverseAsync_ShortCircuitsBeforeMappingLaterElements()
    {
        var mapped = new List<int>();

        var result = await Rop.TraverseAsync<int, int, RopError>(
        [
            1,
            2,
            3,
        ], (int value) =>
        {
            mapped.Add(value);
            return Task.FromResult(value == 1 ? Rop.Err<int>(Sample.Error("E_FIRST")) : Rop.Ok(value));
        });

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_FIRST");
        await TestSupport.AssertSequenceAsync(mapped, 1);
    }

    [Test]
    public async Task Zip3_PropagatesTheFirstError()
    {
        var error = Sample.Error("E_SECOND");

        var result = Rop.Zip(Rop.Ok(1), Rop.Err<int>(error), Rop.Ok(3));

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Validate3_DoesNotInvokeMergerWhenAnyInputFails()
    {
        var called = false;

        var result = Rop.Validate(
            Rop.Ok(1),
            Rop.Err<int>(Sample.Error()),
            Rop.Ok(3),
            (int a, int b, int c) =>
            {
                called = true;
                return a + b + c;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow().Count).IsEqualTo(1);
    }

    [Test]
    public async Task Try_WithBlankCode_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Try(() => 1, code: "  "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithBlankCode_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.TryAsync(() => Task.FromResult(1), code: " "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task FromException_WithBlankCode_Throws()
    {
        var exception = TestSupport.Catch(() => RopError.FromException(new InvalidOperationException("x"), " "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task Result_EqualsObject_ComparesByValue()
    {
        object equal = Rop.Ok<int, string>(1);
        object different = Rop.Ok<int, string>(2);
        object error = Rop.Err<int, string>("E");

        await Assert.That(Rop.Ok<int, string>(1).Equals(equal)).IsTrue();
        await Assert.That(Rop.Ok<int, string>(1).Equals(different)).IsFalse();
        await Assert.That(Rop.Ok<int, string>(1).Equals(error)).IsFalse();
        await Assert.That(Rop.Ok<int, string>(1).Equals(null)).IsFalse();
    }

    [Test]
    public async Task Option_EqualsObject_ComparesByValue()
    {
        object some = Opt.Some(1);
        object none = Opt.None<int>();

        await Assert.That(Opt.Some(1).Equals(some)).IsTrue();
        await Assert.That(Opt.Some(1).Equals(none)).IsFalse();
        await Assert.That(Opt.None<int>().Equals(none)).IsTrue();
        await Assert.That(Opt.Some(1).Equals(null)).IsFalse();
    }

    [Test]
    public async Task RopError_EqualsObject_ComparesByValue()
    {
        var error = RopError.Create("E_A", "message");

        await Assert.That(error.Equals((object)RopError.Create("E_A", "message"))).IsTrue();
        await Assert.That(error.Equals((object)RopError.Create("E_B", "message"))).IsFalse();
        await Assert.That(error.Equals(null)).IsFalse();
    }

    [Test]
    public async Task MapOkAsync_CancellationAware_OnErrorWithCancelledToken_PassesThrough()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await Task.FromResult(Rop.Err<int>(Sample.Error("E_X")))
            .MapOkAsync((value, _) => Task.FromResult(value + 1), cts.Token);

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task SequenceAsync_PreservesEnumerationOrder()
    {
        var first = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = Rop.SequenceAsync([first.Task, second.Task]);

        // The second task completes first, but sequencing must still report results in enumeration order.
        second.SetResult(Rop.Ok(2));
        first.SetResult(Rop.Ok(1));

        await TestSupport.AssertSequenceAsync((await pending).OrThrow(), 1, 2);
    }
}
