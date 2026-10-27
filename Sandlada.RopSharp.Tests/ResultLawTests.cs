using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultLawTests
{
    private static readonly Func<int, int> F = x => x + 1;
    private static readonly Func<int, int> G = x => x * 2;
    private static readonly Func<int, Result<int, RopError>> H = x =>
        x >= 0 ? Rop.Ok(x * 3) : Rop.Err<int>(Sample.Error("E_NEG"));
    private static readonly Func<int, Result<int, RopError>> K = x => Rop.Ok(x + 100);
    private static readonly Func<RopError, Result<int, RopError>> Eh = error => Rop.Ok(error.Code.Length);
    private static readonly Func<RopError, Result<int, RopError>> Ek = error => Rop.Err<int>(error.WithCode("E_SECOND"));

    private static IEnumerable<Result<int, RopError>> Samples()
    {
        yield return Rop.Ok(5);
        yield return Rop.Ok(0);
        yield return Rop.Err<int>(Sample.Error("E_A"));
    }

    [Test]
    public async Task F1_MapOkIdentity()
    {
        foreach (var result in Samples())
        {
            await Assert.That(result.MapOk(x => x)).IsEqualTo(result);
        }
    }

    [Test]
    public async Task F2_MapOkComposition()
    {
        foreach (var result in Samples())
        {
            var composed = result.MapOk(F).MapOk(G);
            var combined = result.MapOk(x => G(F(x)));

            await Assert.That(composed).IsEqualTo(combined);
        }
    }

    [Test]
    public async Task F3_MapErrIdentity()
    {
        foreach (var result in Samples())
        {
            await Assert.That(result.MapErr(error => error)).IsEqualTo(result);
        }
    }

    [Test]
    public async Task F4_MapErrComposition()
    {
        Func<RopError, RopError> first = error => error.WithCode("E_ONE");
        Func<RopError, RopError> second = error => error.WithMessage("done");

        foreach (var result in Samples())
        {
            var composed = result.MapErr(first).MapErr(second);
            var combined = result.MapErr(error => second(first(error)));

            await Assert.That(composed).IsEqualTo(combined);
        }
    }

    [Test]
    public async Task M1_BindLeftIdentity()
    {
        const int Value = 7;

        var left = Rop.Ok(Value).BindOk(H);
        var right = H(Value);

        await Assert.That(left).IsEqualTo(right);
    }

    [Test]
    public async Task M2_BindRightIdentity()
    {
        foreach (var result in Samples())
        {
            var bound = result.BindOk<int, int, RopError>(Rop.Ok<int, RopError>);

            await Assert.That(bound).IsEqualTo(result);
        }
    }

    [Test]
    public async Task M3_BindAssociativity()
    {
        foreach (var result in Samples())
        {
            var left = result.BindOk(H).BindOk(K);
            var right = result.BindOk(x => H(x).BindOk(K));

            await Assert.That(left).IsEqualTo(right);
        }
    }

    [Test]
    public async Task E1_BindErrLeftIdentity()
    {
        var error = Sample.Error("E_A");

        var left = Rop.Err<int>(error).BindErr(Eh);
        var right = Eh(error);

        await Assert.That(left).IsEqualTo(right);
    }

    [Test]
    public async Task E2_BindErrRightIdentity()
    {
        foreach (var result in Samples())
        {
            var bound = result.BindErr<int, RopError>(Rop.Err<int, RopError>);

            await Assert.That(bound).IsEqualTo(result);
        }
    }

    [Test]
    public async Task E3_BindErrAssociativity()
    {
        foreach (var result in Samples())
        {
            var left = result.BindErr(Eh).BindErr(Ek);
            var right = result.BindErr(error => Eh(error).BindErr(Ek));

            await Assert.That(left).IsEqualTo(right);
        }
    }

    [Test]
    public async Task T1_TeeDoesNotChangeTrack()
    {
        var observed = new List<string>();

        foreach (var result in Samples())
        {
            var teeOk = result.TeeOk(value => observed.Add($"ok:{value}"));
            var teeErr = result.TeeErr(error => observed.Add($"err:{error.Code}"));

            await Assert.That(teeOk).IsEqualTo(result);
            await Assert.That(teeErr).IsEqualTo(result);
        }
    }

    [Test]
    public async Task X1_OppositeTrackFunctionsAreNeverInvoked()
    {
        var mapErrCalled = false;
        var mapOkCalled = false;

        var ok = Rop.Ok(1).MapErr(error =>
        {
            mapErrCalled = true;
            return error;
        });

        var err = Rop.Err<int>(Sample.Error()).MapOk(value =>
        {
            mapOkCalled = true;
            return value;
        });

        await Assert.That(mapErrCalled).IsFalse();
        await Assert.That(mapOkCalled).IsFalse();
        await Assert.That(ok.OrThrow()).IsEqualTo(1);
        await Assert.That(err.IsErr).IsTrue();
    }
}
