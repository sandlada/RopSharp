using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class ResultAggregationTests
{
    [Test]
    public async Task Combine2_WithBothSuccess_Merges()
    {
        var result = Result.Combine(Result.Ok(2), Result.Ok(3), (int a, int b) => a * b);

        await Assert.That(result.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task Combine2_WithFirstError_ReturnsFirstError()
    {
        var first = Sample.Error("E_FIRST");
        var mergerCalled = false;

        var result = Result.Combine(
            Result.Err<int>(first),
            Result.Err<int>(Sample.Error("E_SECOND")),
            (int a, int b) =>
            {
                mergerCalled = true;
                return a + b;
            });

        await Assert.That(mergerCalled).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(first);
    }

    [Test]
    public async Task Combine2_WithSecondError_ReturnsSecondError()
    {
        var second = Sample.Error("E_SECOND");

        var result = Result.Combine(Result.Ok(1), Result.Err<int>(second), (int a, int b) => a + b);

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(second);
    }

    [Test]
    public async Task Combine3_WithAllSuccess_Merges()
    {
        var result = Result.Combine(
            Result.Ok(1),
            Result.Ok(2),
            Result.Ok(3),
            (int a, int b, int c) => a + b + c);

        await Assert.That(result.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task Combine3_WithThirdError_ReturnsIt()
    {
        var third = Sample.Error("E_THIRD");

        var result = Result.Combine(
            Result.Ok(1),
            Result.Ok(2),
            Result.Err<int>(third),
            (int a, int b, int c) => a + b + c);

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(third);
    }

    [Test]
    public async Task Zip2_PairsValues()
    {
        var result = Result.Zip(Result.Ok(1), Result.Ok("a"));

        var pair = result.OrThrow();

        await Assert.That(pair.Item1).IsEqualTo(1);
        await Assert.That(pair.Item2).IsEqualTo("a");
    }

    [Test]
    public async Task Zip2_PropagatesFirstError()
    {
        var error = Sample.Error();

        var result = Result.Zip(Result.Err<int>(error), Result.Ok("a"));

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Zip3_PairsValues()
    {
        var triple = Result.Zip(Result.Ok(1), Result.Ok("a"), Result.Ok(true)).OrThrow();

        await Assert.That(triple.Item1).IsEqualTo(1);
        await Assert.That(triple.Item2).IsEqualTo("a");
        await Assert.That(triple.Item3).IsTrue();
    }

    [Test]
    public async Task Validate2_WithAllSuccess_Merges()
    {
        var result = Result.Validate(Result.Ok(2), Result.Ok(3), (int a, int b) => a * b);

        await Assert.That(result.OrThrow()).IsEqualTo(6);
    }

    [Test]
    public async Task Validate2_CollectsEveryErrorInOrder()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");

        var result = Result.Validate(
            Result.Err<int>(first),
            Result.Err<int>(second),
            (int a, int b) => a + b);

        await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), first, second);
    }

    [Test]
    public async Task Validate3_CollectsEveryErrorIncludingMixedTracks()
    {
        var first = Sample.Error("E_FIRST");
        var third = Sample.Error("E_THIRD");

        var result = Result.Validate(
            Result.Err<int>(first),
            Result.Ok(2),
            Result.Err<int>(third),
            (int a, int b, int c) => a + b + c);

        var errors = result.ErrOrThrow();

        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors[0]).IsSameReferenceAs(first);
        await Assert.That(errors[1]).IsSameReferenceAs(third);
    }

    [Test]
    public async Task Validate2_ErrorListIsNeverEmpty()
    {
        var result = Result.Validate(
            Result.Err<int>(Sample.Error()),
            Result.Ok(2),
            (int a, int b) => a + b);

        await Assert.That(result.ErrOrThrow().Count).IsGreaterThan(0);
    }

    [Test]
    public async Task Sequence_WithEmptySource_ReturnsEmptyOk()
    {
        var result = Result.Sequence(Array.Empty<Result<int, RopError>>());

        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task Sequence_WithAllSuccess_PreservesOrder()
    {
        var result = Result.Sequence(new[] { Result.Ok(1), Result.Ok(2), Result.Ok(3) });

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 1, 2, 3);
    }

    [Test]
    public async Task Sequence_WithError_ShortCircuits()
    {
        var error = Sample.Error("E_X");
        var source = new CountingSequence<Result<int, RopError>>(
        [
            Result.Ok(1),
            Result.Err<int>(error),
            Result.Ok(3),
        ]);

        var result = Result.Sequence(source);

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
        await Assert.That(source.GetEnumeratorCalls).IsEqualTo(1);
        await Assert.That(source.ItemsYielded).IsEqualTo(2);
    }

    [Test]
    public async Task SequenceValidation_CollectsAllErrorsAndKeepsSuccesses()
    {
        var first = Sample.Error("E_FIRST");
        var second = Sample.Error("E_SECOND");
        var source = new CountingSequence<Result<int, RopError>>(
        [
            Result.Ok(1),
            Result.Err<int>(first),
            Result.Err<int>(second),
        ]);

        var result = Result.SequenceValidation(source);

        await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), first, second);
        await Assert.That(source.GetEnumeratorCalls).IsEqualTo(1);
        await Assert.That(source.ItemsYielded).IsEqualTo(3);
    }

    [Test]
    public async Task Traverse_MapsAndShortCircuits()
    {
        var error = Sample.Error("E_ODD");

        var result = Result.Traverse([1, 2, 3, 4], value =>
            value % 2 == 0 ? Result.Ok(value * 10) : Result.Err<int>(error));

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Traverse_WithAllSuccess_PreservesOrder()
    {
        var result = Result.Traverse([1, 2, 3], value => Result.Ok(value * 2));

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 2, 4, 6);
    }

    [Test]
    public async Task Partition_SplitsWithoutFailing()
    {
        var error = Sample.Error();

        var (oks, errs) = Result.Partition(new[]
        {
            Result.Ok(1),
            Result.Err<int>(error),
            Result.Ok(2),
        });

        await TestSupport.AssertSequenceAsync(oks, 1, 2);
        await Assert.That(errs.Count).IsEqualTo(1);
        await Assert.That(errs[0]).IsSameReferenceAs(error);
    }

    [Test]
    public async Task ToValidation_LiftsSuccessAndWrapsSingleError()
    {
        var ok = Result.Ok(1).ToValidation();
        var error = Sample.Error();

        var err = Result.Err<int>(error).ToValidation();

        await Assert.That(ok.OrThrow()).IsEqualTo(1);
        await TestSupport.AssertSequenceAsync(err.ErrOrThrow(), error);
    }

    [Test]
    public async Task ToSingleError_KeepsFirstError()
    {
        var first = Sample.Error("E_FIRST");

        var result = Result.Err<int, IReadOnlyList<RopError>>([first, Sample.Error("E_SECOND")])
            .ToSingleError();

        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(first);
    }

    [Test]
    public async Task ToSingleError_WithStrategy_Combines()
    {
        var result = Result.Err<int, IReadOnlyList<RopError>>(
        [
            Sample.Error("E_FIRST", "one"),
            Sample.Error("E_SECOND", "two"),
        ]).ToSingleError(RopError.Combine);

        var combined = result.ErrOrThrow();

        await Assert.That(combined.Code).IsEqualTo("E_MULTIPLE");
    }

    [Test]
    public async Task ToSingleError_KeepsSuccess()
    {
        var result = Result.Ok<int, IReadOnlyList<RopError>>(5).ToSingleError();

        await Assert.That(result.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task ToSingleError_WithEmptyErrorList_Throws()
    {
        var invalid = Result.Err<int, IReadOnlyList<RopError>>([]);

        var exception = TestSupport.Catch(() => invalid.ToSingleError());

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task Combine_WithNullMerger_Throws()
    {
        var exception = TestSupport.Catch(() =>
            Result.Combine(Result.Ok(1), Result.Ok(2), (Func<int, int, int>)null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }
}
