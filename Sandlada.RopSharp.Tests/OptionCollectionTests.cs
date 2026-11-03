using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionCollectionTests
{
    [Test]
    public async Task Combine2_WithBothSome_Merges()
    {
        var option = Option.Combine(Option.Some(2), Option.Some(3), (int a, int b) => a * b);

        await Assert.That(option.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task Combine2_WithNone_ReturnsNoneAndSkipsMerger()
    {
        var called = false;

        var option = Option.Combine(Option.None<int>(), Option.Some(3), (int a, int b) =>
        {
            called = true;
            return a * b;
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Combine3_WithAllSome_Merges()
    {
        var option = Option.Combine(Option.Some(1), Option.Some(2), Option.Some(3), (int a, int b, int c) => a + b + c);

        await Assert.That(option.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task Combine3_WithAnyNone_ReturnsNone()
    {
        var option = Option.Combine(Option.Some(1), Option.None<int>(), Option.Some(3), (int a, int b, int c) => a + b + c);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Zip_PairsOptions()
    {
        var pair = Option.Zip(Option.Some(1), Option.Some("a")).Expect("test");
        var triple = Option.Zip(Option.Some(1), Option.Some("a"), Option.Some(true)).Expect("test");

        await Assert.That(pair.Item1).IsEqualTo(1);
        await Assert.That(pair.Item2).IsEqualTo("a");
        await Assert.That(triple.Item3).IsTrue();
    }

    [Test]
    public async Task Zip_WithNone_ReturnsNone()
    {
        await Assert.That(Option.Zip(Option.Some(1), Option.None<string>()).IsNone).IsTrue();
        await Assert.That(Option.Zip(Option.Some(1), Option.Some("a"), Option.None<bool>()).IsNone).IsTrue();
    }

    [Test]
    public async Task Sequence_WithEmptySource_ReturnsSomeEmpty()
    {
        var option = Option.Sequence(Array.Empty<Option<int>>());

        await Assert.That(option.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task Sequence_WithAllSome_PreservesOrder()
    {
        var option = Option.Sequence(new[] { Option.Some(1), Option.Some(2), Option.Some(3) });

        await TestSupport.AssertSequenceAsync(option.Expect("test"), 1, 2, 3);
    }

    [Test]
    public async Task Sequence_WithNone_ShortCircuits()
    {
        var source = new CountingSequence<Option<int>>(
        [
            Option.Some(1),
            Option.None<int>(),
            Option.Some(3),
        ]);

        var option = Option.Sequence(source);

        await Assert.That(option.IsNone).IsTrue();
        await Assert.That(source.GetEnumeratorCalls).IsEqualTo(1);
        await Assert.That(source.ItemsYielded).IsEqualTo(2);
    }

    [Test]
    public async Task Traverse_MapsAndShortCircuits()
    {
        var some = Option.Traverse([1, 2, 3], value => Option.Some(value * 2));
        var none = Option.Traverse([1, 2, 3], value => value == 2 ? Option.None<int>() : Option.Some(value));

        await TestSupport.AssertSequenceAsync(some.Expect("test"), 2, 4, 6);
        await Assert.That(none.IsNone).IsTrue();
    }

    [Test]
    public async Task Values_DiscardsNoneAndPreservesOrder()
    {
        var values = Option.Values(new[] { Option.Some(1), Option.None<int>(), Option.Some(3) });

        await TestSupport.AssertSequenceAsync(values, 1, 3);
    }

    [Test]
    public async Task Values_WithEmptySource_ReturnsEmpty()
    {
        var values = Option.Values(Array.Empty<Option<int>>());

        await Assert.That(values.Count).IsEqualTo(0);
    }
}
