using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class EitherTests
{
    [Test]
    public async Task LeftAndRight_ExposeBranches()
    {
        var left = Either.Left<string, int>("bad");
        var right = Either.Right<string, int>(42);

        await Assert.That(left.IsLeft).IsTrue();
        await Assert.That(left.IsRight).IsFalse();
        await Assert.That(right.IsLeft).IsFalse();
        await Assert.That(right.IsRight).IsTrue();
    }

    [Test]
    public async Task TryGet_ReturnsOnlyTheActiveBranch()
    {
        var left = Either.Left<string, int>("bad");
        var right = Either.Right<string, int>(42);

        await Assert.That(left.TryGetLeft(out var leftValue)).IsTrue();
        await Assert.That(leftValue).IsEqualTo("bad");
        await Assert.That(left.TryGetRight(out _)).IsFalse();
        await Assert.That(right.TryGetRight(out var rightValue)).IsTrue();
        await Assert.That(rightValue).IsEqualTo(42);
        await Assert.That(right.TryGetLeft(out _)).IsFalse();
    }

    [Test]
    public async Task Match_ProjectsBothBranches()
    {
        var left = Either.Left<string, int>("bad");
        var right = Either.Right<string, int>(42);

        await Assert.That(left.Match(value => value, value => value.ToString())).IsEqualTo("bad");
        await Assert.That(right.Match(value => value, value => value.ToString())).IsEqualTo("42");
    }

    [Test]
    public async Task MapLeft_AndMapRight_OnlyTouchTheirBranch()
    {
        var left = Either.Left<int, string>(2);
        var right = Either.Right<int, string>("x");

        await Assert.That(left.MapLeft(value => value * 10).TryGetLeft(out var mapped)).IsTrue();
        await Assert.That(mapped).IsEqualTo(20);
        await Assert.That(right.MapLeft(value => value * 10).TryGetRight(out var untouched)).IsTrue();
        await Assert.That(untouched).IsEqualTo("x");

        await Assert.That(right.MapRight(value => value.ToUpperInvariant()).TryGetRight(out var upper)).IsTrue();
        await Assert.That(upper).IsEqualTo("X");
        await Assert.That(left.MapRight(value => value.ToUpperInvariant()).TryGetLeft(out var leftUntouched)).IsTrue();
        await Assert.That(leftUntouched).IsEqualTo(2);
    }

    [Test]
    public async Task Swap_FlipsBranches()
    {
        var right = Either.Right<string, int>(42).Swap();

        await Assert.That(right.IsLeft).IsTrue();
        await Assert.That(right.TryGetLeft(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task ToResult_MapsLeftToErrorAndRightToSuccess()
    {
        var error = Sample.Error("E_LEFT");
        var left = Either.Left<string, int>("bad").ToResult(_ => error);
        var right = Either.Right<string, int>(42).ToResult(_ => error);

        await Assert.That(left.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(right.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Equality_ComparesBranchAndPayload()
    {
        var left = Either.Left<string, int>("a");
        var same = Either.Left<string, int>("a");
        var different = Either.Left<string, int>("b");
        var right = Either.Right<string, int>(1);

        await Assert.That(left).IsEqualTo(same);
        await Assert.That(left == same).IsTrue();
        await Assert.That(left.Equals(different)).IsFalse();
        await Assert.That(left.Equals(right)).IsFalse();
        await Assert.That(left != right).IsTrue();
        await Assert.That(left.Equals((object)same)).IsTrue();
        await Assert.That(left.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(right.GetHashCode()).IsEqualTo(Either.Right<string, int>(1).GetHashCode());
        await Assert.That(default(Either<string, string>).GetHashCode())
            .IsEqualTo(default(Either<string, string>).GetHashCode());
    }

    [Test]
    public async Task ToString_ShowsBranchAndPayload()
    {
        await Assert.That(Either.Left<string, int>("bad").ToString()).IsEqualTo("Left(bad)");
        await Assert.That(Either.Right<string, int>(42).ToString()).IsEqualTo("Right(42)");
    }

    [Test]
    public async Task NullGuards_Throw()
    {
        await Assert.That(TestSupport.Catch(() => Either.Left<string, int>(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Either.Right<string, string>(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Either.Left<string, int>("x").Match(null!, value => value)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Either.Left<string, int>("x").ToResult(null!)) is ArgumentNullException).IsTrue();
    }
}
