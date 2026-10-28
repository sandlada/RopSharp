using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncLinqTests
{
    [Test]
    public async Task Select_OnTaskOption_ProjectsSome()
    {
        var option = await Task.FromResult(Opt.Some(2)).Select(value => value * 3);

        await Assert.That(option.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task Select_OnTaskOption_PassesNone()
    {
        var option = await Task.FromResult(Opt.None<int>()).Select(value => value * 3);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Where_OnTaskOption_Filters()
    {
        var kept = await Task.FromResult(Opt.Some(2)).Where(value => value % 2 == 0);
        var dropped = await Task.FromResult(Opt.Some(3)).Where(value => value % 2 == 0);

        await Assert.That(kept.OrThrow()).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithAsyncBinder_ProjectsPair()
    {
        var option = await Task.FromResult(Opt.Some(1)).SelectMany(
            value => Task.FromResult(Opt.Some(value + 1)),
            static (first, second) => first + second);

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task SelectMany_WithSyncBinder_ProjectsPair()
    {
        var option = await Task.FromResult(Opt.Some(1)).SelectMany(
            value => Opt.Some(value + 1),
            static (first, second) => first + second);

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task SelectMany_WithNoneSource_DoesNotInvokeBinder()
    {
        var called = false;

        var option = await Task.FromResult(Opt.None<int>()).SelectMany(
            value =>
            {
                called = true;
                return Task.FromResult(Opt.Some(value + 1));
            },
            static (first, second) => first + second);

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithSyncBinderAndNoneSource_DoesNotInvokeBinder()
    {
        var called = false;

        var option = await Task.FromResult(Opt.None<int>()).SelectMany(
            value =>
            {
                called = true;
                return Opt.Some(value + 1);
            },
            static (first, second) => first + second);

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithAsyncBinderReturningNone_ReturnsNone()
    {
        var option = await Task.FromResult(Opt.Some(1)).SelectMany(
            _ => Task.FromResult(Opt.None<int>()),
            static (first, second) => first + second);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithNullBinderTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Task.FromResult(Opt.Some(1)).SelectMany<int, int, int>(
                _ => null!,
                static (first, second) => first + second));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task QuerySyntax_SelectAndWhere_WorkAcrossTaskOption()
    {
        var query =
            from value in Task.FromResult(Opt.Some(2))
            where value > 1
            select value * 3;

        var option = await query;

        await Assert.That(option.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task QuerySyntax_TwoFromClauses_WorkAcrossTaskOption()
    {
        var query =
            from first in Task.FromResult(Opt.Some(1))
            from second in Opt.Some(2)
            select first + second;

        var option = await query;

        await Assert.That(option.OrThrow()).IsEqualTo(3);
    }
}
