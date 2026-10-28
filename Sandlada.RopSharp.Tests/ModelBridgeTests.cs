using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ModelBridgeTests
{
    [Test]
    public async Task ResultToEither_Ok_BecomesRight()
    {
        var either = Rop.Ok(42).ToEither();

        await Assert.That(either.IsRight).IsTrue();
        await Assert.That(either.TryGetRight(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task ResultToEither_Err_BecomesLeft()
    {
        var error = Sample.Error("E_LEFT");

        var either = Rop.Err<int>(error).ToEither();

        await Assert.That(either.IsLeft).IsTrue();
        await Assert.That(either.TryGetLeft(out var value)).IsTrue();
        await Assert.That(value).IsSameReferenceAs(error);
    }

    [Test]
    public async Task ResultToEither_WithNullSuccess_Throws()
    {
        var nullSuccess = Rop.Ok("x").MapOk(_ => (string?)null);

        var exception = TestSupport.Catch(() => nullSuccess.ToEither());

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task ResultToEither_RoundTripsThroughToResult()
    {
        var error = Sample.Error("E_ROUND");
        var failed = Rop.Err<int>(error).ToEither().ToResult(identity => identity);
        var succeeded = Rop.Ok(7).ToEither().ToResult(identity => identity);

        await Assert.That(failed.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(succeeded.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task OptionToEither_Some_BecomesRight()
    {
        var either = Opt.Some(5).ToEither("missing");

        await Assert.That(either.IsRight).IsTrue();
        await Assert.That(either.TryGetRight(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo(5);
    }

    [Test]
    public async Task OptionToEither_None_BecomesLeft()
    {
        var either = Opt.None<int>().ToEither("missing");

        await Assert.That(either.IsLeft).IsTrue();
        await Assert.That(either.TryGetLeft(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("missing");
    }

    [Test]
    public async Task OptionToEither_WithNullLeft_ThrowsEagerly()
    {
        var exception = TestSupport.Catch(() => Opt.None<int>().ToEither((string)null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task OptionToEither_WithFactory_DoesNotInvokeItForSome()
    {
        var called = false;

        var either = Opt.Some(5).ToEither(() =>
        {
            called = true;
            return "missing";
        });

        await Assert.That(called).IsFalse();
        await Assert.That(either.IsRight).IsTrue();
    }

    [Test]
    public async Task OptionToEither_WithFactory_UsesItForNone()
    {
        var either = Opt.None<int>().ToEither(() => "missing");

        await Assert.That(either.TryGetLeft(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("missing");
    }

    [Test]
    public async Task OptionToEither_WithNullFactory_Throws()
    {
        var exception = TestSupport.Catch(() =>
            Opt.None<int>().ToEither((Func<string>)null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task OptionToEither_WithFactoryReturningNull_Throws()
    {
        var exception = TestSupport.Catch(() =>
            Opt.None<int>().ToEither(() => (string)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task EitherToOption_Right_BecomesSome()
    {
        var option = Either.Right<string, int>(42).ToOption();

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task EitherToOption_Left_BecomesNone()
    {
        var option = Either.Left<string, int>("bad").ToOption();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task EitherToOption_DiscardsTheLeftValue()
    {
        var withReason = Either.Left<string, int>("first reason").ToOption();
        var otherReason = Either.Left<string, int>("second reason").ToOption();

        await Assert.That(withReason).IsEqualTo(otherReason);
    }
}
