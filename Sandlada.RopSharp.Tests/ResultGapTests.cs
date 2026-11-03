using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultGapTests
{
    [Test]
    public async Task SuccessIf_SelectsTrack()
    {
        var error = Sample.Error();

        var ok = Result.SuccessIf(true, 42, error);
        var err = Result.SuccessIf(false, 42, error);

        await Assert.That(ok.Expect("test")).IsEqualTo(42);
        await Assert.That(err.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task SuccessIf_WithFactory_EvaluatesLazily()
    {
        var called = false;

        var ok = Result.SuccessIf(true, 42, () =>
        {
            called = true;
            return Sample.Error();
        });

        await Assert.That(ok.Expect("test")).IsEqualTo(42);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task FailureIf_SelectsTrack()
    {
        var error = Sample.Error();

        var err = Result.FailureIf(true, 42, error);
        var ok = Result.FailureIf(false, 42, error);

        await Assert.That(err.ExpectErr()).IsSameReferenceAs(error);
        await Assert.That(ok.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task FailureIf_WithFactory_EvaluatesLazily()
    {
        var called = false;

        var ok = Result.FailureIf(false, 42, () =>
        {
            called = true;
            return Sample.Error();
        });

        await Assert.That(ok.Expect("test")).IsEqualTo(42);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task Parse_SucceedsAndFails()
    {
        var ok = Result.Parse<int>("42");
        var err = Result.Parse<int>("not-a-number");

        await Assert.That(ok.Expect("test")).IsEqualTo(42);
        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task TryParse_ReturnsSomeOrNone()
    {
        await Assert.That(Result.TryParse<int>("42").Expect("test")).IsEqualTo(42);
        await Assert.That(Result.TryParse<int>("nope").IsNone).IsTrue();
        await Assert.That(Result.TryParse<int>(null).IsNone).IsTrue();
    }

    [Test]
    public async Task FailIf_FailsOnlyWhenPredicateHolds()
    {
        var error = Sample.Error("E_TOO_LARGE");

        var ok = Result.Ok(1).FailIf(value => value > 10, error);
        var err = Result.Ok(20).FailIf(value => value > 10, error);

        await Assert.That(ok.Expect("test")).IsEqualTo(1);
        await Assert.That(err.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task FailIf_WithFactory_EvaluatesLazily()
    {
        var called = false;

        var ok = Result.Ok(1).FailIf(
            value => value > 10,
            value =>
            {
                called = true;
                return Sample.Error("E_TOO_LARGE", value.ToString());
            });

        await Assert.That(ok.Expect("test")).IsEqualTo(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ThenEnsure_KeepsOriginalValueOnSuccess()
    {
        var result = Result.Ok(42).ThenEnsure(value => Result.Ok<Unit>(Unit.Value));

        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task ThenEnsure_PropagatesCheckError()
    {
        var error = Sample.Error("E_CHECK");
        var result = Result.Ok(42).ThenEnsure(_ => Result.Err<Unit>(error));

        await Assert.That(result.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task ThenEnsure_DoesNotRunOnFailure()
    {
        var called = false;
        var error = Sample.Error();

        var result = Result.Err<int>(error).ThenEnsure(_ =>
        {
            called = true;
            return Result.Ok<Unit>(Unit.Value);
        });

        await Assert.That(result.ExpectErr()).IsSameReferenceAs(error);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task Switch_RunsSelectedAction()
    {
        var okValue = 0;
        var errValue = RopError.Create("E_NONE", "none");

        Result.Ok(42).Switch(value => okValue = value, _ => errValue = Sample.Error());
        Result.Err<int>(Sample.Error("E_X")).Switch(_ => okValue = -1, error => errValue = error);

        await Assert.That(okValue).IsEqualTo(42);
        await Assert.That(errValue.Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task SwitchAsync_RunsSelectedActionAndHonorsCancellation()
    {
        var seen = 0;
        await ResultTask.From(Result.Ok(42)).SwitchAsync((value, _) =>
        {
            seen = value;
            return Task.CompletedTask;
        },
            (_, _) => Task.CompletedTask);

        var error = Sample.Error("E_SWITCH");
        var errorSeen = string.Empty;
        await ResultTask.From(Result.Err<int>(error)).SwitchAsync((_, _) => Task.CompletedTask,
            (err, _) =>
            {
                errorSeen = err.Code;
                return Task.CompletedTask;
            });

        await Assert.That(seen).IsEqualTo(42);
        await Assert.That(errorSeen).IsEqualTo("E_SWITCH");

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var exception = await TestSupport.CatchAsync(() => ResultTask.From(Result.Ok(42)).WithCancellation(cts.Token).SwitchAsync((_, _) => Task.CompletedTask,
            (_, _) => Task.CompletedTask));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task MatchFirst_UsesFirstError()
    {
        var first = Sample.Error("E_FIRST");
        var validation = Validation.Failure<int, RopError>([first, Sample.Error("E_SECOND")]);

        var code = validation.MatchFirst(_ => "ok", error => error.Code);
        var ok = Validation.Success<int, RopError>(7).MatchFirst(value => value.ToString(), _ => "err");

        await Assert.That(code).IsEqualTo("E_FIRST");
        await Assert.That(ok).IsEqualTo("7");
    }

    [Test]
    public async Task SwitchFirst_UsesFirstError()
    {
        var first = Sample.Error("E_FIRST");
        var seen = string.Empty;
        var validation = Validation.Failure<int, RopError>([first, Sample.Error("E_SECOND")]);

        validation.SwitchFirst(_ => seen = "ok", error => seen = error.Code);

        await Assert.That(seen).IsEqualTo("E_FIRST");

        var successSeen = 0;
        Validation.Success<int, RopError>(7).SwitchFirst(value => successSeen = value, _ => { });

        await Assert.That(successSeen).IsEqualTo(7);
    }

    [Test]
    public async Task SwitchFirst_WithEmptyErrorList_Throws()
    {
        var exception = TestSupport.Catch(() => Validation.Failure<int, RopError>([]));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task CombineAsync_MergesSuccessAndReturnsFirstError()
    {
        var ok = await ResultTask.Zip(
            ResultTask.From(Result.Ok(2)),
            ResultTask.From(Result.Ok(3)),
            (int a, int b) => a * b);

        var first = Sample.Error("E_FIRST");
        var err = await ResultTask.Zip(
            ResultTask.From(Result.Err<int>(first)),
            ResultTask.From(Result.Err<int>(Sample.Error("E_SECOND"))),
            (int a, int b) => a + b);

        await Assert.That(ok.Expect("test")).IsEqualTo(6);
        await Assert.That(err.ExpectErr()).IsSameReferenceAs(first);
    }

    [Test]
    public async Task CombineAsync3_MergesSuccess()
    {
        var result = await ResultTask.Zip(
            ResultTask.From(Result.Ok(1)),
            ResultTask.From(Result.Ok(2)),
            ResultTask.From(Result.Ok(3)),
            (int a, int b, int c) => a + b + c);

        await Assert.That(result.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task ValidateAsync_CollectsEveryError()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = await ValidationTask.Zip(
            ResultTask.From(Result.Err<int>(first)),
            ResultTask.From(Result.Err<int>(second)),
            (int a, int b) => a + b);

        await TestSupport.AssertSequenceAsync(result.ExpectErrors(), first, second);
    }

    [Test]
    public async Task ValidateAsync3_CollectsEveryError()
    {
        var first = Sample.Error("E_FIRST");
        var third = Sample.Error("E_THIRD");

        var result = await ValidationTask.Zip(
            ResultTask.From(Result.Err<int>(first)),
            ResultTask.From(Result.Ok(2)),
            ResultTask.From(Result.Err<int>(third)),
            (int a, int b, int c) => a + b + c);

        await TestSupport.AssertSequenceAsync(result.ExpectErrors(), first, third);
    }

    [Test]
    public async Task ZipAsync_PairsValues()
    {
        var pair = await ResultTask.Zip(ResultTask.From(Result.Ok(1)), ResultTask.From(Result.Ok("a")));
        var triple = await ResultTask.Zip(
            ResultTask.From(Result.Ok(1)),
            ResultTask.From(Result.Ok("a")),
            ResultTask.From(Result.Ok(true)));

        await Assert.That(pair.Expect("test")).IsEqualTo((1, "a"));
        await Assert.That(triple.Expect("test")).IsEqualTo((1, "a", true));
    }

    [Test]
    public async Task PartitionAsync_SplitsResults()
    {
        var error = Sample.Error();
        var (oks, errs) = (await ResultTask.Partition(new[]
        {
            ResultTask.From(Result.Ok(1)),
            ResultTask.From(Result.Err<int>(error)),
            ResultTask.From(Result.Ok(2)),
        })).Expect("test");

        await TestSupport.AssertSequenceAsync(oks, 1, 2);
        await Assert.That(errs.Count).IsEqualTo(1);
        await Assert.That(errs[0]).IsSameReferenceAs(error);
    }

    [Test]
    public async Task AsyncAggregation_HonorsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => ResultTask.Zip(
            ResultTask.From(Result.Ok(1)),
            ResultTask.From(Result.Ok(2)),
            (int a, int b) => a + b,
            cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task AsyncLinq_Select_ProjectsSuccessAndPreservesError()
    {
        var ok = await ResultTask.From(Result.Ok(2)).Select(value => value * 2);
        var error = Sample.Error();
        var err = await ResultTask.From(Result.Err<int>(error)).Select(value => value * 2);

        await Assert.That(ok.Expect("test")).IsEqualTo(4);
        await Assert.That(err.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task AsyncLinq_SelectAndSelectMany_WorkInQuerySyntax()
    {
        var result =
            from a in ResultTask.From(Result.Ok(2))
            from b in Result.Ok(3)
            select a * b;

        var syncBinder =
            from a in ResultTask.From(Result.Ok(2))
            from b in Result.Ok(3)
            select a + b;

        await Assert.That((await result).Expect("test")).IsEqualTo(6);
        await Assert.That((await syncBinder).Expect("test")).IsEqualTo(5);
    }

    [Test]
    public async Task AsyncLinq_PropagatesFirstError()
    {
        var error = Sample.Error();
        var asyncBinder =
            from a in ResultTask.From(Result.Err<int>(error))
            from b in Result.Ok(3)
            select a + b;
        var syncBinder =
            from a in ResultTask.From(Result.Err<int>(error))
            from b in Result.Ok(3)
            select a + b;

        await Assert.That((await asyncBinder).ExpectErr()).IsSameReferenceAs(error);
        await Assert.That((await syncBinder).ExpectErr()).IsSameReferenceAs(error);
    }
}
