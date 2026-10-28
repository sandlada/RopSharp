using System.Collections;
using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Maintenance coverage: previously untested branches, single-enumeration guarantees, immutable materialization,
/// thread safety and the hardening fixes for diagnostics and error factories.
/// </summary>
public sealed class MaintenanceHardeningTests
{
    private static AppError App(string code = "E_TEST", string message = "failed") => new(code, message);

    // ---------------------------------------------------------------- equality and hashing branches

    [Test]
    public async Task Either_Swap_MovesTheLeftPayloadToTheRightBranch()
    {
        Either<string, int> left = Either.Left<string, int>("payload");

        var swapped = left.Swap();

        await Assert.That(swapped.IsRight).IsTrue();
        await Assert.That(swapped.TryGetRight(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("payload");
    }

    [Test]
    public async Task Either_Equals_ComparesTheLeftPayload()
    {
        Either<string, int> first = Either.Left<string, int>("payload");
        Either<string, int> same = Either.Left<string, int>("payload");
        Either<string, int> different = Either.Left<string, int>("other");

        await Assert.That(first.Equals(same)).IsTrue();
        await Assert.That(first.Equals(different)).IsFalse();
    }

    [Test]
    public async Task Either_Equals_ComparesTheRightPayload()
    {
        Either<string, int> first = Either.Right<string, int>(1);
        Either<string, int> same = Either.Right<string, int>(1);
        Either<string, int> different = Either.Right<string, int>(2);

        await Assert.That(first.Equals(same)).IsTrue();
        await Assert.That(first.Equals(different)).IsFalse();
    }

    [Test]
    public async Task Either_EqualsObject_ReturnsFalseForANonEither()
    {
        object boxed = Either.Right<string, int>(1);

        await Assert.That(boxed.Equals("not an either")).IsFalse();
        await Assert.That(boxed.Equals(null)).IsFalse();
    }

    [Test]
    public async Task Either_GetHashCode_IsStableForBothBranches()
    {
        var left = Either.Left<string, int>("payload");
        var sameLeft = Either.Left<string, int>("payload");
        var right = Either.Right<string, int>(1);
        var sameRight = Either.Right<string, int>(1);

        await Assert.That(left.GetHashCode()).IsEqualTo(sameLeft.GetHashCode());
        await Assert.That(right.GetHashCode()).IsEqualTo(sameRight.GetHashCode());
    }

    [Test]
    public async Task Result_GetHashCode_IsStableOnTheSuccessTrack()
    {
        var first = Rop.Ok<int, RopError>(7);
        var same = Rop.Ok<int, RopError>(7);
        var failed = Rop.Err<int, RopError>(Sample.Error());

        await Assert.That(first.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(first.Equals(failed)).IsFalse();
    }

    [Test]
    public async Task Result_GetHashCode_CoversTheDefaultErrorValue()
    {
        await Assert.That(default(Result<int, RopError>).GetHashCode())
            .IsEqualTo(default(Result<int, RopError>).GetHashCode());
    }

    [Test]
    public async Task Option_GetHashCode_TreatsNoneAndTheDefaultValueAlike()
    {
        await Assert.That(Opt.None<int>().GetHashCode()).IsEqualTo(default(Option<int>).GetHashCode());
        await Assert.That(Opt.Some(1).GetHashCode()).IsEqualTo(Opt.Some(1).GetHashCode());
    }

    [Test]
    public async Task Either_EqualsObject_ReturnsTrueForABoxedEqualEither()
    {
        object boxed = Either.Left<string, int>("payload");

        await Assert.That(boxed.Equals(Either.Left<string, int>("payload"))).IsTrue();
    }

    [Test]
    public async Task Either_GetHashCode_CoversTheDefaultRightValue()
    {
        var value = default(Either<int, string>);

        await Assert.That(value.GetHashCode()).IsEqualTo(default(Either<int, string>).GetHashCode());
        await Assert.That(value.IsRight).IsTrue();
    }

    [Test]
    public async Task Result_GetHashCode_CoversANullSuccessPayload()
    {
        var withNull = Rop.Ok<int, RopError>(1).MapOk<int, string?, RopError>(_ => null);
        var same = Rop.Ok<int, RopError>(1).MapOk<int, string?, RopError>(_ => null);

        await Assert.That(withNull.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(withNull.OrThrow()).IsNull();
    }

    // ---------------------------------------------------------------- conditional / ensure branches

    [Test]
    public async Task FailIf_WithFactory_DoesNotRunTheFactoryOnTheErrorTrack()
    {
        var called = false;
        Result<int, RopError> source = Rop.Err<int>(Sample.Error());

        var result = source.FailIf(_ => true, _ =>
        {
            called = true;
            return Sample.Error("E_SHOULD_NOT_RUN");
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_TEST");
    }

    [Test]
    public async Task FailIf_WithFactory_UsesTheFactoryWhenThePredicateHolds()
    {
        var result = Rop.Ok<int, RopError>(5)
            .FailIf(value => value > 0, value => RopError.Create("E_REJECT", $"rejected {value}"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_REJECT");
        await Assert.That(result.ErrOrThrow().Message).IsEqualTo("rejected 5");
    }

    [Test]
    public async Task RequireNotNull_WithCustomError_PreservesANonNullSuccess()
    {
        Result<string?, AppError> source =
            Rop.Ok<string, AppError>("x").MapOk<string, string?, AppError>(_ => "x");

        var result = source.RequireNotNull(() => App("E_NULL", "required"));

        await Assert.That(result.OrThrow()).IsEqualTo("x");
    }

    [Test]
    public async Task Validate_WithThreeInputs_ReturnsOkWhenEveryInputSucceeds()
    {
        var result = Rop.Validate(
            Rop.Ok<int, RopError>(1),
            Rop.Ok<int, RopError>(2),
            Rop.Ok<int, RopError>(3),
            (a, b, c) => a + b + c);

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task SuccessIf_WithRopErrorFactory_DoesNotRunTheFactoryOnSuccess()
    {
        var called = false;

        var result = Rop.SuccessIf(true, 1, () =>
        {
            called = true;
            return Sample.Error();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task SuccessIf_WithRopErrorFactory_UsesTheFactoryWhenTheConditionFails()
    {
        var result = Rop.SuccessIf(false, 1, () => Sample.Error("E_CONDITION"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_CONDITION");
    }

    [Test]
    public async Task FailureIf_WithRopErrorFactory_UsesTheFactoryWhenTheConditionHolds()
    {
        var result = Rop.FailureIf(true, 1, () => Sample.Error("E_REJECT"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_REJECT");
    }

    [Test]
    public async Task SuccessIf_WithCustomFactory_UsesTheFactoryWhenTheConditionFails()
    {
        var result = Rop.SuccessIf(false, 1, () => App("E_CONDITION", "not allowed"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_CONDITION");
    }

    [Test]
    public async Task FailureIf_WithRopErrorFactory_DoesNotRunTheFactoryWhenTheConditionFails()
    {
        var called = false;

        var result = Rop.FailureIf(false, 1, () =>
        {
            called = true;
            return Sample.Error();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    // ---------------------------------------------------------------- factory hardening

    [Test]
    public async Task RequireSome_WithNullProducingFactory_FaultsWithInvalidOperation()
    {
        var exception = TestSupport.Catch(() =>
            Opt.None<string>().RequireSome(() => (AppError)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task Either_ToResult_WithNullProducingFactory_FaultsWithInvalidOperation()
    {
        var exception = TestSupport.Catch(() =>
            Either.Left<AppError, int>(App()).ToResult(_ => (RopError)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapErr_WithNullProducingMapper_FaultsWithInvalidOperation()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Err<int, RopError>(Sample.Error()).MapErr<int, RopError, RopError>(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task Ensure_WithNullProducingFactory_FaultsWithInvalidOperation()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Ok<int, RopError>(1).Ensure(_ => false, _ => (RopError)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task FailIf_WithNullProducingFactory_FaultsWithInvalidOperation()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Ok<int, RopError>(1).FailIf(_ => true, _ => (RopError)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task ToSingleError_WithNullProducingCombine_FaultsWithInvalidOperation()
    {
        Result<int, IReadOnlyList<RopError>> source = Rop.Validate(
            Rop.Err<int>(Sample.Error("E_A")),
            Rop.Err<int>(Sample.Error("E_B")),
            (a, b) => a + b);

        var exception = TestSupport.Catch(() => source.ToSingleError<int, RopError>(_ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapErrAsync_WithNullProducingSynchronousMapper_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Err<int>(Sample.Error()))
                .MapErrAsync<int, RopError, RopError>(_ => (RopError)null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapErrAsync_WithNullProducingAsynchronousMapper_FaultsWithInvalidOperation()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Task.FromResult(Rop.Err<int>(Sample.Error()))
                .MapErrAsync<int, RopError, RopError>(_ => Task.FromResult<RopError>(null!)));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    // ---------------------------------------------------------------- parsing hardening

    [Test]
    public async Task TryParse_WhenTheParsableTypeReturnsNull_ReturnsNone()
    {
        var option = Rop.TryParse<NullParsable>("anything");

        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Parse_WhenTheParsableTypeReturnsNull_ReturnsAParseFailure()
    {
        var result = Rop.Parse<NullParsable, AppError>(
            "anything",
            failure => new AppError(failure.Code, failure.Message));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task Parse_WithRopError_WhenTheParsableTypeReturnsNull_ReturnsAParseFailure()
    {
        var result = Rop.Parse<NullParsable>("anything");

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_PARSE");
        await Assert.That(result.ErrOrThrow().Details).IsEqualTo("input=\"anything\"");
    }

    // ---------------------------------------------------------------- async partitioning

    [Test]
    public async Task PartitionAsync_WithANullTask_FaultsWithInvalidOperation()
    {
        Task<Result<int, RopError>>[] tasks = [null!];

        var exception = await TestSupport.CatchAsync(() => Rop.PartitionAsync<int, RopError>(tasks));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task PartitionAsync_PreservesOrderAndSplitsBothTracks()
    {
        Task<Result<int, RopError>>[] tasks =
        [
            Task.FromResult(Rop.Ok<int, RopError>(1)),
            Task.FromResult(Rop.Err<int, RopError>(Sample.Error("E_A"))),
            Task.FromResult(Rop.Ok<int, RopError>(3)),
            Task.FromResult(Rop.Err<int, RopError>(Sample.Error("E_B"))),
        ];

        var (oks, errs) = await Rop.PartitionAsync<int, RopError>(tasks);

        await Assert.That(oks.Count).IsEqualTo(2);
        await Assert.That(oks[0]).IsEqualTo(1);
        await Assert.That(oks[1]).IsEqualTo(3);
        await Assert.That(errs.Count).IsEqualTo(2);
        await Assert.That(errs[0].Code).IsEqualTo("E_A");
        await Assert.That(errs[1].Code).IsEqualTo("E_B");
    }

    [Test]
    public async Task PartitionAsync_WithAnEmptySequence_ReturnsEmptyLists()
    {
        var (oks, errs) = await Rop.PartitionAsync<int, RopError>(Array.Empty<Task<Result<int, RopError>>>());

        await Assert.That(oks.Count).IsEqualTo(0);
        await Assert.That(errs.Count).IsEqualTo(0);
    }

    // ---------------------------------------------------------------- aggregation complexity

    [Test]
    public async Task Combine_WithManyDuplicatedErrors_DeduplicatesInFirstSeenOrder()
    {
        var errors = new RopError[2_000];
        for (var index = 0; index < errors.Length; index++)
        {
            errors[index] = RopError.Create($"E_{index % 10}", $"message {index % 10}");
        }

        var combined = RopError.Combine(errors);

        await Assert.That(combined.Code).IsEqualTo("E_MULTIPLE");
        await Assert.That(combined.InnerErrors.Count).IsEqualTo(errors.Length);
        await Assert.That(combined.Details).IsEqualTo(string.Join(", ", Enumerable.Range(0, 10).Select(i => $"E_{i}")));
        await Assert.That(combined.Message).IsEqualTo(string.Join("; ", Enumerable.Range(0, 10).Select(i => $"message {i}")));
    }

    [Test]
    public async Task Combine_WithDistinctOrdinalCodes_KeepsBoth()
    {
        var combined = RopError.Combine([RopError.Create("E_A", "m"), RopError.Create("E_a", "m")]);

        await Assert.That(combined.Details).IsEqualTo("E_A, E_a");
    }

    // ---------------------------------------------------------------- single enumeration guarantees

    [Test]
    public async Task Sequence_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<Result<int, RopError>>(
        [
            Rop.Ok<int, RopError>(1),
            Rop.Ok<int, RopError>(2),
        ]);

        var result = Rop.Sequence(source);

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(result.OrThrow().Count).IsEqualTo(2);
    }

    [Test]
    public async Task Traverse_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<int>([1, 2, 3]);

        var result = Rop.Traverse<int, int, RopError>(source, value => Rop.Ok<int, RopError>(value * 2));

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(result.OrThrow().Count).IsEqualTo(3);
    }

    [Test]
    public async Task SequenceValidation_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<Result<int, RopError>>(
        [
            Rop.Err<int>(Sample.Error("E_A")),
            Rop.Ok<int, RopError>(2),
        ]);

        var result = Rop.SequenceValidation(source);

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(result.TryGetErr(out var errors)).IsTrue();
        await Assert.That(errors!.Count).IsEqualTo(1);
    }

    [Test]
    public async Task TraverseValidation_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<int>([1, 2, 3]);

        var result = Rop.TraverseValidation<int, int, RopError>(
            source,
            value => value == 2 ? Rop.Err<int>(Sample.Error("E_EVEN")) : Rop.Ok<int, RopError>(value));

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(result.IsErr).IsTrue();
    }

    [Test]
    public async Task Partition_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<Result<int, RopError>>(
        [
            Rop.Ok<int, RopError>(1),
            Rop.Err<int>(Sample.Error()),
        ]);

        var (oks, errs) = Rop.Partition(source);

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(oks.Count).IsEqualTo(1);
        await Assert.That(errs.Count).IsEqualTo(1);
    }

    [Test]
    public async Task OptionValues_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<Option<int>>([Opt.Some(1), Opt.None<int>(), Opt.Some(3)]);

        var values = Opt.Values(source);

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(values.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Choose_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<Option<int>>([Opt.Some(1), Opt.None<int>(), Opt.Some(3)]);

        var values = source.Choose(value => value * 10);

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(10);
        await Assert.That(values[1]).IsEqualTo(30);
    }

    [Test]
    public async Task PartitionAsync_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<Task<Result<int, RopError>>>(
        [
            Task.FromResult(Rop.Ok<int, RopError>(1)),
            Task.FromResult(Rop.Err<int>(Sample.Error())),
        ]);

        var (oks, errs) = await Rop.PartitionAsync<int, RopError>(source);

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(oks.Count).IsEqualTo(1);
        await Assert.That(errs.Count).IsEqualTo(1);
    }

    [Test]
    public async Task SequenceValidationAsync_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<Task<Result<int, RopError>>>(
        [
            Task.FromResult(Rop.Ok<int, RopError>(1)),
            Task.FromResult(Rop.Err<int>(Sample.Error())),
        ]);

        var result = await Rop.SequenceValidationAsync<int, RopError>(source);

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(result.IsErr).IsTrue();
    }

    [Test]
    public async Task TraverseValidationAsync_EnumeratesTheSourceExactlyOnce()
    {
        var source = new SinglePassEnumerable<int>([1, 2, 3]);

        var result = await Rop.TraverseValidationAsync<int, int, RopError>(
            source,
            value => Task.FromResult(value == 2
                ? Rop.Err<int>(Sample.Error("E_EVEN"))
                : Rop.Ok<int, RopError>(value)));

        await Assert.That(source.Enumerations).IsEqualTo(1);
        await Assert.That(result.IsErr).IsTrue();
    }

    // ---------------------------------------------------------------- immutability and thread safety

    [Test]
    public async Task MaterializedCollections_AreReadOnly()
    {
        var values = Rop.Sequence([Rop.Ok<int, RopError>(1), Rop.Ok<int, RopError>(2)]).OrThrow();
        var errors = Rop.Validate(
            Rop.Err<int>(Sample.Error("E_A")),
            Rop.Err<int>(Sample.Error("E_B")),
            (a, b) => a + b).ErrOrThrow();

        var valueMutation = TestSupport.Catch(() => ((IList<int>)values).Add(3));
        var errorMutation = TestSupport.Catch(() => ((IList<RopError>)errors).Add(Sample.Error()));

        await Assert.That(valueMutation is NotSupportedException).IsTrue();
        await Assert.That(errorMutation is NotSupportedException).IsTrue();
    }

    [Test]
    public async Task SharedImmutableValues_AreSafeForConcurrentReaders()
    {
        var shared = Rop.Ok<int, RopError>(42);
        var error = RopError.Create("E_SHARED", "shared");

        var outcomes = await Task.WhenAll(Enumerable.Range(0, 64).Select(offset => Task.Run(() =>
        {
            var mapped = shared.MapOk(value => value + offset);
            var combined = RopError.Combine([error, error]);
            return mapped.OrThrow() == 42 + offset
                && combined.InnerErrors.Count == 2
                && combined.Details == "E_SHARED";
        })));

        await Assert.That(outcomes.All(outcome => outcome)).IsTrue();
    }

    // ---------------------------------------------------------------- invalid default results

    [Test]
    public async Task DefaultResult_ErrorTrackAccessFailsFast()
    {
        var result = default(Result<int, RopError>);

        await Assert.That(() => result.Match(ok: value => value.ToString(), err: error => error.Code))
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.Switch(ok: _ => { }, err: _ => { }))
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.MapErr(error => error.WithCode("E_MAPPED")))
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.BindErr(_ => Rop.Ok<int, RopError>(1)))
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.BindErrValue(1))
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.TeeErr(_ => { }))
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.ErrOrThrow())
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.OrThrow())
            .Throws<InvalidOperationException>();
        await Assert.That(() => result.TryGetErr(out _))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ValidResults_AreUnaffectedByDefaultGuards()
    {
        var error = RopError.Create("E_VALID", "valid");
        var failed = Rop.Err<int, RopError>(error);
        var succeeded = Rop.Ok<int, RopError>(7);

        await Assert.That(failed.TryGetErr(out var actual)).IsTrue();
        await Assert.That(ReferenceEquals(actual, error)).IsTrue();
        await Assert.That(failed.ErrOrThrow().Code).IsEqualTo("E_VALID");
        await Assert.That(failed.Match(ok: value => value, err: e => e.Code == "E_VALID" ? 1 : 0))
            .IsEqualTo(1);
        await Assert.That(succeeded.TryGetErr(out _)).IsFalse();
        await Assert.That(succeeded.Match(ok: value => value, err: _ => -1)).IsEqualTo(7);
    }

    [Test]
    public async Task DefaultStructError_RemainsAValidValue()
    {
        var result = Rop.Err<int, StructError>(default);

        await Assert.That(result.TryGetErr(out var error)).IsTrue();
        await Assert.That(error.Status).IsEqualTo(0);
        await Assert.That(result.ErrOrThrow().Status).IsEqualTo(0);
        await Assert.That(result.Match(ok: value => value, err: e => e.Status)).IsEqualTo(0);
    }

    [Test]
    public async Task DefaultResult_WithNullableValueError_FailsFast()
    {
        var result = default(Result<int, int?>);

        await Assert.That(() => result.TryGetErr(out _)).Throws<InvalidOperationException>();
        await Assert.That(() => result.ErrOrThrow()).Throws<InvalidOperationException>();

        var valid = Rop.Err<int, int?>(5);
        await Assert.That(valid.TryGetErr(out var error)).IsTrue();
        await Assert.That(error).IsEqualTo(5);
    }

    [Test]
    public async Task DefaultResult_InArrayFailsFast()
    {
        var results = new Result<int, RopError>[1];

        await Assert.That(results[0].IsErr).IsTrue();
        await Assert.That(() => results[0].Match(ok: value => value.ToString(), err: error => error.Code))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task DefaultResult_WithCustomClassError_FailsFast()
    {
        var result = default(Result<int, AppError>);

        await Assert.That(() => result.TryGetErr(out _)).Throws<InvalidOperationException>();
        await Assert.That(() => result.ErrOrThrow()).Throws<InvalidOperationException>();
        await Assert.That(() => result.Match(ok: value => value.ToString(), err: error => error.Code))
            .Throws<InvalidOperationException>();
    }

    // ---------------------------------------------------------------- documented default values

    [Test]
    public async Task DefaultValues_FollowTheirDocumentedContracts()
    {
        await Assert.That(default(Option<int>).IsNone).IsTrue();
        await Assert.That(default(Option<int>).Equals(Opt.None<int>())).IsTrue();
        await Assert.That(default(Result<int, RopError>).IsErr).IsTrue();
        await Assert.That(default(Result<int, RopError>).Equals(default)).IsTrue();
        await Assert.That(default(Either<int, string>).IsRight).IsTrue();
        await Assert.That(default(ParseFailure).Input).IsNull();
    }
}

/// <summary>An enumerable that throws if it is enumerated more than once.</summary>
internal sealed class SinglePassEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _source;
    private bool _consumed;

    internal SinglePassEnumerable(IEnumerable<T> source) => _source = source;

    internal int Enumerations { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        if (_consumed)
        {
            throw new InvalidOperationException("The source was enumerated more than once.");
        }

        _consumed = true;
        Enumerations++;
        return _source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>An <see cref="IParsable{T}"/> implementation that reports success without producing a value.</summary>
internal sealed class NullParsable : IParsable<NullParsable>
{
    public static NullParsable Parse(string s, IFormatProvider? provider) => new();

    public static bool TryParse(string? s, IFormatProvider? provider, out NullParsable result)
    {
        result = null!;
        return true;
    }
}
