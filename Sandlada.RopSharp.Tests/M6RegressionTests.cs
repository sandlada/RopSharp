namespace Sandlada.RopSharp.Tests;

public sealed class M6RegressionTests
{
    [Test]
    public async Task GenericParameterOrder_TerrIsLast_OnMatchAndMapAsync()
    {
        var ok = Result.Ok<int, string>(7).Match<int, string, string>(
            ok: static n => $"ok:{n}",
            err: static e => $"err:{e}");
        await Assert.That(ok).IsEqualTo("ok:7");

        var mapped = await Validation.Success<int, string>(7)
            .MapAsync<int, string, string>(static (n, _) => Task.FromResult($"v:{n}"));
        await Assert.That(mapped.Expect("test")).IsEqualTo("v:7");
    }

    [Test]
    public async Task OptionValuesAndChoose_BothProducePresentValues()
    {
        Option<int>[] items = [Option.Some(1), Option.None<int>(), Option.Some(3)];
        var values = Option.Values(items);
        var chosen = items.Choose();

        await Assert.That(values.Count).IsEqualTo(2);
        await Assert.That(values.SequenceEqual(chosen)).IsTrue();
    }

    [Test]
    public async Task OptionExtensionsSwitch_RunsMatchingBranch()
    {
        var hitSome = 0;
        var hitNone = 0;

        Option.Some(5).Switch(n => hitSome = n, () => hitNone++);
        Option.None<int>().Switch(n => hitSome = n, () => hitNone++);

        await Assert.That(hitSome).IsEqualTo(5);
        await Assert.That(hitNone).IsEqualTo(1);
    }

    [Test]
    public async Task OptionTask_ParityCombinatorsAndTerminals_WorkEndToEnd()
    {
        var matchedSome = await OptionTask.From(Option.Some(10))
            .MapSomeValue("ten")
            .MatchAsync(
                some: static (s, _) => Task.FromResult(s),
                none: static _ => Task.FromResult("none"));
        await Assert.That(matchedSome).IsEqualTo("ten");

        var matchedNone = await OptionTask.From(Option.None<int>())
            .BindNoneValueAsync(static _ => Task.FromResult(99))
            .MatchAsync(
                some: static (n, _) => Task.FromResult(n),
                none: static _ => Task.FromResult(-1));
        await Assert.That(matchedNone).IsEqualTo(99);

        var switchHit = "";
        await OptionTask.From(Option.None<string>()).SwitchAsync(
            some: (s, _) => { switchHit = s; return Task.CompletedTask; },
            none: _ => { switchHit = "empty"; return Task.CompletedTask; });
        await Assert.That(switchHit).IsEqualTo("empty");

        var expected = await OptionTask.From(Option.Some("ok")).ExpectAsync("present");
        await Assert.That(expected).IsEqualTo("ok");

        var missingEx = await TestSupport.CatchAsync(() => OptionTask.From(Option.None<string>()).ExpectAsync("must exist"));
        await Assert.That(missingEx is InvalidOperationException).IsTrue();

        var nullFallbackEx = await TestSupport.CatchAsync(async () =>
            _ = await OptionTask.From(Option.None<string>())
                .BindNoneValueAsync(static _ => Task.FromResult<string>(null!)));
        await Assert.That(nullFallbackEx is InvalidOperationException).IsTrue();

        var flattened = await OptionTask.From(Option.Some(Option.Some(42))).Flatten();
        await Assert.That(flattened.Expect("inner present")).IsEqualTo(42);
    }

    [Test]
    public async Task ResultExtensions_RequireNotNullValue_HandlesBothTracks()
    {
        int? someVal = 12;
        int? noneVal = null;

        var ok = Result.Ok<int?, RopError>(someVal).RequireNotNullValue(() => RopError.Create("E_NULL", "missing"));
        var err = Result.Ok(1).MapOk(_ => noneVal).RequireNotNullValue(() => RopError.Create("E_NULL", "missing"));
        var customErr = Result.Ok<int, string>(1).MapOk(_ => noneVal).RequireNotNullValue(() => "missing-custom");

        await Assert.That(ok.Expect("present")).IsEqualTo(12);
        await Assert.That(err.IsErr && err.TryGetErr(out var e) && e.Code == "E_NULL").IsTrue();
        await Assert.That(customErr.IsErr && customErr.TryGetErr(out var ce) && ce == "missing-custom").IsTrue();
    }

    [Test]
    public async Task ResultTask_ParityCombinatorsAndTerminals_WorkEndToEnd()
    {
        var ensureOk = await ResultTask.From(Result.Ok<int, string>(10))
            .Ensure(static n => n > 0, "non-positive")
            .EnsureAsync(static (n, _) => Task.FromResult(n < 100), static (n, _) => Task.FromResult($"too-big:{n}"))
            .FailIf(static n => n == 5, "is-five")
            .FailIfAsync(static (n, _) => Task.FromResult(n == 99), static (_, _) => Task.FromResult("is-99"))
            .ThenEnsure(static n => n == 10 ? Result.Ok<Unit, string>(Unit.Value) : Result.Err<Unit, string>("not-ten"))
            .ThenEnsureAsync(static (_, _) => Task.FromResult(Result.Ok<Unit, string>(Unit.Value)))
            .MapOkValue("done")
            .ExpectAsync("pipeline ok");
        await Assert.That(ensureOk).IsEqualTo("done");

        var ensureFailed = await ResultTask.From(Result.Ok<int, string>(10))
            .EnsureAsync(static (_, _) => Task.FromResult(false), static (n, _) => Task.FromResult($"bad:{n}"))
            .BindErrValueAsync(static _ => Task.FromResult(77))
            .MatchAsync(
                ok: static (n, _) => Task.FromResult($"ok:{n}"),
                err: static (e, _) => Task.FromResult($"err:{e}"));
        await Assert.That(ensureFailed).IsEqualTo("ok:77");

        var recoveredSync = await ResultTask.From(Result.Err<int, string>("boom"))
            .BindErrValue(5);
        await Assert.That(recoveredSync.Expect("recovered")).IsEqualTo(5);

        int? nullableInt = 3;
        var nonNullVal = await ResultTask.From(Result.Ok<int?, string>(nullableInt))
            .RequireNotNullValue(static () => "null-int");
        await Assert.That(nonNullVal.Expect("value")).IsEqualTo(3);

        var nonNullRef = await ResultTask.From(Result.Ok<string?, string>("abc"))
            .RequireNotNull(static () => "null-str");
        await Assert.That(nonNullRef.Expect("value")).IsEqualTo("abc");

        var flattened = await ResultTask.From(Result.Ok<Result<int, string>, string>(Result.Ok<int, string>(8))).Flatten();
        await Assert.That(flattened.Expect("inner")).IsEqualTo(8);
    }

