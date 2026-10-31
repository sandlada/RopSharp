using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class LinqTests
{
    private static Result<Sample.User, RopError> GetUser(int id) =>
        id > 0
            ? Result.Ok(new Sample.User("bob", id))
            : Result.Err<Sample.User>(Sample.Error("E_USER", "missing user"));

    private static Result<Sample.Order, RopError> GetOrder(int age) =>
        Result.Ok(new Sample.Order("order", age * 10m));

    [Test]
    public async Task Result_Select_ProjectsSuccess()
    {
        var query = from user in GetUser(2)
                    select user.Name;

        await Assert.That(query.OrThrow()).IsEqualTo("bob");
    }

    [Test]
    public async Task Result_TwoFromClauses_BindAndProject()
    {
        var query = from user in GetUser(2)
                    from order in GetOrder(user.Age)
                    select order.Amount;

        await Assert.That(query.OrThrow()).IsEqualTo(20m);
    }

    [Test]
    public async Task Result_Query_PropagatesError()
    {
        var query = from user in GetUser(-1)
                    from order in GetOrder(user.Age)
                    select order.Amount;

        await Assert.That(query.ErrOrThrow().Code).IsEqualTo("E_USER");
    }

    [Test]
    public async Task Option_SelectAndWhere_Compose()
    {
        var query = from a in Option.Some(1)
                    from b in Option.Some(2)
                    where a < b
                    select a + b;

        await Assert.That(query.OrThrow()).IsEqualTo(3);
    }

    [Test]
    public async Task Option_Where_CanRejectValue()
    {
        var query = from a in Option.Some(2)
                    from b in Option.Some(1)
                    where a < b
                    select a + b;

        await Assert.That(query.IsNone).IsTrue();
    }

    [Test]
    public async Task Option_Query_PropagatesNone()
    {
        var query = from a in Option.Some(1)
                    from b in Option.None<int>()
                    select a + b;

        await Assert.That(query.IsNone).IsTrue();
    }
}
