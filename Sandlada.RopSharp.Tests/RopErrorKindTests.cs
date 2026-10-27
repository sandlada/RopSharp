using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class RopErrorKindTests
{
    [Test]
    public async Task Create_DefaultsToFailureKind()
    {
        var error = RopError.Create("E_X", "bad");

        await Assert.That(error.Kind).IsEqualTo(RopErrorKind.Failure);
    }

    [Test]
    public async Task Create_WithKindAndDetails_PreservesBoth()
    {
        var error = RopError.Create("E_X", "bad", "details", RopErrorKind.Validation);

        await Assert.That(error.Kind).IsEqualTo(RopErrorKind.Validation);
        await Assert.That(error.Details).IsEqualTo("details");
    }

    [Test]
    public async Task Create_WithKindOnly_PreservesKind()
    {
        var error = RopError.Create("E_X", "bad", RopErrorKind.NotFound);

        await Assert.That(error.Kind).IsEqualTo(RopErrorKind.NotFound);
        await Assert.That(error.Details).IsNull();
    }

    [Test]
    public async Task FromException_DefaultsToUnexpectedKind()
    {
        var error = RopError.FromException(new InvalidOperationException("boom"));

        await Assert.That(error.Kind).IsEqualTo(RopErrorKind.Unexpected);
    }

    [Test]
    public async Task FromException_WithKind_PreservesKind()
    {
        var error = RopError.FromException(new InvalidOperationException("boom"), RopErrorKind.Conflict);

        await Assert.That(error.Kind).IsEqualTo(RopErrorKind.Conflict);
    }

    [Test]
    public async Task WithKind_PreservesOtherFields()
    {
        var original = RopError.Create("E_X", "bad", "details", RopErrorKind.Validation);

        var updated = original.WithKind(RopErrorKind.Forbidden);

        await Assert.That(updated.Kind).IsEqualTo(RopErrorKind.Forbidden);
        await Assert.That(updated.Code).IsEqualTo("E_X");
        await Assert.That(updated.Message).IsEqualTo("bad");
        await Assert.That(updated.Details).IsEqualTo("details");
    }

    [Test]
    public async Task Equality_IncludesKind()
    {
        var failure = RopError.Create("E_X", "bad");
        var validation = RopError.Create("E_X", "bad", RopErrorKind.Validation);

        await Assert.That(failure.Equals(validation)).IsFalse();
        await Assert.That(failure == validation).IsFalse();
    }

    [Test]
    public async Task Combine_PreservesUniformKind()
    {
        var combined = RopError.Combine(
        [
            RopError.Create("E_A", "a", RopErrorKind.Validation),
            RopError.Create("E_B", "b", RopErrorKind.Validation),
        ]);

        await Assert.That(combined.Kind).IsEqualTo(RopErrorKind.Validation);
    }

    [Test]
    public async Task Combine_MixedKindFallsBackToFailure()
    {
        var combined = RopError.Combine(
        [
            RopError.Create("E_A", "a", RopErrorKind.Validation),
            RopError.Create("E_B", "b", RopErrorKind.NotFound),
        ]);

        await Assert.That(combined.Kind).IsEqualTo(RopErrorKind.Failure);
    }
}
