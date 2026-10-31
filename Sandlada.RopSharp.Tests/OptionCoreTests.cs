using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionCoreTests
{
    [Test]
    public async Task Some_ContainsValue()
    {
        var option = Option.Some(42);

        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.IsNone).IsFalse();
        await Assert.That(option.TryGetValue(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task None_IsEmpty()
    {
        var option = Option.None<int>();

        await Assert.That(option.IsSome).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
        await Assert.That(option.TryGetValue(out _)).IsFalse();
    }

    [Test]
    public async Task Default_IsNone()
    {
        Option<string> option = default;

        await Assert.That(option.IsNone).IsTrue();
        await Assert.That(option).IsEqualTo(Option.None<string>());
    }

    [Test]
    public async Task DefaultArrayAndFields_AreNone()
    {
        var slots = new Option<string>[3];

        foreach (var slot in slots)
        {
            await Assert.That(slot.IsNone).IsTrue();
        }
    }

    [Test]
    public async Task Some_WithNull_Throws()
    {
        var exception = TestSupport.Catch(() => Option.Some<string>(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task FromNullable_WithNull_ReturnsNone()
    {
        string? value = null;

        var option = Option.FromNullable(value);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task FromNullable_WithValue_ReturnsSome()
    {
        string? value = "x";

        var option = Option.FromNullable(value);

        await Assert.That(option.OrThrow()).IsEqualTo("x");
    }

    [Test]
    public async Task FromNullableValue_WithNull_ReturnsNone()
    {
        int? value = null;

        var option = Option.FromNullableValue(value);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task FromNullableValue_WithValue_ReturnsSome()
    {
        int? value = 5;

        var option = Option.FromNullableValue(value);

        await Assert.That(option.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task When_WithFalse_DoesNotEvaluateFactory()
    {
        var called = false;

        var option = Option.When(false, () =>
        {
            called = true;
            return 1;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task When_WithTrue_EvaluatesFactory()
    {
        var option = Option.When(true, () => 7);

        await Assert.That(option.OrThrow()).IsEqualTo(7);
    }

    [Test]
    public async Task Equality_ComparesPresenceAndValue()
    {
        await Assert.That(Option.Some(1).Equals(Option.Some(1))).IsTrue();
        await Assert.That(Option.Some(1) == Option.Some(1)).IsTrue();
        await Assert.That(Option.Some(1).Equals(Option.Some(2))).IsFalse();
        await Assert.That(Option.None<int>().Equals(Option.None<int>())).IsTrue();
        await Assert.That(Option.Some(1).Equals(Option.None<int>())).IsFalse();
        await Assert.That(Option.Some(1) != Option.Some(2)).IsTrue();
        await Assert.That(Option.Some("a").GetHashCode()).IsEqualTo(Option.Some("a").GetHashCode());
    }

    [Test]
    public async Task ToString_ShowsSomeOrNone()
    {
        await Assert.That(Option.Some(1).ToString()).IsEqualTo("Some(1)");
        await Assert.That(Option.None<int>().ToString()).IsEqualTo("None");
    }

    [Test]
    public async Task Match_ProjectsPresentAndEmptyTracks()
    {
        var some = Option.Some(2).Match(value => $"some:{value}", () => "none");
        var none = Option.None<int>().Match(value => $"some:{value}", () => "none");

        await Assert.That(some).IsEqualTo("some:2");
        await Assert.That(none).IsEqualTo("none");
    }

    [Test]
    public async Task Match_WithNullDelegates_Throws()
    {
        await Assert.That(TestSupport.Catch(() => Option.Some(1).Match(null!, () => 0)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Option.Some(1).Match(_ => 0, null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task OrThrow_ReturnsValueOrThrows()
    {
        await Assert.That(Option.Some(3).OrThrow()).IsEqualTo(3);

        var exception = TestSupport.Catch(() => Option.None<int>().OrThrow());

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task BindNoneValue_ProvidesFallback()
    {
        var some = Option.Some(1).BindNoneValue(9);
        var none = Option.None<int>().BindNoneValue(9);

        await Assert.That(some.OrThrow()).IsEqualTo(1);
        await Assert.That(none.OrThrow()).IsEqualTo(9);
    }
}
