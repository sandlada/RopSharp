using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class RopErrorTests
{
    [Test]
    public async Task Create_SetsCodeMessageAndOptionalDetails()
    {
        var error = RopError.Create("E_USER_NOT_FOUND", "user 42 not found", "id=42");

        await Assert.That(error.Code).IsEqualTo("E_USER_NOT_FOUND");
        await Assert.That(error.Message).IsEqualTo("user 42 not found");
        await Assert.That(error.Details).IsEqualTo("id=42");
        await Assert.That(error.Cause).IsNull();
    }

    [Test]
    public async Task Create_WithoutDetails_LeavesDetailsNull()
    {
        var error = RopError.Create("E_X", "bad");

        await Assert.That(error.Details).IsNull();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Create_WithBlankCode_Throws(string code)
    {
        var exception = TestSupport.Catch(() => RopError.Create(code, "message"));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Create_WithBlankMessage_Throws(string message)
    {
        var exception = TestSupport.Catch(() => RopError.Create("E_X", message));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task Create_WithEmptyDetails_Throws()
    {
        var exception = TestSupport.Catch(() => RopError.Create("E_X", "message", "  "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task FromException_CapturesCauseAndDefaultCode()
    {
        var cause = new InvalidOperationException("boom");

        var error = RopError.FromException(cause);

        await Assert.That(error.Code).IsEqualTo("E_EXN");
        await Assert.That(error.Message).IsEqualTo("boom");
        await Assert.That(error.Cause).IsSameReferenceAs(cause);
    }

    [Test]
    public async Task FromException_UsesSuppliedCode()
    {
        var error = RopError.FromException(new InvalidOperationException("boom"), "E_IO");

        await Assert.That(error.Code).IsEqualTo("E_IO");
    }

    [Test]
    public async Task FromException_WithNull_Throws()
    {
        var exception = TestSupport.Catch(() => RopError.FromException(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task FromException_WithEmptyExceptionMessage_FallsBackToTypeName()
    {
        var error = RopError.FromException(new InvalidOperationException(string.Empty));

        await Assert.That(error.Message).IsEqualTo(nameof(InvalidOperationException));
    }

    [Test]
    public async Task WithCode_ReturnsNewInstanceAndPreservesOtherFields()
    {
        var original = RopError.Create("E_A", "message", "details");

        var updated = original.WithCode("E_B");

        await Assert.That(original.Code).IsEqualTo("E_A");
        await Assert.That(updated.Code).IsEqualTo("E_B");
        await Assert.That(updated.Message).IsEqualTo("message");
        await Assert.That(updated.Details).IsEqualTo("details");
        await Assert.That(updated).IsNotSameReferenceAs(original);
    }

    [Test]
    public async Task WithMessage_PreservesCause()
    {
        var cause = new InvalidOperationException("boom");
        var original = RopError.FromException(cause, "E_EXN");

        var updated = original.WithMessage("different");

        await Assert.That(updated.Message).IsEqualTo("different");
        await Assert.That(updated.Cause).IsSameReferenceAs(cause);
    }

    [Test]
    public async Task WithDetails_CanRemoveDetails()
    {
        var original = RopError.Create("E_A", "message", "details");

        var updated = original.WithDetails(null);

        await Assert.That(updated.Details).IsNull();
        await Assert.That(original.Details).IsEqualTo("details");
    }

    [Test]
    public async Task Equality_IgnoresCause()
    {
        var left = RopError.FromException(new InvalidOperationException("boom"), "E_X");
        var right = RopError.FromException(new InvalidOperationException("boom"), "E_X");

        await Assert.That(left.Cause).IsNotSameReferenceAs(right.Cause);
        await Assert.That(left.Equals(right)).IsTrue();
        await Assert.That(left == right).IsTrue();
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task Equality_ComparesCodeMessageAndDetails()
    {
        var baseline = RopError.Create("E_A", "message", "details");

        await Assert.That(baseline.Equals(RopError.Create("E_A", "message", "details"))).IsTrue();
        await Assert.That(baseline.Equals(RopError.Create("E_B", "message", "details"))).IsFalse();
        await Assert.That(baseline.Equals(RopError.Create("E_A", "other", "details"))).IsFalse();
        await Assert.That(baseline.Equals(RopError.Create("E_A", "message", null))).IsFalse();
        await Assert.That(baseline != RopError.Create("E_B", "message", "details")).IsTrue();
    }

    [Test]
    public async Task Combine_WithSingleError_ReturnsThatError()
    {
        var error = RopError.Create("E_A", "message");

        var combined = RopError.Combine([error]);

        await Assert.That(combined).IsSameReferenceAs(error);
    }

    [Test]
    public async Task Combine_WithMultipleErrors_JoinsMessagesAndCodes()
    {
        var combined = RopError.Combine(
        [
            RopError.Create("E_NAME", "name is empty"),
            RopError.Create("E_AMOUNT", "amount is negative"),
        ]);

        await Assert.That(combined.Code).IsEqualTo("E_MULTIPLE");
        await Assert.That(combined.Message).IsEqualTo("name is empty; amount is negative");
        await Assert.That(combined.Details).IsEqualTo("E_NAME, E_AMOUNT");
        await Assert.That(combined.Cause).IsNull();
    }

    [Test]
    public async Task Combine_WithEmptyList_Throws()
    {
        var exception = TestSupport.Catch(() => RopError.Combine([]));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task Combine_WithNull_Throws()
    {
        var exception = TestSupport.Catch(() => RopError.Combine(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task ToString_IncludesCodeAndMessage()
    {
        var error = RopError.Create("E_A", "message", "details");

        await Assert.That(error.ToString()).IsEqualTo("E_A: message (details)");
    }
}
