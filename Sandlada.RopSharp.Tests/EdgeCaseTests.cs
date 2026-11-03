using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class EdgeCaseTests
{
    [Test]
    public async Task SequenceValidation_WithEmptySource_ReturnsEmptyOk()
    {
        var result = Validation.Sequence(Array.Empty<Result<int, RopError>>());

        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceValidation_WithAllSuccess_KeepsValues()
    {
        var result = Validation.Sequence(new[] { Result.Ok(1), Result.Ok(2) });

        await TestSupport.AssertSequenceAsync(result.Expect("test"), 1, 2);
    }

    [Test]
    public async Task Traverse_WithEmptySource_ReturnsEmptyOk()
    {
        var result = Result.Traverse(Array.Empty<int>(), value => Result.Ok(value));

        await Assert.That(result.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task Partition_WithEmptySource_ReturnsEmptyLists()
    {
        var (oks, errs) = Result.Partition(Array.Empty<Result<int, RopError>>());

        await Assert.That(oks.Count).IsEqualTo(0);
        await Assert.That(errs.Count).IsEqualTo(0);
    }

    [Test]
    public async Task OptTraverse_WithEmptySource_ReturnsEmptySome()
    {
        var option = Option.Traverse(Array.Empty<int>(), value => Option.Some(value));

        await Assert.That(option.Expect("test").Count).IsEqualTo(0);
    }

    [Test]
    public async Task Combine3_DoesNotInvokeMergerWhenAnyInputFails()
    {
        var called = false;
        var error = Sample.Error("E_X");

        var result = Result.Combine(
            Result.Ok(1),
            Result.Err<int>(error),
            Result.Ok(3),
            (int a, int b, int c) =>
            {
                called = true;
                return a + b + c;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ExpectErr()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Validate_DoesNotInvokeMergerWhenAnyInputFails()
    {
        var called = false;

        var result = Validation.Zip(
            Result.Err<int>(Sample.Error()),
            Result.Ok(2),
            (int a, int b) =>
            {
                called = true;
                return a + b;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(result.IsFailure).IsTrue();
    }

    [Test]
    public async Task ToSingleError_WithStrategy_ThrowsOnEmptyList()
    {
        var exception = TestSupport.Catch(() => Validation.Failure<int, RopError>([]));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task MapErrAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();

        var result = await ResultTask.From(Result.Err<int>(Sample.Error("E_X")))
            .WithCancellation(cts.Token)
            .MapErrAsync((error, token) => Task.FromResult(error.WithCode(token == cts.Token ? "E_CT" : "E_NO")));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_CT");
    }

    [Test]
    public async Task TeeOkAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await ResultTask.From(Result.Ok(1)).WithCancellation(cts.Token).TeeOkAsync((value, token) =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TeeErrAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await ResultTask.From(Result.Err<int>(Sample.Error())).WithCancellation(cts.Token).TeeErrAsync((error, token) =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TeeSomeAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await OptionTask.From(Task.FromResult(Option.Some(1))).WithCancellation(cts.Token).TeeSomeAsync((value, token) =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await OptionTask.From(Task.FromResult(Option.None<int>())).WithCancellation(cts.Token).TeeNoneAsync(token =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        });

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task OptionSequence_EnumeratesSourceOnce()
    {
        var source = new CountingSequence<Option<int>>([Option.Some(1), Option.Some(2)]);

        var option = Option.Sequence(source);

        await Assert.That(option.Expect("test").Count).IsEqualTo(2);
        await Assert.That(source.GetEnumeratorCalls).IsEqualTo(1);
    }

    [Test]
    public async Task EmptyAggregations_NeverReturnNullLists()
    {
        var sequence = Result.Sequence(Array.Empty<Result<int, RopError>>()).Expect("test");
        var validation = Validation.Sequence(Array.Empty<Result<int, RopError>>()).Expect("test");
        var (oks, errs) = Result.Partition(Array.Empty<Result<int, RopError>>());

        await Assert.That(sequence).IsNotNull();
        await Assert.That(validation).IsNotNull();
        await Assert.That(oks).IsNotNull();
        await Assert.That(errs).IsNotNull();
    }

    [Test]
    public async Task ExposedAggregationLists_AreReadOnlyViews()
    {
        var sequence = Result.Sequence(new[] { Result.Ok(1), Result.Ok(2) }).Expect("test");
        var validation = Validation.Sequence(new[] { Result.Ok(1), Result.Ok(2) }).Expect("test");
        var (oks, errs) = Result.Partition(new[] { Result.Ok(1), Result.Err<int>(Sample.Error()) });

        foreach (var list in new IReadOnlyList<int>[] { sequence, validation, oks })
        {
            await Assert.That(list is IList<int>).IsTrue();
            await Assert.That(() => ((IList<int>)list).Add(99)).Throws<NotSupportedException>();
        }

        await Assert.That(errs is IList<RopError>).IsTrue();
        await Assert.That(() => ((IList<RopError>)errs).Clear()).Throws<NotSupportedException>();
    }

    [Test]
    public async Task Result_PayloadEquality_WithRecords()
    {
        var left = Result.Ok(new Sample.Order("o", 10m));
        var right = Result.Ok(new Sample.Order("o", 10m));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task Option_PayloadEquality_WithRecords()
    {
        var left = Option.Some(new Sample.Order("o", 10m));
        var right = Option.Some(new Sample.Order("o", 10m));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task TeeOk_ActionExceptionPropagates()
    {
        var exception = TestSupport.Catch(() => Result.Ok(1).TeeOk(_ => throw new InvalidOperationException("bug")));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapOk_MapperExceptionPropagates()
    {
        var exception = TestSupport.Catch(() => Result.Ok(1).MapOk<int, int, RopError>(_ => throw new InvalidOperationException("bug")));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapOk_OperationCanceledExceptionPropagates()
    {
        var exception = TestSupport.Catch(() => Result.Ok(1).MapOk<int, int, RopError>(_ => throw new OperationCanceledException()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }
}
