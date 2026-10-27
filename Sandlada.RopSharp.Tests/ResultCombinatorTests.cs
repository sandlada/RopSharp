using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultCombinatorTests
{
    [Test]
    public async Task MapOk_MapsSuccess()
    {
        var result = Rop.Ok(2).MapOk(value => value * 10);

        await Assert.That(result.OrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task MapOk_DoesNotInvokeMapperOnError()
    {
        var called = false;

        var result = Rop.Err<int>(Sample.Error("E_X"))
            .MapOk(value =>
            {
                called = true;
                return value * 10;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task MapOk_WithNullMapper_Throws()
    {
        await Assert.That(TestSupport.Catch(() => Rop.Ok(1).MapOk<int, int, RopError>(null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task MapErr_MapsError()
    {
        var result = Rop.Err<int>(Sample.Error("E_X")).MapErr(error => error.WithCode("E_Y"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_Y");
    }

    [Test]
    public async Task MapErr_DoesNotInvokeMapperOnSuccess()
    {
        var called = false;

        var result = Rop.Ok(1).MapErr(error =>
        {
            called = true;
            return error;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task TeeOk_ObservesSuccessAndReturnsSameResult()
    {
        var observed = new List<int>();
        var source = Rop.Ok(3);

        var result = source.TeeOk(observed.Add);

        await Assert.That(result).IsEqualTo(source);
        await Assert.That(observed.Count).IsEqualTo(1);
        await Assert.That(observed[0]).IsEqualTo(3);
    }

    [Test]
    public async Task TeeOk_DoesNotObserveError()
    {
        var observed = false;

        var result = Rop.Err<int>(Sample.Error()).TeeOk(_ => observed = true);

        await Assert.That(observed).IsFalse();
        await Assert.That(result.IsErr).IsTrue();
    }

    [Test]
    public async Task TeeErr_ObservesErrorAndReturnsSameResult()
    {
        var observed = new List<string>();
        var source = Rop.Err<int>(Sample.Error("E_X"));

        var result = source.TeeErr(error => observed.Add(error.Code));

        await Assert.That(result).IsEqualTo(source);
        await Assert.That(observed.Count).IsEqualTo(1);
        await Assert.That(observed[0]).IsEqualTo("E_X");
    }

    [Test]
    public async Task TeeErr_DoesNotObserveSuccess()
    {
        var observed = false;

        var result = Rop.Ok(1).TeeErr(_ => observed = true);

        await Assert.That(observed).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task Ensure_WithConstantError_TurnsSuccessIntoFailure()
    {
        var result = Rop.Ok(-1).Ensure(value => value >= 0, Sample.Error("E_AGE", "negative"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_AGE");
    }

    [Test]
    public async Task Ensure_KeepsSuccessWhenPredicateHolds()
    {
        var source = Rop.Ok(10);

        var result = source.Ensure(value => value >= 0, Sample.Error());

        await Assert.That(result).IsEqualTo(source);
    }

    [Test]
    public async Task Ensure_DoesNotEvaluatePredicateOnError()
    {
        var called = false;
        var error = Sample.Error("E_X");

        var result = Rop.Err<int>(error).Ensure(
            _ =>
            {
                called = true;
                return false;
            },
            Sample.Error());

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Ensure_WithFactory_BuildsErrorLazily()
    {
        var factoryCalls = 0;

        var ok = Rop.Ok(10).Ensure(
            value => value >= 0,
            value =>
            {
                factoryCalls++;
                return RopError.Create("E_AGE", $"{value}");
            });

        var failed = Rop.Ok(-5).Ensure(
            value => value >= 0,
            value =>
            {
                factoryCalls++;
                return RopError.Create("E_AGE", $"{value}");
            });

        await Assert.That(ok.IsOk).IsTrue();
        await Assert.That(factoryCalls).IsEqualTo(1);
        await Assert.That(failed.ErrOrThrow().Message).IsEqualTo("-5");
    }

    [Test]
    public async Task MapOkValue_ReplacesSuccessValue()
    {
        var ok = Rop.Ok(5).MapOkValue(Unit.Value);
        var err = Rop.Err<int>(Sample.Error("E_X")).MapOkValue(Unit.Value);

        await Assert.That(ok.OrThrow()).IsEqualTo(Unit.Value);
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task MapOkValue_WithNull_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Ok(5).MapOkValue<int, string, RopError>(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task RequireNotNull_TurnsNullSuccessIntoError()
    {
        var source = Rop.Ok<string?>("bob").MapOk(_ => (string?)null);

        var result = source.RequireNotNull(() => Sample.Error("E_NULL", "name null"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_NULL");
    }

    [Test]
    public async Task RequireNotNull_KeepsNonNullSuccess()
    {
        Result<string?, RopError> source = Rop.Ok<string?, RopError>("bob");

        var result = source.RequireNotNull(() => Sample.Error());

        await Assert.That(result.OrThrow()).IsEqualTo("bob");
    }

    [Test]
    public async Task RequireNotNull_PassesErrorThrough()
    {
        var error = Sample.Error("E_X");
        Result<string?, RopError> source = Rop.Err<string?>(error);

        var result = source.RequireNotNull(() => Sample.Error());

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task BindOk_ChainsSuccess()
    {
        var result = Rop.Ok(2)
            .BindOk(value => Rop.Ok(value * 3))
            .BindOk(value => Rop.Ok(value + 1));

        await Assert.That(result.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task BindOk_ShortCircuitsOnFirstError()
    {
        var secondCalled = false;
        var error = Sample.Error("E_FIRST");

        var result = Rop.Err<int>(error)
            .BindOk(value =>
            {
                secondCalled = true;
                return Rop.Ok(value);
            });

        await Assert.That(secondCalled).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task BindOk_PropagatesErrorFromBinder()
    {
        var result = Rop.Ok(1).BindOk<int, int, RopError>(_ => Rop.Err<int>(Sample.Error("E_BIND")));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_BIND");
    }

    [Test]
    public async Task BindErr_RecoversFromError()
    {
        var result = Rop.Err<int>(Sample.Error()).BindErr(_ => Rop.Ok(99));

        await Assert.That(result.OrThrow()).IsEqualTo(99);
    }

    [Test]
    public async Task BindErr_DoesNotInvokeFallbackOnSuccess()
    {
        var called = false;
        var source = Rop.Ok(1);

        var result = source.BindErr((RopError value) =>
        {
            called = true;
            return Rop.Ok(value.Code.Length);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result).IsEqualTo(source);
    }

    [Test]
    public async Task BindErr_CanFlipBackToFailure()
    {
        var result = Rop.Err<int>(Sample.Error("E_A"))
            .BindErr(error => Rop.Err<int>(error.WithCode("E_B")));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_B");
    }

    [Test]
    public async Task BindErrValue_RecoversWithValue()
    {
        var result = Rop.Err<int>(Sample.Error()).BindErrValue(7);

        await Assert.That(result.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task BindErrValue_KeepsSuccess()
    {
        var source = Rop.Ok(3);

        var result = source.BindErrValue(7);

        await Assert.That(result).IsEqualTo(source);
    }

    [Test]
    public async Task BindErrValue_WithFactory_IsLazy()
    {
        var calls = 0;

        var ok = Rop.Ok(3).BindErrValue(() =>
        {
            calls++;
            return 7;
        });

        var failed = Rop.Err<int>(Sample.Error()).BindErrValue(() =>
        {
            calls++;
            return 7;
        });

        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(ok.OrThrow()).IsEqualTo(3);
        await Assert.That(failed.OrThrow()).IsEqualTo(7);
    }
}
