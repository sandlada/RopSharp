using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultCoreTests
{
    [Test]
    public async Task Ok_SetsSuccessTrackAndValue()
    {
        var result = Rop.Ok<int, RopError>(42);

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.IsErr).IsFalse();
        await Assert.That(result.TryGetOk(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo(42);
        await Assert.That(result.TryGetErr(out _)).IsFalse();
    }

    [Test]
    public async Task Err_SetsFailureTrackAndError()
    {
        var error = Sample.Error("E_X", "bad");

        var result = Rop.Err<int, RopError>(error);

        await Assert.That(result.IsErr).IsTrue();
        await Assert.That(result.IsOk).IsFalse();
        await Assert.That(result.TryGetErr(out var actual)).IsTrue();
        await Assert.That(actual).IsSameReferenceAs(error);
        await Assert.That(result.TryGetOk(out _)).IsFalse();
    }

    [Test]
    public async Task SingleTypeParameterFactories_UseRopError()
    {
        Result<int, RopError> ok = Rop.Ok(1);
        Result<int, RopError> err = Rop.Err<int>(Sample.Error());

        await Assert.That(ok.IsOk).IsTrue();
        await Assert.That(err.IsErr).IsTrue();
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_TEST");
    }

    [Test]
    public async Task CustomErrorType_IsSupported()
    {
        Result<int, string> ok = Rop.Ok<int, string>(42);
        Result<int, string> err = Rop.Err<int, string>("bad");

        await Assert.That(ok.OrThrow()).IsEqualTo(42);
        await Assert.That(err.ErrOrThrow()).IsEqualTo("bad");
    }

    [Test]
    public async Task Ok_WithNullReference_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Ok<string, RopError>(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task Ok_WithNullNullableValue_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Ok<int?, RopError>(null));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task Err_WithNull_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Err<int, string>(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task OkOrDefault_ReturnsValueOrDefault()
    {
        await Assert.That(Rop.Ok<int, RopError>(7).OkOrDefault()).IsEqualTo(7);
        await Assert.That(Rop.Err<int, RopError>(Sample.Error()).OkOrDefault()).IsEqualTo(0);
        await Assert.That(Rop.Err<string, RopError>(Sample.Error()).OkOrDefault()).IsNull();
    }

    [Test]
    public async Task Equality_ComparesTrackAndPayload()
    {
        var ok = Rop.Ok<int, string>(1);
        var otherOk = Rop.Ok<int, string>(1);
        var differentOk = Rop.Ok<int, string>(2);
        var err = Rop.Err<int, string>("a");
        var otherErr = Rop.Err<int, string>("a");

        await Assert.That(ok.Equals(otherOk)).IsTrue();
        await Assert.That(ok == otherOk).IsTrue();
        await Assert.That(ok.Equals(differentOk)).IsFalse();
        await Assert.That(ok.Equals(err)).IsFalse();
        await Assert.That(err.Equals(otherErr)).IsTrue();
        await Assert.That(err != otherErr).IsFalse();
        await Assert.That(ok.GetHashCode()).IsEqualTo(otherOk.GetHashCode());
    }

    [Test]
    public async Task Equality_WithRopError_UsesErrorValueEquality()
    {
        var left = Rop.Err<int, RopError>(RopError.Create("E_A", "message"));
        var right = Rop.Err<int, RopError>(RopError.Create("E_A", "message"));

        await Assert.That(left.Equals(right)).IsTrue();
    }

    [Test]
    public async Task ToString_ShowsTrackAndPayload()
    {
        await Assert.That(Rop.Ok<int, string>(1).ToString()).IsEqualTo("Ok(1)");
        await Assert.That(Rop.Err<int, string>("bad").ToString()).IsEqualTo("Err(bad)");
    }

    [Test]
    public async Task Match_ProjectsTheActiveTrackOnly()
    {
        var okCalls = 0;
        var errCalls = 0;

        var okText = Rop.Ok<int, RopError>(2).Match(
            ok: value =>
            {
                okCalls++;
                return $"ok:{value}";
            },
            err: error =>
            {
                errCalls++;
                return $"err:{error.Code}";
            });

        await Assert.That(okText).IsEqualTo("ok:2");
        await Assert.That(okCalls).IsEqualTo(1);
        await Assert.That(errCalls).IsEqualTo(0);

        var errText = Rop.Err<int, RopError>(Sample.Error("E_X")).Match(
            ok: value => $"ok:{value}",
            err: error => $"err:{error.Code}");

        await Assert.That(errText).IsEqualTo("err:E_X");
    }

    [Test]
    public async Task Match_WithNullDelegates_Throws()
    {
        var result = Rop.Ok(1);

        await Assert.That(TestSupport.Catch(() => result.Match(null!, _ => 0)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => result.Match(_ => 0, null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task OrThrow_ReturnsValueOnSuccess()
    {
        await Assert.That(Rop.Ok<int, RopError>(5).OrThrow()).IsEqualTo(5);
        await Assert.That(Rop.Ok<int, RopError>(5).OkOrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task OrThrow_OnError_ThrowsInvalidOperation()
    {
        var exception = TestSupport.Catch(() => Rop.Err<int, RopError>(Sample.Error()).OrThrow());

        await Assert.That(exception is InvalidOperationException).IsTrue();
        await Assert.That(exception!.Message).Contains("E_TEST");
    }

    [Test]
    public async Task ErrOrThrow_ReturnsErrorOnFailure()
    {
        var error = Sample.Error();

        await Assert.That(Rop.Err<int, RopError>(error).ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task ErrOrThrow_OnSuccess_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Ok<int, RopError>(1).ErrOrThrow());

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task Unit_HasValueEquality()
    {
        await Assert.That(Unit.Value).IsEqualTo(default(Unit));
        await Assert.That(Unit.Value.ToString()).IsEqualTo("()");
        await Assert.That(Unit.Value == default).IsTrue();
        await Assert.That(Unit.Value != default).IsFalse();
    }
}
