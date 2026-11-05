using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class OptionAsyncLinqTests
{
    [Test]
    public async Task Select_OnTaskOption_ProjectsSome()
    {
        var option = await OptionTask.From(Task.FromResult(Option.Some(2))).Select(value => value * 3);

        await Assert.That(option.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task Select_OnTaskOption_PassesNone()
    {
        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).Select(value => value * 3);

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Where_OnTaskOption_Filters()
    {
        var kept = await OptionTask.From(Task.FromResult(Option.Some(2))).Where(value => value % 2 == 0);
        var dropped = await OptionTask.From(Task.FromResult(Option.Some(3))).Where(value => value % 2 == 0);

        await Assert.That(kept.Expect("test")).IsEqualTo(2);
        await Assert.That(dropped.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithAsyncBinder_ProjectsPair()
    {
        var option = await OptionTask.From(Task.FromResult(Option.Some(1)))
            .SelectMany(first => OptionTask.From(Option.Some(first + 1)), (first, second) => first + second);

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task SelectMany_WithSyncBinder_ProjectsPair()
    {
        var option = await OptionTask.From(Option.Some(1))
            .SelectMany(value => Option.Some(value + 1), static (first, second) => first + second);

        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task SelectMany_WithNoneSource_DoesNotInvokeBinder()
    {
        var called = false;

        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).BindSomeAsync((value, _) =>
        {
            called = true;
            return Task.FromResult(Option.Some(value + 1));
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithSyncBinderAndNoneSource_DoesNotInvokeBinder()
    {
        var called = false;

        var option = await OptionTask.From(Task.FromResult(Option.None<int>())).BindSome(value =>
        {
            called = true;
            return Option.Some(value + 1);
        });

        await Assert.That(called).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithAsyncBinderReturningNone_ReturnsNone()
    {
        var option = await OptionTask.From(Task.FromResult(Option.Some(1)))
            .SelectMany(_ => OptionTask.From(Option.None<int>()));

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task SelectMany_WithNullBinderTask_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            OptionTask.From(Task.FromResult(Option.Some(1))).BindSomeAsync((_, _) => (Task<Option<int>>)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task QuerySyntax_SelectAndWhere_WorkAcrossTaskOption()
    {
        var query =
            from value in OptionTask.From(Option.Some(2))
            where value > 1
            select value * 3;

        var option = await query;
        await Assert.That(option.Expect("test")).IsEqualTo(6);
    }

    [Test]
    public async Task QuerySyntax_TwoFromClauses_WorkAcrossTaskOption()
    {
        var query =
            from first in OptionTask.From(Option.Some(1))
            from second in OptionTask.From(Option.Some(2))
            select first + second;

        var option = await query;
        await Assert.That(option.Expect("test")).IsEqualTo(3);
    }

    [Test]
    public async Task QuerySyntax_FromTaskAndFromSync_WorkAcrossTaskOption()
    {
        var query =
            from first in OptionTask.From(Option.Some(10))
            from second in Option.Some(20)
            select first + second;

        var option = await query;
        await Assert.That(option.Expect("test")).IsEqualTo(30);
    }
}
