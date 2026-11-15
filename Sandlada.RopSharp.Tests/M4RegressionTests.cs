using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class M4RegressionTests
{
    [Test]
    public async Task DEF01_ValidationFrom_NullableOkResult_PreservesPassThroughPayload()
    {
        var nullOk = Result.Ok("x").MapOk<string, string?, RopError>(static _ => null);
        var lifted = Validation.From(nullOk);

        await Assert.That(lifted.IsSuccess).IsTrue();
        await Assert.That(lifted.Expect("test")).IsNull();
    }

    [Test]
    public async Task DEF02_LazyFactoriesAndMappersReturningNull_ThrowInvalidOperationException()
    {
        await Assert.That(TestSupport.Catch(() => Option.None<string>().BindNoneValue(static () => null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Option.When<string>(true, static () => null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Option.Combine(Option.Some("a"), Option.Some("b"), static (string _, string _) => (string)null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Option.Combine(Option.Some("a"), Option.Some("b"), Option.Some("c"), static (string _, string _, string _) => (string)null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Option.Some("a").MapSome<string, string>(static _ => null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Result.Err<string>(Sample.Error()).BindErrValue(static () => null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Validation.Success<string, RopError>("a").Map<string>(static _ => null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Validation.Zip(Result.Ok("a"), Result.Ok("b"), static (string _, string _) => (string)null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Validation.Zip(Result.Ok("a"), Result.Ok("b"), Result.Ok("c"), static (string _, string _, string _) => (string)null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Either.Left<string, int>("a").MapLeft<string>(static _ => null!)) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Either.Right<string, string>("b").MapRight<string>(static _ => null!)) is InvalidOperationException).IsTrue();

        var optionTaskBindNull = await CatchAsync(async () =>
            _ = await OptionTask.From(Option.None<string>()).BindNoneValue(static () => null!));
        await Assert.That(optionTaskBindNull is InvalidOperationException).IsTrue();

        var optionTaskMapNull = await CatchAsync(async () =>
            _ = await OptionTask.From(Option.Some("a")).MapSome<string>(static _ => null!));
        await Assert.That(optionTaskMapNull is InvalidOperationException).IsTrue();

        var optionTaskMapAsyncNull = await CatchAsync(async () =>
            _ = await OptionTask.From(Option.Some("a")).MapSomeAsync<string>(static (_, _) => Task.FromResult<string>(null!)));
        await Assert.That(optionTaskMapAsyncNull is InvalidOperationException).IsTrue();

        var validationTaskMapAsyncNull = await CatchAsync(async () =>
            _ = await ValidationTask.From(Validation.Success<string, RopError>("a"))
                .MapAsync<string>(static (_, _) => Task.FromResult<string>(null!)));
        await Assert.That(validationTaskMapAsyncNull is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task DEF03_OptionTask_BindNoneValue_NullFallback_ThrowsArgumentNullExceptionEagerly()
    {
        await Assert.That(TestSupport.Catch(() => OptionTask.From(Option.Some("a")).BindNoneValue((string)null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => OptionTask.From(Option.None<string>()).BindNoneValue((string)null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task DEF04_LinqSelectMany_ThreeArg_NullSelectors_ThrowArgumentNullExceptionOnFailureTracks()
    {
        var errResult = Result.Err<int>(Sample.Error());
        var errResultTask = ResultTask.From(errResult);
        var noneOption = Option.None<int>();
        var noneOptionTask = OptionTask.From(noneOption);

        await Assert.That(TestSupport.Catch(() => errResult.SelectMany<int, int, int, RopError>(null!, (_, b) => b)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => errResult.SelectMany<int, int, int, RopError>(_ => Result.Ok(1), null!)) is ArgumentNullException).IsTrue();

        await Assert.That(TestSupport.Catch(() => errResultTask.SelectMany<int, int, int, RopError>((Func<int, Result<int, RopError>>)null!, (_, b) => b)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => errResultTask.SelectMany<int, int, int, RopError>(_ => Result.Ok(1), null!)) is ArgumentNullException).IsTrue();

        await Assert.That(TestSupport.Catch(() => noneOption.SelectMany<int, int, int>(null!, (_, b) => b)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => noneOption.SelectMany<int, int, int>(_ => Option.Some(1), null!)) is ArgumentNullException).IsTrue();

        await Assert.That(TestSupport.Catch(() => noneOptionTask.SelectMany<int, int, int>((Func<int, Option<int>>)null!, (_, b) => b)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => noneOptionTask.SelectMany<int, int, int>(_ => Option.Some(1), null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task DEF05_NullDelegateGuards_RunBeforeUninitializedCheck()
    {
        ResultTask<int, RopError> uninitRt = default;
        Result<int, RopError> uninitR = default;
        Validation<int, RopError> uninitV = default;

        await Assert.That(TestSupport.Catch(() => uninitRt.SwitchAsync(null!, (_, _) => Task.CompletedTask)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => uninitRt.SwitchAsync((_, _) => Task.CompletedTask, null!)) is ArgumentNullException).IsTrue();

        await Assert.That(TestSupport.Catch(() => uninitR.MapOkAsync<int, int, RopError>(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => uninitR.MapErrAsync<int, RopError, RopError>(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => uninitR.BindOkAsync<int, int, RopError>(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => uninitR.BindErrAsync<int, RopError>(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => uninitR.TeeOkAsync(null!)) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => uninitR.TeeErrAsync(null!)) is ArgumentNullException).IsTrue();

        await Assert.That(TestSupport.Catch(() => uninitV.MapAsync<int, int, RopError>(null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task DEF07_RopError_SharedDiamondDag_PreservesStructuralEqualityAndHashCode()
    {
        var leaf = RopError.Create("E_LEAF", "leaf");
        var midA = RopError.Combine([leaf, RopError.Create("E_B", "b")]);
        var root1 = RopError.Combine([midA, midA]);
        var root2 = RopError.Combine([
            RopError.Combine([RopError.Create("E_LEAF", "leaf"), RopError.Create("E_B", "b")]),
            RopError.Combine([RopError.Create("E_LEAF", "leaf"), RopError.Create("E_B", "b")]),
        ]);

        await Assert.That(root1.Equals(root2)).IsTrue();
        await Assert.That(root1.GetHashCode()).IsEqualTo(root2.GetHashCode());
        await Assert.That(root1.Flatten().Count).IsEqualTo(root2.Flatten().Count);
    }

    [Test]
    public async Task DEF11Sync_TaskLikeGuard_RunsBeforeBodyAndOnAllSyncCombinators()
    {
        var invoked = false;
        Func<Task<int>> taskFunc = () =>
        {
            invoked = true;
            return Task.FromResult(1);
        };
        var tryEx = TestSupport.Catch(() => Result.Try(taskFunc, _ => Sample.Error()));
        await Assert.That(tryEx is InvalidOperationException).IsTrue();
        await Assert.That(invoked).IsFalse();

        var tryErrEx = TestSupport.Catch(() => Result.Try<Task<int>, RopError>(
            () =>
            {
                invoked = true;
                return Task.FromResult(1);
            },
            _ => Sample.Error(),
            () => Sample.Error()));
        await Assert.That(tryErrEx is InvalidOperationException).IsTrue();
        await Assert.That(invoked).IsFalse();

        await Assert.That(TestSupport.Catch(() => Result.Switch<int, Task<int>, RopError>(static x => Task.FromResult(x))) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Either.Right<string, int>(1).MapLeft(static _ => Task.FromResult(1))) is InvalidOperationException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Either.Left<string, int>("a").MapRight(static _ => Task.FromResult(1))) is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task DEF16_ValidationMap_TaskLikeMessage_NamesMap()
    {
        var ex = TestSupport.Catch(() => Validation.Success<int, RopError>(1).Map(static x => Task.FromResult(x)));

        await Assert.That(ex is InvalidOperationException).IsTrue();
        await Assert.That(ex!.Message.Contains("Map", StringComparison.Ordinal)).IsTrue();
    }

    private static async Task<Exception?> CatchAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
