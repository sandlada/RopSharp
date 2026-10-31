using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionBridgeTests
{
    [Test]
    public async Task RequireSome_WithValue_ProducesOk()
    {
        var result = Option.Some(5).RequireSome(Sample.Error());

        await Assert.That(result.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task RequireSome_WithNone_ProducesErr()
    {
        var error = Sample.Error("E_NONE");

        var result = Option.None<int>().RequireSome(error);

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task RequireSome_WithFactory_IsLazy()
    {
        var calls = 0;

        var some = Option.Some(1).RequireSome(() =>
        {
            calls++;
            return Sample.Error();
        });

        var none = Option.None<int>().RequireSome(() =>
        {
            calls++;
            return Sample.Error("E_NONE");
        });

        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(some.OrThrow()).IsEqualTo(1);
        await Assert.That(none.ErrOrThrow().Code).IsEqualTo("E_NONE");
    }

    [Test]
    public async Task ToOption_MapsTracks()
    {
        await Assert.That(Result.Ok(1).ToOption().OrThrow()).IsEqualTo(1);
        await Assert.That(Result.Err<int>(Sample.Error()).ToOption().IsNone).IsTrue();
    }

    [Test]
    public async Task ToOption_WithNullPayload_ReturnsNone()
    {
        var source = Result.Ok<string>("x").MapOk(_ => (string?)null);

        var option = source.ToOption();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task ToNullable_ForReferenceTypes()
    {
        await Assert.That(Option.Some("x").ToNullable()).IsEqualTo("x");
        await Assert.That(Option.None<string>().ToNullable()).IsNull();
    }

    [Test]
    public async Task ToNullableValue_ForValueTypes()
    {
        await Assert.That(Option.Some(5).ToNullableValue()).IsEqualTo(5);
        await Assert.That(Option.None<int>().ToNullableValue()).IsNull();
    }

    [Test]
    public async Task BindSomeNullable_MapsNullToNone()
    {
        var profile = new Sample.Profile(null, null);
        var withValue = new Sample.Profile("nick", null);

        var none = Option.Some(profile).BindSomeNullable(value => value.Nickname);
        var some = Option.Some(withValue).BindSomeNullable(value => value.Nickname);

        await Assert.That(none.IsNone).IsTrue();
        await Assert.That(some.OrThrow()).IsEqualTo("nick");
    }

    [Test]
    public async Task BindSomeNullable_DoesNotInvokeBinderOnNone()
    {
        var called = false;

        var option = Option.None<Sample.Profile>().BindSomeNullable(value =>
        {
            called = true;
            return value.Nickname;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSomeNullableValue_MapsNullToNone()
    {
        var missing = new Sample.Profile(null, null);
        var withScore = new Sample.Profile(null, 7);

        var none = Option.Some(missing).BindSomeNullableValue(value => value.Score);
        var some = Option.Some(withScore).BindSomeNullableValue(value => value.Score);

        await Assert.That(none.IsNone).IsTrue();
        await Assert.That(some.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task Transpose_OptionOfResult_ToResultOfOption()
    {
        var ok = Option.Some(Result.Ok(1)).Transpose();
        var err = Option.Some(Result.Err<int>(Sample.Error("E_X"))).Transpose();
        var none = Option.None<Result<int, RopError>>().Transpose();

        await Assert.That(ok.IsOk).IsTrue();
        await Assert.That(ok.OrThrow().OrThrow()).IsEqualTo(1);
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_X");
        await Assert.That(none.IsOk).IsTrue();
        await Assert.That(none.OrThrow().IsNone).IsTrue();
    }

    [Test]
    public async Task Transpose_ResultOfOption_ToOptionOfResult()
    {
        var some = Result.Ok(Option.Some(1)).Transpose();
        var none = Result.Ok(Option.None<int>()).Transpose();
        var err = Result.Err<Option<int>>(Sample.Error("E_X")).Transpose();

        await Assert.That(some.OrThrow().OrThrow()).IsEqualTo(1);
        await Assert.That(none.IsNone).IsTrue();
        await Assert.That(err.OrThrow().ErrOrThrow().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task Transpose_IsInverseForSuccess()
    {
        var original = Result.Ok(Option.Some(3));

        var roundTrip = original.Transpose().Transpose();

        await Assert.That(roundTrip.OrThrow().OrThrow()).IsEqualTo(3);
    }
}
