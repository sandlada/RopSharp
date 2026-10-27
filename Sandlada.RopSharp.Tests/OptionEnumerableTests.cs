using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionEnumerableTests
{
    [Test]
    public async Task TryFirst_ReturnsFirstOrNone()
    {
        await Assert.That(new[] { 1, 2, 3 }.TryFirst().OrThrow()).IsEqualTo(1);
        await Assert.That(Array.Empty<int>().TryFirst().IsNone).IsTrue();
    }

    [Test]
    public async Task TryFirst_WithPredicate_ReturnsFirstMatch()
    {
        var match = new[] { 1, 2, 3, 4 }.TryFirst(value => value % 2 == 0);
        var missing = new[] { 1, 3, 5 }.TryFirst(value => value % 2 == 0);

        await Assert.That(match.OrThrow()).IsEqualTo(2);
        await Assert.That(missing.IsNone).IsTrue();
    }

    [Test]
    public async Task TryLast_ReturnsLastOrNone()
    {
        await Assert.That(new[] { 1, 2, 3 }.TryLast().OrThrow()).IsEqualTo(3);
        await Assert.That(Array.Empty<int>().TryLast().IsNone).IsTrue();
    }

    [Test]
    public async Task TryLast_WithPredicate_ReturnsLastMatch()
    {
        var match = new[] { 1, 2, 3, 4 }.TryLast(value => value % 2 == 0);
        var missing = new[] { 1, 3, 5 }.TryLast(value => value % 2 == 0);

        await Assert.That(match.OrThrow()).IsEqualTo(4);
        await Assert.That(missing.IsNone).IsTrue();
    }

    [Test]
    public async Task TryFind_ReturnsValueOrNone()
    {
        IReadOnlyDictionary<string, int> source = new Dictionary<string, int>
        {
            ["a"] = 1,
        };

        await Assert.That(source.TryFind("a").OrThrow()).IsEqualTo(1);
        await Assert.That(source.TryFind("missing").IsNone).IsTrue();
    }

    [Test]
    public async Task Choose_DiscardsNoneAndPreservesOrder()
    {
        var values = new[] { Opt.Some(1), Opt.None<int>(), Opt.Some(3) }.Choose();

        await TestSupport.AssertSequenceAsync(values, 1, 3);
    }

    [Test]
    public async Task Choose_WithMapper_ProjectsPresentValues()
    {
        var values = new[] { Opt.Some(1), Opt.None<int>(), Opt.Some(3) }
            .Choose(value => value * 10);

        await TestSupport.AssertSequenceAsync(values, 10, 30);
    }

    [Test]
    public async Task Execute_RunsOnlyForSome()
    {
        var seen = 0;
        Opt.Some(42).Execute(value => seen = value);

        var noneRan = false;
        Opt.None<int>().Execute(_ => noneRan = true);

        await Assert.That(seen).IsEqualTo(42);
        await Assert.That(noneRan).IsFalse();
    }

    [Test]
    public async Task ExecuteNoValue_RunsOnlyForNone()
    {
        var noneRan = false;
        Opt.None<int>().ExecuteNoValue(() => noneRan = true);

        var someRan = false;
        Opt.Some(42).ExecuteNoValue(() => someRan = true);

        await Assert.That(noneRan).IsTrue();
        await Assert.That(someRan).IsFalse();
    }

    [Test]
    public async Task NullGuards_Throw()
    {
        await Assert.That(TestSupport.Catch(() => ((IEnumerable<int>)null!).TryFirst()) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => ((IEnumerable<int>)null!).TryFirst(_ => true)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => ((IEnumerable<int>)null!).TryLast()) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => new[] { 1 }.TryFirst(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Opt.Some(1).Execute(null!)) is ArgumentNullException).IsTrue();
    }
}
