using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class RopErrorInnerErrorsTests
{
    [Test]
    public async Task CreatedError_HasNoInnerErrors()
    {
        var error = RopError.Create("E_X", "bad", "details");

        await Assert.That(error.InnerErrors).IsNotNull();
        await Assert.That(error.InnerErrors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task FromException_HasNoInnerErrors()
    {
        var error = RopError.FromException(new InvalidOperationException("boom"));

        await Assert.That(error.InnerErrors).IsNotNull();
        await Assert.That(error.InnerErrors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Combine_PreservesEveryErrorInOrder()
    {
        var first = Sample.Error("E_FIRST", "one");
        var second = Sample.Error("E_SECOND", "two");

        var combined = RopError.Combine([first, second]);

        await Assert.That(combined.InnerErrors.Count).IsEqualTo(2);
        await Assert.That(combined.InnerErrors[0]).IsSameReferenceAs(first);
        await Assert.That(combined.InnerErrors[1]).IsSameReferenceAs(second);
    }

    [Test]
    public async Task Combine_WithSingleError_ReturnsThatErrorWithoutWrapping()
    {
        var error = Sample.Error("E_A", "a");

        var combined = RopError.Combine([error]);

        await Assert.That(combined).IsSameReferenceAs(error);
        await Assert.That(combined.Code).IsEqualTo("E_A");
        await Assert.That(combined.InnerErrors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Combine_WithNestedAggregate_KeepsNesting()
    {
        var first = Sample.Error("E_FIRST", "one");
        var second = Sample.Error("E_SECOND", "two");
        var third = Sample.Error("E_THIRD", "three");
        var inner = RopError.Combine([first, second]);

        var combined = RopError.Combine([inner, third]);

        await Assert.That(combined.InnerErrors.Count).IsEqualTo(2);
        await Assert.That(combined.InnerErrors[0]).IsSameReferenceAs(inner);
        await Assert.That(combined.InnerErrors[0].InnerErrors.Count).IsEqualTo(2);
        await Assert.That(combined.InnerErrors[1]).IsSameReferenceAs(third);
    }

    [Test]
    public async Task Combine_SnapshotsTheSourceCollection()
    {
        var first = Sample.Error("E_FIRST", "one");
        var second = Sample.Error("E_SECOND", "two");
        var source = new List<RopError> { first, second };

        var combined = RopError.Combine(source);
        source[0] = Sample.Error("E_REPLACED", "replaced");

        await Assert.That(combined.InnerErrors[0]).IsSameReferenceAs(first);
        await Assert.That(combined.InnerErrors.Count).IsEqualTo(2);
    }

    [Test]
    public async Task CombinedInnerErrors_AreReadOnly()
    {
        var combined = RopError.Combine([Sample.Error("E_A", "a"), Sample.Error("E_B", "b")]);

        await Assert.That(combined.InnerErrors is IList<RopError> { IsReadOnly: true }).IsTrue();
    }

    [Test]
    public async Task WithCode_PreservesInnerErrors()
    {
        var combined = RopError.Combine([Sample.Error("E_A", "a"), Sample.Error("E_B", "b")]);

        var updated = combined.WithCode("E_OUTER");

        await Assert.That(updated.Code).IsEqualTo("E_OUTER");
        await Assert.That(updated.InnerErrors).IsSameReferenceAs(combined.InnerErrors);
    }

    [Test]
    public async Task WithMessage_PreservesInnerErrors()
    {
        var combined = RopError.Combine([Sample.Error("E_A", "a"), Sample.Error("E_B", "b")]);

        var updated = combined.WithMessage("outer");

        await Assert.That(updated.Message).IsEqualTo("outer");
        await Assert.That(updated.InnerErrors).IsSameReferenceAs(combined.InnerErrors);
    }

    [Test]
    public async Task WithDetails_PreservesInnerErrors()
    {
        var combined = RopError.Combine([Sample.Error("E_A", "a"), Sample.Error("E_B", "b")]);

        var updated = combined.WithDetails("outer");

        await Assert.That(updated.Details).IsEqualTo("outer");
        await Assert.That(updated.InnerErrors).IsSameReferenceAs(combined.InnerErrors);
    }

    [Test]
    public async Task WithKind_PreservesInnerErrors()
    {
        var combined = RopError.Combine([Sample.Error("E_A", "a"), Sample.Error("E_B", "b")]);

        var updated = combined.WithKind(RopErrorKind.Validation);

        await Assert.That(updated.Kind).IsEqualTo(RopErrorKind.Validation);
        await Assert.That(updated.InnerErrors).IsSameReferenceAs(combined.InnerErrors);
    }

    [Test]
    public async Task Equality_IgnoresInnerErrors()
    {
        var first = Sample.Error("E_A", "a");
        var second = Sample.Error("E_B", "b");
        var two = RopError.Combine([first, second]);
        var three = RopError.Combine([first, second, first]);

        await Assert.That(two.InnerErrors.Count).IsEqualTo(2);
        await Assert.That(three.InnerErrors.Count).IsEqualTo(3);
        await Assert.That(two.Equals(three)).IsTrue();
        await Assert.That(two == three).IsTrue();
        await Assert.That(two.GetHashCode()).IsEqualTo(three.GetHashCode());
    }

    [Test]
    public async Task ToSingleError_WithCombine_PreservesInnerErrors()
    {
        var first = Sample.Error("E_FIRST", "one");
        var second = Sample.Error("E_SECOND", "two");

        var collapsed = Rop.Err<int, IReadOnlyList<RopError>>([first, second])
            .ToSingleError(RopError.Combine);

        await Assert.That(collapsed.ErrOrThrow().Code).IsEqualTo("E_MULTIPLE");
        await Assert.That(collapsed.ErrOrThrow().InnerErrors.Count).IsEqualTo(2);
        await Assert.That(collapsed.ErrOrThrow().InnerErrors[0]).IsSameReferenceAs(first);
        await Assert.That(collapsed.ErrOrThrow().InnerErrors[1]).IsSameReferenceAs(second);
    }
}
