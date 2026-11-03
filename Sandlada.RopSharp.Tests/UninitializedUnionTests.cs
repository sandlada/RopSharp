using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class UninitializedUnionTests
{
    [Test]
    public async Task DefaultResult_WithValueTypeError_IsNotEqualToErrZero()
    {
        var unset = default(Result<string, int>);
        var errZero = Result.Err<string, int>(0);

        await Assert.That(() => { unset.TryGetErr(out _); return 0; }).Throws<InvalidOperationException>();
        await Assert.That(() => unset.Match(ok: s => s, err: e => e.ToString())).Throws<InvalidOperationException>();
        await Assert.That(unset.Equals(errZero)).IsFalse();
        await Assert.That(unset == errZero).IsFalse();
    }

    [Test]
    public async Task DefaultEither_IsNotEqualToRightZero()
    {
        var unset = default(Either<string, int>);
        var rightZero = Either.Right<string, int>(0);

        await Assert.That(unset.IsLeft).IsFalse();
        await Assert.That(unset.IsRight).IsFalse();
        await Assert.That(() => unset.Match(l => l, r => r.ToString())).Throws<InvalidOperationException>();
        await Assert.That(unset.Equals(rightZero)).IsFalse();
    }

    [Test]
    public async Task Either_BindRight_OnlyRunsOnTheRightBranch()
    {
        var calls = 0;
        var left = Either.Left<string, int>("skip")
            .BindRight(_ =>
            {
                calls++;
                return Either.Right<string, int>(1);
            });

        await Assert.That(calls).IsEqualTo(0);
        await Assert.That(left.TryGetLeft(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("skip");
    }

    [Test]
    public async Task Either_BindRight_UninitializedBinderResultStaysUninitialized()
    {
        var outcome = Either.Right<string, int>(42)
            .BindRight(_ => default(Either<string, int>));

        await Assert.That(outcome.IsLeft).IsFalse();
        await Assert.That(outcome.IsRight).IsFalse();
        await Assert.That(() => outcome.Match(l => l, r => r.ToString())).Throws<InvalidOperationException>();
    }
}
