using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionBridgeTests
{
    [Test]
    public async Task RequireSome_WithValue_ProducesOk()
    {
        var result = Opt.Some(5).RequireSome(Sample.Error());

        await Assert.That(result.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task RequireSome_WithNone_ProducesErr()
    {
        var error = Sample.Error("E_NONE");

        var result = Opt.None<int>().RequireSome(error);

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task RequireSome_WithFactory_IsLazy()
    {
        var calls = 0;

        var some = Opt.Some(1).RequireSome(() =>
        {
            calls++;
            return Sample.Error();
        });

        var none = Opt.None<int>().RequireSome(() =>
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
        await Assert.That(Rop.Ok(1).ToOption().OrThrow()).IsEqualTo(1);
        await Assert.That(Rop.Err<int>(Sample.Error()).ToOption().IsNone).IsTrue();
    }

    [Test]
    public async Task ToOption_WithNullPayload_ReturnsNone()
    {
        var source = Rop.Ok<string>("x").MapOk(_ => (string?)null);

        var option = source.ToOption();

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task ToNullable_ForReferenceTypes()
    {
        await Assert.That(Opt.Some("x").ToNullable()).IsEqualTo("x");
        await Assert.That(Opt.None<string>().ToNullable()).IsNull();
    }

    [Test]
    public async Task ToNullableValue_ForValueTypes()
    {
        await Assert.That(Opt.Some(5).ToNullableValue()).IsEqualTo(5);
        await Assert.That(Opt.None<int>().ToNullableValue()).IsNull();
    }

    [Test]
    public async Task BindSomeNullable_MapsNullToNone()
    {
        var profile = new Sample.Profile(null, null);
        var withValue = new Sample.Profile("nick", null);

        var none = Opt.Some(profile).BindSomeNullable(value => value.Nickname);
        var some = Opt.Some(withValue).BindSomeNullable(value => value.Nickname);

        await Assert.That(none.IsNone).IsTrue();
        await Assert.That(some.OrThrow()).IsEqualTo("nick");
    }

    [Test]
    public async Task BindSomeNullable_DoesNotInvokeBinderOnNone()
    {
        var called = false;

        var option = Opt.None<Sample.Profile>().BindSomeNullable(value =>
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

        var none = Opt.Some(missing).BindSomeNullableValue(value => value.Score);
        var some = Opt.Some(withScore).BindSomeNullableValue(value => value.Score);

        await Assert.That(none.IsNone).IsTrue();
        await Assert.That(some.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task Transpose_OptionOfResult_ToResultOfOption()
    {
        var ok = Opt.Some(Rop.Ok(1)).Transpose();
        var err = Opt.Some(Rop.Err<int>(Sample.Error("E_X"))).Transpose();
        var none = Opt.None<Result<int, RopError>>().Transpose();

        await Assert.That(ok.IsOk).IsTrue();
        await Assert.That(ok.OrThrow().OrThrow()).IsEqualTo(1);
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_X");
        await Assert.That(none.IsOk).IsTrue();
        await Assert.That(none.OrThrow().IsNone).IsTrue();
    }

    [Test]
    public async Task Transpose_ResultOfOption_ToOptionOfResult()
    {
        var some = Rop.Ok(Opt.Some(1)).Transpose();
        var none = Rop.Ok(Opt.None<int>()).Transpose();
        var err = Rop.Err<Option<int>>(Sample.Error("E_X")).Transpose();

        await Assert.That(some.OrThrow().OrThrow()).IsEqualTo(1);
        await Assert.That(none.IsNone).IsTrue();
        await Assert.That(err.OrThrow().ErrOrThrow().Code).IsEqualTo("E_X");
    }

    [Test]
    public async Task Transpose_IsInverseForSuccess()
    {
        var original = Rop.Ok(Opt.Some(3));

        var roundTrip = original.Transpose().Transpose();

        await Assert.That(roundTrip.OrThrow().OrThrow()).IsEqualTo(3);
    }
}