    [Test]
    public async Task ValidationAndValidationTask_SelfAggregationAndTerminals_AccumulateAllErrors()
    {
        var rTuple2 = Validation.Zip(Result.Ok<int, string>(1), Result.Ok<string, string>("a"));
        var rTuple3 = Validation.Zip(Result.Ok<int, string>(1), Result.Ok<string, string>("a"), Result.Ok<bool, string>(true));
        await Assert.That(rTuple2.Expect("ok")).IsEqualTo((1, "a"));
        await Assert.That(rTuple3.Expect("ok")).IsEqualTo((1, "a", true));

        var vZipFail = Validation.Zip(
            Validation.Failure<int, string>("e1"),
            Validation.Failure<int, string>("e2"));
        await Assert.That(vZipFail.TryGetErrors(out var errs2) && errs2.SequenceEqual(["e1", "e2"])).IsTrue();

        var vSeq = Validation.Sequence([
            Validation.Failure<int, string>(["a", "b"]),
            Validation.Success<int, string>(3),
            Validation.Failure<int, string>("c"),
        ]);
        await Assert.That(vSeq.TryGetErrors(out var seqErrs) && seqErrs.SequenceEqual(["a", "b", "c"])).IsTrue();

        var vTrav = Validation.Traverse([1, 2], static n => Validation.Success<int, string>(n * 2));
        await Assert.That(vTrav.Expect("ok").SequenceEqual([2, 4])).IsTrue();

        var vt = ValidationTask.Zip(
            ValidationTask.From(Validation.Failure<int, string>("e1")),
            ValidationTask.From(Validation.Failure<int, string>("e2")),
            ValidationTask.From(Validation.Failure<int, string>("e3")));
        var collapsed = await vt.Collapse(static list => string.Join(",", list));
        await Assert.That(collapsed.TryGetErr(out var merged) && merged == "e1,e2,e3").IsTrue();

        var firstLabel = await vt.MatchFirstAsync(
            ok: static (_, _) => Task.FromResult("ok"),
            err: static (e, _) => Task.FromResult(e));
        await Assert.That(firstLabel).IsEqualTo("e1");

        var allLabel = await vt.MatchAsync(
            ok: static (_, _) => Task.FromResult("ok"),
            errors: static (list, _) => Task.FromResult($"{list.Count}"));
        await Assert.That(allLabel).IsEqualTo("3");

        var sw = "";
        await vt.SwitchAsync(
            ok: (_, _) => Task.CompletedTask,
            errors: (list, _) => { sw = $"all:{list.Count}"; return Task.CompletedTask; });
        await Assert.That(sw).IsEqualTo("all:3");

        await vt.SwitchFirstAsync(
            ok: (_, _) => Task.CompletedTask,
            err: (e, _) => { sw = $"first:{e}"; return Task.CompletedTask; });
        await Assert.That(sw).IsEqualTo("first:e1");

        var okVal = await ValidationTask.From(Validation.Success<int, string>(55)).ExpectAsync("ok");
        await Assert.That(okVal).IsEqualTo(55);

        var vtSeq = await ValidationTask.Sequence([
            ValidationTask.From(Validation.Failure<int, string>("x")),
            ValidationTask.From(Validation.Failure<int, string>("y")),
        ]);
        await Assert.That(vtSeq.TryGetErrors(out var vtErrs) && vtErrs.SequenceEqual(["x", "y"])).IsTrue();

        var vtTrav = await ValidationTask.Traverse([1, 2], static (n, _) => Task.FromResult(Validation.Success<int, string>(n + 1)));
        await Assert.That(vtTrav.Expect("ok").SequenceEqual([2, 3])).IsTrue();
    }

    [Test]
    public async Task TryAsyncAndSwitchAsync_CancellationTokenDelegateOverloads_ForwardToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken observed = default;

        var res = await Result.TryAsync(
            async ct =>
            {
                observed = ct;
                await Task.Yield();
                return 123;
            },
            "E_TEST",
            cts.Token);

        await Assert.That(res.Expect("ok")).IsEqualTo(123);
        await Assert.That(observed).IsEqualTo(cts.Token);

        CancellationToken switchObserved = default;
        var lifted = Result.TrySwitchAsync<int, int>(
            (n, ct) =>
            {
                switchObserved = ct;
                return Task.FromResult(n * 2);
            },
            static ex => RopError.FromException(ex, "E_SW"),
            cts.Token);

        var swRes = await lifted(6);
        await Assert.That(swRes.Expect("ok")).IsEqualTo(12);
        await Assert.That(switchObserved).IsEqualTo(cts.Token);
    }
}
