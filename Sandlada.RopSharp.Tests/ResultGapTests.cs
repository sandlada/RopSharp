using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultGapTests
{
    [Test]
    public async Task SuccessIf_SelectsTrack()
    {
        var error = Sample.Error();

        var ok = Rop.SuccessIf(true, 42, error);
        var err = Rop.SuccessIf(false, 42, error);

        await Assert.That(ok.OrThrow()).IsEqualTo(42);
        await Assert.That(err.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task SuccessIf_WithFactory_EvaluatesLazily()
    {
        var called = false;

        var ok = Rop.SuccessIf(true, 42, () =>
        {
            called = true;
            return Sample.Error();
        });

        await Assert.That(ok.OrThrow()).IsEqualTo(42);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task FailureIf_SelectsTrack()
    {
        var error = Sample.Error();

        var err = Rop.FailureIf(true, 42, error);
        var ok = Rop.FailureIf(false, 42, error);

        await Assert.That(err.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(ok.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task FailureIf_WithFactory_EvaluatesLazily()
    {
        var called = false;

        var ok = Rop.FailureIf(false, 42, () =>
        {
            called = true;
            return Sample.Error();
        });

        await Assert.That(ok.OrThrow()).IsEqualTo(42);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task Parse_SucceedsAndFails()
    {
        var ok = Rop.Parse<int>("42");
        var err = Rop.Parse<int>("not-a-number");

        await Assert.That(ok.OrThrow()).IsEqualTo(42);
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task TryParse_ReturnsSomeOrNone()
    {
        await Assert.That(Rop.TryParse<int>("42").OrThrow()).IsEqualTo(42);
        await Assert.That(Rop.TryParse<int>("nope").IsNone).IsTrue();
        await Assert.That(Rop.TryParse<int>(null).IsNone).IsTrue();
    }

    [Test]
    public async Task FailIf_FailsOnlyWhenPredicateHolds()
    {
        var error = Sample.Error("E_TOO_LARGE");

        var ok = Rop.Ok(1).FailIf(value => value > 10, error);
        var err = Rop.Ok(20).FailIf(value => value > 10, error);

        await Assert.That(ok.OrThrow()).IsEqualTo(1);
        await Assert.That(err.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task FailIf_WithFactory_EvaluatesLazily()
    {
        var called = false;

        var ok = Rop.Ok(1).FailIf(
            value => value > 10,
            value =>
            {
                called = true;
                return Sample.Error("E_TOO_LARGE", value.ToString());
            });

        await Assert.That(ok.OrThrow()).IsEqualTo(1);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task ThenEnsure_KeepsOriginalValueOnSuccess()
    {
        var result = Rop.Ok(42).ThenEnsure(value => Rop.Ok<Unit>(Unit.Value));

        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task ThenEnsure_PropagatesCheckError()
    {
        var error = Sample.Error("E_CHECK");
        var result = Rop.Ok(42).ThenEnsure(_ => Rop.Err<Unit>(error));

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task ThenEnsure_DoesNotRunOnFailure()
    {
        var called = false;
        var error = Sample.Error();

        var result = Rop.Err<int>(error).ThenEnsure(_ =>
        {
            called = true;
            return Rop.Ok<Unit>(Unit.Value);
        });

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(called).IsFalse();
    }

    [Test]
    public async Task Switch_RunsSelectedAction()
    {
        var okValue = 0;
        var errValue = RopError.Create("E_NONE", "none");

        Rop.Ok(42).Switch(value => okValue = value, _ => errValue = Sample.Error());
        Rop.Err<int>(Sample.Error("E_X")).Switch(_ => okValue = -1, error => errValue = error);

        await Assert.That(okValue).IsEqualTo(42);
        await Assert.That(errValue.Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task SwitchAsync_RunsSelectedActionAndHonorsCancellation()
    {
        var seen = 0;
        await Task.FromResult(Rop.Ok(42)).SwitchAsync(
            value =>
            {
                seen = value;
                return Task.CompletedTask;
            },
            _ => Task.CompletedTask);

        var error = Sample.Error("E_SWITCH");
        var errorSeen = string.Empty;
        await Task.FromResult(Rop.Err<int>(error)).SwitchAsync(
            _ => Task.CompletedTask,
            err =>
            {
                errorSeen = err.Code;
                return Task.CompletedTask;
            });

        await Assert.That(seen).IsEqualTo(42);
        await Assert.That(errorSeen).IsEqualTo("E_SWITCH");

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var exception = await TestSupport.CatchAsync(() => Task.FromResult(Rop.Ok(42)).SwitchAsync(
            _ => Task.CompletedTask,
            _ => Task.CompletedTask,
            cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task MatchFirst_UsesFirstError()
    {
        var first = Sample.Error("E_FIRST");
        var result = Rop.Err<int, IReadOnlyList<RopError>>([first, Sample.Error("E_SECOND")]);

        var code = result.MatchFirst(_ => "ok", error => error.Code);
        var ok = Rop.Ok<int, IReadOnlyList<RopError>>(7).MatchFirst(value => value.ToString(), _ => "err");

        await Assert.That(code).IsEqualTo("E_FIRST");
        await Assert.That(ok).IsEqualTo("7");
    }

    [Test]
    public async Task SwitchFirst_UsesFirstError()
    {
        var first = Sample.Error("E_FIRST");
        var seen = string.Empty;
        var result = Rop.Err<int, IReadOnlyList<RopError>>([first, Sample.Error("E_SECOND")]);

        result.SwitchFirst(_ => seen = "ok", error => seen = error.Code);

        await Assert.That(seen).IsEqualTo("E_FIRST");

        var successSeen = 0;
        Rop.Ok<int, IReadOnlyList<RopError>>(7).SwitchFirst(value => successSeen = value, _ => { });

        await Assert.That(successSeen).IsEqualTo(7);
    }

    [Test]
    public async Task SwitchFirst_WithEmptyErrorList_Throws()
    {
        var invalid = Rop.Err<int, IReadOnlyList<RopError>>([]);

        var exception = TestSupport.Catch(() => invalid.SwitchFirst(_ => { }, _ => { }));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task CombineAsync_MergesSuccessAndReturnsFirstError()
    {
        var ok = await Rop.CombineAsync(
            Task.FromResult(Rop.Ok(2)),
            Task.FromResult(Rop.Ok(3)),
            (int a, int b) => a * b);

        var first = Sample.Error("E_FIRST");
        var err = await Rop.CombineAsync(
            Task.FromResult(Rop.Err<int>(first)),
            Task.FromResult(Rop.Err<int>(Sample.Error("E_SECOND"))),
            (int a, int b) => a + b);

        await Assert.That(ok.OrThrow()).IsEqualTo(6);
        await Assert.That(err.ErrOrThrow()).IsSameReferenceAs(first);
    }

    [Test]
    public async Task CombineAsync3_MergesSuccess()
    {
        var result = await Rop.CombineAsync(
            Task.FromResult(Rop.Ok(1)),
            Task.FromResult(Rop.Ok(2)),
            Task.FromResult(Rop.Ok(3)),
            (int a, int b, int c) => a + b + c);

        await Assert.That(result.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task ValidateAsync_CollectsEveryError()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = await Rop.ValidateAsync(
            Task.FromResult(Rop.Err<int>(first)),
            Task.FromResult(Rop.Err<int>(second)),
            (int a, int b) => a + b);

        await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), first, second);
    }

    [Test]
    public async Task ValidateAsync3_CollectsEveryError()
    {
        var first = Sample.Error("E_FIRST");
        var third = Sample.Error("E_THIRD");

        var result = await Rop.ValidateAsync(
            Task.FromResult(Rop.Err<int>(first)),
            Task.FromResult(Rop.Ok(2)),
            Task.FromResult(Rop.Err<int>(third)),
            (int a, int b, int c) => a + b + c);

        await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), first, third);
    }

    [Test]
    public async Task ZipAsync_PairsValues()
    {
        var pair = await Rop.ZipAsync(Task.FromResult(Rop.Ok(1)), Task.FromResult(Rop.Ok("a")));
        var triple = await Rop.ZipAsync(
            Task.FromResult(Rop.Ok(1)),
            Task.FromResult(Rop.Ok("a")),
            Task.FromResult(Rop.Ok(true)));

        await Assert.That(pair.OrThrow()).IsEqualTo((1, "a"));
        await Assert.That(triple.OrThrow()).IsEqualTo((1, "a", true));
    }

    [Test]
    public async Task PartitionAsync_SplitsResults()
    {
        var error = Sample.Error();
        var (oks, errs) = await Rop.PartitionAsync(new[]
        {
            Task.FromResult(Rop.Ok(1)),
            Task.FromResult(Rop.Err<int>(error)),
            Task.FromResult(Rop.Ok(2)),
        });

        await TestSupport.AssertSequenceAsync(oks, 1, 2);
        await Assert.That(errs.Count).IsEqualTo(1);
        await Assert.That(errs[0]).IsSameReferenceAs(error);
    }

    [Test]
    public async Task AsyncAggregation_HonorsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await TestSupport.CatchAsync(() => Rop.CombineAsync(
            Task.FromResult(Rop.Ok(1)),
            Task.FromResult(Rop.Ok(2)),
            (int a, int b) => a + b,
            cts.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task AsyncLinq_Select_ProjectsSuccessAndPreservesError()
    {
        var ok = await Task.FromResult(Rop.Ok(2)).Select(value => value * 2);
        var error = Sample.Error();
        var err = await Task.FromResult(Rop.Err<int>(error)).Select(value => value * 2);

        await Assert.That(ok.OrThrow()).IsEqualTo(4);
        await Assert.That(err.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task AsyncLinq_SelectAndSelectMany_WorkInQuerySyntax()
    {
        var result =
            from a in Task.FromResult(Rop.Ok(2))
            from b in Task.FromResult(Rop.Ok(3))
            select a * b;

        var syncBinder =
            from a in Task.FromResult(Rop.Ok(2))
            from b in Rop.Ok(3)
            select a + b;

        await Assert.That((await result).OrThrow()).IsEqualTo(6);
        await Assert.That((await syncBinder).OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task AsyncLinq_PropagatesFirstError()
    {
        var error = Sample.Error();
        var asyncBinder =
            from a in Task.FromResult(Rop.Err<int>(error))
            from b in Task.FromResult(Rop.Ok(3))
            select a + b;
        var syncBinder =
            from a in Task.FromResult(Rop.Err<int>(error))
            from b in Rop.Ok(3)
            select a + b;

        await Assert.That((await asyncBinder).ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That((await syncBinder).ErrOrThrow()).IsSameReferenceAs(error);
    }
}
