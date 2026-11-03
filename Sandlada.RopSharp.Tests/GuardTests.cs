using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class GuardTests
{
    private static Result<int, RopError> Ok => Result.Ok(1);
    private static Result<int, RopError> Err => Result.Err<int>(Sample.Error());

    [Test]
    public async Task ResultCombinators_RejectNullDelegates()
    {
        await AssertArgumentNullAsync(() => Ok.MapOk<int, int, RopError>(null!));
        await AssertArgumentNullAsync(() => Ok.MapErr<int, RopError, RopError>(null!));
        await AssertArgumentNullAsync(() => Ok.TeeOk(null!));
        await AssertArgumentNullAsync(() => Ok.TeeErr(null!));
        await AssertArgumentNullAsync(() => Ok.Ensure(null!, Sample.Error()));
        await AssertArgumentNullAsync(() => Ok.Ensure(value => value > 0, (Func<int, RopError>)null!));
        await AssertArgumentNullAsync(() => Ok.BindOk<int, int, RopError>(null!));
        await AssertArgumentNullAsync(() => Ok.BindErr<int, RopError>(null!));
        await AssertArgumentNullAsync(() => Ok.BindErrValue((Func<int>)null!));
        await AssertArgumentNullAsync(() => Result.Ok<string?>("x").RequireNotNull(null!));
    }

    [Test]
    public async Task Factories_RejectAnEmptyPayload_ButAcceptAValueTypePayload()
    {
        // The null check of the factories must not box the payload: a value type is never null, and a nullable
        // value type is null only when it carries no value.
        await AssertArgumentNullAsync(() => Result.Ok<int?, RopError>(null));
        await AssertArgumentNullAsync(() => Option.Some<int?>(null));
        await AssertArgumentNullAsync(() => Result.Err<int, string?>(null));

        var zero = Result.Ok(0);
        var nullable = Result.Ok<int?, RopError>(0);
        var some = Option.Some(0);

        await Assert.That(zero.IsOk).IsTrue();
        await Assert.That(nullable.IsOk).IsTrue();
        await Assert.That(some.IsSome).IsTrue();
    }

    [Test]
    public async Task ResultCombinators_EnsureWithNullError_ThrowsEagerlyAndDeterministically()
    {
        // The guard runs at entry, so a null error is reported even when the predicate passes and even on the
        // failure track; it must not depend on the shape of the input.
        var passed = TestSupport.Catch(() => Ok.Ensure(value => value > 0, (RopError)null!));
        var failed = TestSupport.Catch(() => Ok.Ensure(value => value < 0, (RopError)null!));
        var errTrack = TestSupport.Catch(() => Err.Ensure(value => value > 0, (RopError)null!));

        await Assert.That(passed is ArgumentNullException).IsTrue();
        await Assert.That(failed is ArgumentNullException).IsTrue();
        await Assert.That(errTrack is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task ValueArguments_AreRejectedEagerlyOnBothTracks()
    {
        await AssertArgumentNullAsync(() => Ok.MapOkValue((string)null!));
        await AssertArgumentNullAsync(() => Err.MapOkValue((string)null!));
        await AssertArgumentNullAsync(() => Result.Ok("x").BindErrValue((string)null!));
        await AssertArgumentNullAsync(() => Result.Err<string>(Sample.Error()).BindErrValue((string)null!));

        await AssertArgumentNullAsync(() => Option.Some(1).MapSomeValue((string)null!));
        await AssertArgumentNullAsync(() => Option.None<int>().MapSomeValue((string)null!));
        await AssertArgumentNullAsync(() => Option.Some("a").BindNoneValue((string)null!));
        await AssertArgumentNullAsync(() => Option.None<string>().BindNoneValue((string)null!));
        await AssertArgumentNullAsync(() => Option.Some("a").RequireSome((RopError)null!));
        await AssertArgumentNullAsync(() => Option.None<string>().RequireSome((RopError)null!));
    }

    [Test]
    public async Task Aggregators_RejectNullArguments()
    {
        await AssertArgumentNullAsync(() => Validation.Zip(Result.Ok(1), Result.Ok(2), (Func<int, int, int>)null!));
        await AssertArgumentNullAsync(() => Result.Sequence<int, RopError>(null!));
        await AssertArgumentNullAsync(() => Validation.Sequence<int, RopError>(null!));
        await AssertArgumentNullAsync(() => Result.Traverse<int, int, RopError>(null!, value => Result.Ok(value)));
        await AssertArgumentNullAsync(() => Result.Traverse<int, int, RopError>([1], null!));
        await AssertArgumentNullAsync(() => Result.Partition<int, RopError>(null!));
    }

    [Test]
    public async Task OptionFactories_RejectNullArguments()
    {
        await AssertArgumentNullAsync(() => Option.When(true, (Func<int>)null!));
        await AssertArgumentNullAsync(() => Option.Combine(Option.Some(1), Option.Some(2), (Func<int, int, int>)null!));
        await AssertArgumentNullAsync(() => Option.Combine(Option.Some(1), Option.Some(2), Option.Some(3), (Func<int, int, int, int>)null!));
        await AssertArgumentNullAsync(() => Option.Sequence<int>(null!));
        await AssertArgumentNullAsync(() => Option.Traverse<int, int>(null!, value => Option.Some(value)));
        await AssertArgumentNullAsync(() => Option.Traverse<int, int>([1], null!));
        await AssertArgumentNullAsync(() => Option.Values<int>(null!));
    }

    [Test]
    public async Task OptionCombinators_RejectNullDelegates()
    {
        await AssertArgumentNullAsync(() => Option.Some(1).MapSome<int, int>(null!));
        await AssertArgumentNullAsync(() => Option.Some(1).TeeSome(null!));
        await AssertArgumentNullAsync(() => Option.Some(1).TeeNone(null!));
        await AssertArgumentNullAsync(() => Option.Some(1).Filter(null!));
        await AssertArgumentNullAsync(() => Option.Some(1).BindSome<int, int>(null!));
        await AssertArgumentNullAsync(() => Option.Some(1).BindNone(null!));
        await AssertArgumentNullAsync(() => Option.Some(1).BindNoneValue((Func<int>)null!));
        await AssertArgumentNullAsync(() => Option.Some(1).RequireSome((Func<RopError>)null!));
        await AssertArgumentNullAsync(() => Option.Some("a").BindSomeNullable<string, string>(null!));
        await AssertArgumentNullAsync(() => Option.Some(1).BindSomeNullableValue<int, int>(null!));
    }

    [Test]
    public async Task AsyncCombinators_RejectNullDelegates()
    {
        await AssertArgumentNullAsync(() => Task.FromResult(Ok).MapOkAsync<int, int, RopError>(null!));
        await AssertArgumentNullAsync(() => Task.FromResult(Ok).BindOkAsync<int, int, RopError>(null!));
        await AssertArgumentNullAsync(() => Ok.BindOkAsync<int, int, RopError>(null!));
        await AssertArgumentNullAsync(() => Ok.BindErrAsync<int, RopError>(null!));
        await AssertArgumentNullAsync(() => Task.FromResult(Option.Some(1)).MapSomeAsync<int, int>(null!));
        await AssertArgumentNullAsync(() => Task.FromResult(Option.Some(1)).BindSomeAsync<int, int>(null!));
    }

    [Test]
    public async Task RopError_WithMethodsRejectBlankValues()
    {
        var error = RopError.Create("E_A", "message");

        await Assert.That(TestSupport.Catch(() => error.WithCode(" ")) is ArgumentException).IsTrue();
        await Assert.That(TestSupport.Catch(() => error.WithMessage(string.Empty)) is ArgumentException).IsTrue();
        await Assert.That(TestSupport.Catch(() => error.WithDetails("  ")) is ArgumentException).IsTrue();
    }

    private static async Task AssertArgumentNullAsync(Action action)
    {
        var exception = TestSupport.Catch(action);

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    private static async Task AssertArgumentNullAsync(Func<Task> action)
    {
        var exception = await TestSupport.CatchAsync(action);

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }
}
