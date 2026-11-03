using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionCombinatorTests
{
    [Test]
    public async Task MapSome_MapsPresentValue()
    {
        var option = Option.Some(2).MapSome(value => value * 10);

        await Assert.That(option.Expect("test")).IsEqualTo(20);
    }

    [Test]
    public async Task MapSome_DoesNotInvokeMapperOnNone()
    {
        var called = false;

        var option = Option.None<int>().MapSome(value =>
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
        var source = Option.Some(1);

        var option = source.TeeSome(observed.Add);

        await Assert.That(option).IsEqualTo(source);
        await Assert.That(observed.Count).IsEqualTo(1);
    }

    [Test]
    public async Task TeeSome_DoesNotObserveNone()
    {
        var observed = false;

        var option = Option.None<int>().TeeSome(_ => observed = true);

        await Assert.That(observed).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task TeeNone_ObservesAndReturnsSameOption()
    {
        var observed = 0;
        var source = Option.None<int>();

        var option = source.TeeNone(() => observed++);

        await Assert.That(option).IsEqualTo(source);
        await Assert.That(observed).IsEqualTo(1);
    }

    [Test]
    public async Task TeeNone_DoesNotObserveSome()
    {
        var observed = false;

        var option = Option.Some(1).TeeNone(() => observed = true);

        await Assert.That(observed).IsFalse();
        await Assert.That(option.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task Filter_KeepsValueWhenPredicateHolds()
    {
        var source = Option.Some(2);

        var option = source.Filter(value => value % 2 == 0);

        await Assert.That(option).IsEqualTo(source);
    }

    [Test]
    public async Task Filter_ReturnsNoneWhenPredicateFails()
    {
        var option = Option.Some(3).Filter(value => value % 2 == 0);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Filter_DoesNotEvaluatePredicateOnNone()
    {
        var called = false;

        var option = Option.None<int>().Filter(_ =>
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
        var some = Option.Some(1).MapSomeValue(Unit.Value);
        var none = Option.None<int>().MapSomeValue(Unit.Value);

        await Assert.That(some.Expect("test")).IsEqualTo(Unit.Value);
        await Assert.That(none.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSome_ChainsAndShortCircuits()
    {
        var chained = Option.Some(2)
            .BindSome(value => Option.Some(value * 3))
            .BindSome(value => Option.Some(value + 1));

        var none = Option.Some(2)
            .BindSome(_ => Option.None<int>())
            .BindSome(value => Option.Some(value + 1));

        await Assert.That(chained.Expect("test")).IsEqualTo(7);
        await Assert.That(none.IsNone).IsTrue();
    }

    [Test]
    public async Task BindSome_DoesNotInvokeBinderOnNone()
    {
        var called = false;

        var option = Option.None<int>().BindSome(value =>
        {
            called = true;
            return Option.Some(value);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task BindNone_RecoversFromNone()
    {
        var recovered = Option.None<int>().BindNone(() => Option.Some(5));
        var stillNone = Option.None<int>().BindNone(static () => Option.None<int>());

        await Assert.That(recovered.Expect("test")).IsEqualTo(5);
        await Assert.That(stillNone.IsNone).IsTrue();
    }

    [Test]
    public async Task BindNone_DoesNotInvokeFallbackOnSome()
    {
        var called = false;
        var source = Option.Some(1);

        var option = source.BindNone(() =>
        {
            called = true;
            return Option.Some(9);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option).IsEqualTo(source);
    }

    [Test]
    public async Task BindNoneValue_WithFactory_IsLazy()
    {
        var calls = 0;

        var some = Option.Some(1).BindNoneValue(() =>
        {
            calls++;
            return 9;
        });

        var none = Option.None<int>().BindNoneValue(() =>
        {
            calls++;
            return 9;
        });

        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(some.Expect("test")).IsEqualTo(1);
        await Assert.That(none.Expect("test")).IsEqualTo(9);
    }

    [Test]
    public async Task Flatten_UnwrapsNestedOptions()
    {
        await Assert.That(Option.Some(Option.Some("x")).Flatten().Expect("test")).IsEqualTo("x");
        await Assert.That(Option.Some(Option.None<string>()).Flatten().IsNone).IsTrue();
        await Assert.That(Option.None<Option<string>>().Flatten().IsNone).IsTrue();
    }
}
