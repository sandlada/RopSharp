using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class EdgeCaseTests
{
    [Test]
    public async Task SequenceValidation_WithEmptySource_ReturnsEmptyOk()
    {
        var result = Rop.SequenceValidation(Array.Empty<Result<int, RopError>>());

        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task SequenceValidation_WithAllSuccess_KeepsValues()
    {
        var result = Rop.SequenceValidation(new[] { Rop.Ok(1), Rop.Ok(2) });

        await TestSupport.AssertSequenceAsync(result.OrThrow(), 1, 2);
    }

    [Test]
    public async Task Traverse_WithEmptySource_ReturnsEmptyOk()
    {
        var result = Rop.Traverse(Array.Empty<int>(), value => Rop.Ok(value));

        await Assert.That(result.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task Partition_WithEmptySource_ReturnsEmptyLists()
    {
        var (oks, errs) = Rop.Partition(Array.Empty<Result<int, RopError>>());

        await Assert.That(oks.Count).IsEqualTo(0);
        await Assert.That(errs.Count).IsEqualTo(0);
    }

    [Test]
    public async Task OptTraverse_WithEmptySource_ReturnsEmptySome()
    {
        var option = Opt.Traverse(Array.Empty<int>(), value => Opt.Some(value));

        await Assert.That(option.OrThrow().Count).IsEqualTo(0);
    }

    [Test]
    public async Task Combine3_DoesNotInvokeMergerWhenAnyInputFails()
    {
        var called = false;
        var error = Sample.Error("E_X");

        var result = Rop.Combine(
            Rop.Ok(1),
            Rop.Err<int>(error),
            Rop.Ok(3),
            (int a, int b, int c) =>
            {
                called = true;
                return a + b + c;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(result.ErrOrThrow()).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Validate_DoesNotInvokeMergerWhenAnyInputFails()
    {
        var called = false;

        var result = Rop.Validate(
            Rop.Err<int>(Sample.Error()),
            Rop.Ok(2),
            (int a, int b) =>
            {
                called = true;
                return a + b;
            });

        await Assert.That(called).IsFalse();
        await Assert.That(result.IsErr).IsTrue();
    }

    [Test]
    public async Task ToSingleError_WithStrategy_ThrowsOnEmptyList()
    {
        var invalid = Rop.Err<int, IReadOnlyList<RopError>>([]);

        var exception = TestSupport.Catch(() => invalid.ToSingleError(RopError.Combine));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapErrAsync_WithCancellationAwareMapper_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();

        var result = await Task.FromResult(Rop.Err<int>(Sample.Error("E_X")))
            .MapErrAsync((error, token) => Task.FromResult(error.WithCode(token == cts.Token ? "E_CT" : "E_NO")), cts.Token);

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_CT");
    }

    [Test]
    public async Task TeeOkAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await Task.FromResult(Rop.Ok(1)).TeeOkAsync((value, token) =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        }, cts.Token);

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TeeErrAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await Task.FromResult(Rop.Err<int>(Sample.Error())).TeeErrAsync((error, token) =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        }, cts.Token);

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TeeSomeAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await Task.FromResult(Opt.Some(1)).TeeSomeAsync((value, token) =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        }, cts.Token);

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task TeeNoneAsync_WithCancellationAwareAction_ReceivesToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = false;

        await Task.FromResult(Opt.None<int>()).TeeNoneAsync(token =>
        {
            observed = token == cts.Token;
            return Task.CompletedTask;
        }, cts.Token);

        await Assert.That(observed).IsTrue();
    }

    [Test]
    public async Task OptionSequence_EnumeratesSourceOnce()
    {
        var source = new CountingSequence<Option<int>>([Opt.Some(1), Opt.Some(2)]);

        var option = Opt.Sequence(source);

        await Assert.That(option.OrThrow().Count).IsEqualTo(2);
        await Assert.That(source.GetEnumeratorCalls).IsEqualTo(1);
    }

    [Test]
    public async Task EmptyAggregations_NeverReturnNullLists()
    {
        var sequence = Rop.Sequence(Array.Empty<Result<int, RopError>>()).OrThrow();
        var validation = Rop.SequenceValidation(Array.Empty<Result<int, RopError>>()).OrThrow();
        var (oks, errs) = Rop.Partition(Array.Empty<Result<int, RopError>>());

        await Assert.That(sequence).IsNotNull();
        await Assert.That(validation).IsNotNull();
        await Assert.That(oks).IsNotNull();
        await Assert.That(errs).IsNotNull();
    }

    [Test]
    public async Task ExposedAggregationLists_AreReadOnlyViews()
    {
        var sequence = Rop.Sequence(new[] { Rop.Ok(1), Rop.Ok(2) }).OrThrow();
        var validation = Rop.SequenceValidation(new[] { Rop.Ok(1), Rop.Ok(2) }).OrThrow();
        var (oks, errs) = Rop.Partition(new[] { Rop.Ok(1), Rop.Err<int>(Sample.Error()) });

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
        var left = Rop.Ok(new Sample.Order("o", 10m));
        var right = Rop.Ok(new Sample.Order("o", 10m));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task Option_PayloadEquality_WithRecords()
    {
        var left = Opt.Some(new Sample.Order("o", 10m));
        var right = Opt.Some(new Sample.Order("o", 10m));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task TeeOk_ActionExceptionPropagates()
    {
        var exception = TestSupport.Catch(() => Rop.Ok(1).TeeOk(_ => throw new InvalidOperationException("bug")));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapOk_MapperExceptionPropagates()
    {
        var exception = TestSupport.Catch(() => Rop.Ok(1).MapOk<int, int, RopError>(_ => throw new InvalidOperationException("bug")));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task MapOk_OperationCanceledExceptionPropagates()
    {
        var exception = TestSupport.Catch(() => Rop.Ok(1).MapOk<int, int, RopError>(_ => throw new OperationCanceledException()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }
}
