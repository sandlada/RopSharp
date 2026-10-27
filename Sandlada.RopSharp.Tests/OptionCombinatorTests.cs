using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionCombinatorTests
{
    [Test]
    public async Task MapSome_MapsPresentValue()
    {
        var option = Opt.Some(2).MapSome(value => value * 10);

        await Assert.That(option.OrThrow()).IsEqualTo(20);
    }

    [Test]
    public async Task MapSome_DoesNotInvokeMapperOnNone()
    {
        var called = false;

        var option = Opt.None<int>().MapSome(value =>
        {
            called = true;
            return value * 10;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeSome_ObservesAndReturnsSameOption()
    {
        var observed = new List<int>();
        var source = Opt.Some(1);

        var option = source.TeeSome(observed.Add);

        await Assert.That(option).IsEqualTo(source);
        await Assert.That(observed.Count).IsEqualTo(1);
    }

    [Test]
    public async Task TeeSome_DoesNotObserveNone()
    {
        var observed = false;

        var option = Opt.None<int>().TeeSome(_ => observed = true);

        await Assert.That(observed).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeNone_ObservesAndReturnsSameOption()
    {
        var observed = 0;
        var source = Opt.None<int>();

        var option = source.TeeNone(() => observed++);

        await Assert.That(option).IsEqualTo(source);
        await Assert.That(observed).IsEqualTo(1);
    }

    [Test]
    public async Task TeeNone_DoesNotObserveSome()
    {
        var observed = false;

        var option = Opt.Some(1).TeeNone(() => observed = true);

        await Assert.That(observed).IsFalse();
        await Assert.That(option.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task Filter_KeepsValueWhenPredicateHolds()
    {
        var source = Opt.Some(2);

        var option = source.Filter(value => value % 2 == 0);

        await Assert.That(option).IsEqualTo(source);
    }

    [Test]
    public async Task Filter_ReturnsNoneWhenPredicateFails()
    {
        var option = Opt.Some(3).Filter(value => value % 2 == 0);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Filter_DoesNotEvaluatePredicateOnNone()
    {
        var called = false;

        var option = Opt.None<int>().Filter(_ =>
        {
            called = true;
            return true;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task MapSomeValue_ReplacesValue()
    {
        var some = Opt.Some(1).MapSomeValue(Unit.Value);
        var none = Opt.None<int>().MapSomeValue(Unit.Value);

        await Assert.That(some.OrThrow()).IsEqualTo(Unit.Value);
        await Assert.That(none.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSome_ChainsAndShortCircuits()
    {
        var chained = Opt.Some(2)
            .BindSome(value => Opt.Some(value * 3))
            .BindSome(value => Opt.Some(value + 1));

        var none = Opt.Some(2)
            .BindSome(_ => Opt.None<int>())
            .BindSome(value => Opt.Some(value + 1));

        await Assert.That(chained.OrThrow()).IsEqualTo(7);
        await Assert.That(none.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSome_DoesNotInvokeBinderOnNone()
    {
        var called = false;

        var option = Opt.None<int>().BindSome(value =>
        {
            called = true;
            return Opt.Some(value);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindNone_RecoversFromNone()
    {
        var recovered = Opt.None<int>().BindNone(() => Opt.Some(5));
        var stillNone = Opt.None<int>().BindNone(static () => Opt.None<int>());

        await Assert.That(recovered.OrThrow()).IsEqualTo(5);
        await Assert.That(stillNone.IsNone).IsTrue();
    }

    [Test]
    public async Task BindNone_DoesNotInvokeFallbackOnSome()
    {
        var called = false;
        var source = Opt.Some(1);

        var option = source.BindNone(() =>
        {
            called = true;
            return Opt.Some(9);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option).IsEqualTo(source);
    }

    [Test]
    public async Task BindNoneValue_WithFactory_IsLazy()
    {
        var calls = 0;

        var some = Opt.Some(1).BindNoneValue(() =>
        {
            calls++;
            return 9;
        });

        var none = Opt.None<int>().BindNoneValue(() =>
        {
            calls++;
            return 9;
        });

        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(some.OrThrow()).IsEqualTo(1);
        await Assert.That(none.OrThrow()).IsEqualTo(9);
    }

    [Test]
    public async Task Flatten_UnwrapsNestedOptions()
    {
        await Assert.That(Opt.Some(Opt.Some("x")).Flatten().OrThrow()).IsEqualTo("x");
        await Assert.That(Opt.Some(Opt.None<string>()).Flatten().IsNone).IsTrue();
        await Assert.That(Opt.None<Option<string>>().Flatten().IsNone).IsTrue();
    }
}
