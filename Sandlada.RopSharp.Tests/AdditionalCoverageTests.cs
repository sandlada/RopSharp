using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class AdditionalCoverageTests
{
    [Test]
    public async Task TeeOkAsync_DoesNotRunOnError()
    {
        var called = false;

        var result = await ResultTask.From(Result.Err<int>(Sample.Error("E_X"))).TeeOkAsync((_, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task TeeErrAsync_DoesNotRunOnSuccess()
    {
        var called = false;

        var result = await ResultTask.From(Result.Ok(1)).TeeErrAsync((_, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task TeeSomeAsync_DoesNotRunOnNone()
    {
        var called = false;

        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).TeeSomeAsync((_, _) =>
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

        var option = await OptionTask.From(Task.FromResult(Option.Some(1))).TeeNoneAsync(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task TraverseAsync_ShortCircuitsBeforeMappingLaterElements()
    {
        var mapped = new List<int>();

        var result = await ResultTask.Traverse<int, int, RopError>(
        [
            1,
            2,
            3,
        ], (value, _) =>
        {
            mapped.Add(value);
            return Task.FromResult(value == 1 ? Result.Err<int>(Sample.Error("E_FIRST")) : Result.Ok(value));
        });

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_FIRST");
        await TestSupport.AssertSequenceAsync(mapped, 1);
    }

    [Test]
    public async Task Zip3_PropagatesTheFirstError()
    {
        var error = Sample.Error("E_SECOND");

        var result = Result.Zip(Result.Ok(1), Result.Err<int>(error), Result.Ok(3));

        await Assert.That(result.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Validate3_DoesNotInvokeMergerWhenAnyInputFails()
    {
        var called = false;

        var result = Validation.Zip(
            Result.Ok(1),
            Result.Err<int>(Sample.Error()),
            Result.Ok(3),
            (int a, int b, int c) =>
            {
                called = true;
                return a + b + c;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ExpectErrors().Count).IsEqualTo(1);
    }

    [Test]
    public async Task Try_WithBlankCode_Throws()
    {
        var exception = TestSupport.Catch(() => Result.Try(() => 1, code: "  "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithBlankCode_Throws()
    {
        var exception = TestSupport.Catch(() => Result.TryAsync(() => Task.FromResult(1), code: " "));

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
        object equal = Result.Ok<int, string>(1);
        object different = Result.Ok<int, string>(2);
        object error = Result.Err<int, string>("E");

        await Assert.That(Result.Ok<int, string>(1).Equals(equal)).IsTrue();
        await Assert.That(Result.Ok<int, string>(1).Equals(different)).IsFalse();
        await Assert.That(Result.Ok<int, string>(1).Equals(error)).IsFalse();
        await Assert.That(Result.Ok<int, string>(1).Equals(null)).IsFalse();
    }

    [Test]
    public async Task Option_EqualsObject_ComparesByValue()
    {
        object some = Option.Some(1);
        object none = Option.None<int>();

        await Assert.That(Option.Some(1).Equals(some)).IsTrue();
        await Assert.That(Option.Some(1).Equals(none)).IsFalse();
        await Assert.That(Option.None<int>().Equals(none)).IsTrue();
        await Assert.That(Option.Some(1).Equals(null)).IsFalse();
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

        var result = await ResultTask.From(Result.Err<int>(Sample.Error("E_X")))
            .WithCancellation(cts.Token).MapOkAsync((value, _) => Task.FromResult(value + 1));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task SequenceAsync_PreservesEnumerationOrder()
    {
        var first = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<Result<int, RopError>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pending = ResultTask.Sequence([ResultTask.From(first.Task), ResultTask.From(second.Task)]);

        // The second task completes first, but sequencing must still report results in enumeration order.
        second.SetResult(Result.Ok(2));
        first.SetResult(Result.Ok(1));

        await TestSupport.AssertSequenceAsync((await pending).Expect("test"), 1, 2);
    }
}
