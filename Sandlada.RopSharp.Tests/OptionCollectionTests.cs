using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionCollectionTests
{
    [Test]
    public async Task Combine2_WithBothSome_Merges()
    {
        var option = Opt.Combine(Opt.Some(2), Opt.Some(3), (int a, int b) => a * b);

        await Assert.That(option.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task Combine2_WithNone_ReturnsNoneAndSkipsMerger()
    {
        var called = false;

        var option = Opt.Combine(Opt.None<int>(), Opt.Some(3), (int a, int b) =>
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
        var option = Opt.Combine(Opt.Some(1), Opt.Some(2), Opt.Some(3), (int a, int b, int c) => a + b + c);

        await Assert.That(option.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task Combine3_WithAnyNone_ReturnsNone()
    {
        var option = Opt.Combine(Opt.Some(1), Opt.None<int>(), Opt.Some(3), (int a, int b, int c) => a + b + c);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Zip_PairsOptions()
    {
        var pair = Opt.Zip(Opt.Some(1), Opt.Some("a")).OrThrow();
        var triple = Opt.Zip(Opt.Some(1), Opt.Some("a"), Opt.Some(true)).OrThrow();

        await Assert.That(pair.Item1).IsEqualTo(1);
        await Assert.That(pair.Item2).IsEqualTo("a");
        await Assert.That(triple.Item3).IsTrue();
    }

    [Test]
    public async Task Zip_WithNone_ReturnsNone()
    {
        await Assert.That(Opt.Zip(Opt.Some(1), Opt.None<string>()).IsNone).IsTrue();
        await Assert.That(Opt.Zip(Opt.Some(1), Opt.Some("a"), Opt.None<bool>()).IsNone).IsTrue();
    }

    [Test]
    public async Task Sequence_WithEmptySource_ReturnsSomeEmpty()
    {
        var option = Opt.Sequence(Array.Empty<Option<int>>());

        await Assert.That(option.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task Sequence_WithAllSome_PreservesOrder()
    {
        var option = Opt.Sequence(new[] { Opt.Some(1), Opt.Some(2), Opt.Some(3) });

        await TestSupport.AssertSequenceAsync(option.OrThrow(), 1, 2, 3);
    }

    [Test]
    public async Task Sequence_WithNone_ShortCircuits()
    {
        var source = new CountingSequence<Option<int>>(
        [
            Opt.Some(1),
            Opt.None<int>(),
            Opt.Some(3),
        ]);

        var option = Opt.Sequence(source);

        await Assert.That(option.IsNone).IsTrue();
        await Assert.That(source.GetEnumeratorCalls).IsEqualTo(1);
        await Assert.That(source.ItemsYielded).IsEqualTo(2);
    }

    [Test]
    public async Task Traverse_MapsAndShortCircuits()
    {
        var some = Opt.Traverse([1, 2, 3], value => Opt.Some(value * 2));
        var none = Opt.Traverse([1, 2, 3], value => value == 2 ? Opt.None<int>() : Opt.Some(value));

        await TestSupport.AssertSequenceAsync(some.OrThrow(), 2, 4, 6);
        await Assert.That(none.IsNone).IsTrue();
    }

    [Test]
    public async Task Values_DiscardsNoneAndPreservesOrder()
    {
        var values = Opt.Values(new[] { Opt.Some(1), Opt.None<int>(), Opt.Some(3) });

        await TestSupport.AssertSequenceAsync(values, 1, 3);
    }

    [Test]
    public async Task Values_WithEmptySource_ReturnsEmpty()
    {
        var values = Opt.Values(Array.Empty<Option<int>>());

        await Assert.That(values.Count).IsEqualTo(0);
    }
}
