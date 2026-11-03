using System.Globalization;
using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class BoundaryTests
{
    [Test]
    public async Task Try_ReturnsOkWhenFunctionSucceeds()
    {
        var result = Result.Try(() => 42, _ => Sample.Error());

        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task Try_ConvertsExceptionToErrorWithCause()
    {
        var result = Result.Try<int>(() => throw new FormatException("bad"), exception => RopError.FromException(exception, "E_PARSE"));

        var error = result.ExpectErr();

        await Assert.That(error.Code).IsEqualTo("E_PARSE");
        await Assert.That(error.Cause is FormatException).IsTrue();
    }

    [Test]
    public async Task Try_WithCodeOverload_UsesProvidedCode()
    {
        var result = Result.Try<int>(() => throw new InvalidOperationException("bad"), "E_CODE");

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_CODE");
    }

    [Test]
    public async Task Try_WithNullResult_ReturnsNullResultError()
    {
        var result = Result.Try<string>(() => null!, _ => Sample.Error());

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_NULL_RESULT");
    }

    [Test]
    public async Task Try_RethrowsOperationCanceledException()
    {
        var exception = TestSupport.Catch(() =>
            Result.Try<int>(() => throw new OperationCanceledException(), _ => Sample.Error()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task Try_WithNullArguments_Throws()
    {
        await Assert.That(TestSupport.Catch(() => Result.Try<int>(null!, _ => Sample.Error())) is ArgumentNullException).IsTrue();
        await Assert.That(TestSupport.Catch(() => Result.Try(() => 1, (Func<Exception, RopError>)null!)) is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task TrySwitch_LiftsFunctionAndCapturesExceptions()
    {
        var safeParse = Result.TrySwitch<string, int>(
            text => int.Parse(text, CultureInfo.InvariantCulture),
            exception => RopError.FromException(exception, "E_PARSE"));

        var ok = safeParse("12");
        var err = safeParse("nope");

        await Assert.That(ok.Expect("test")).IsEqualTo(12);
        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task Switch_LiftsWithoutCatchingExceptions()
    {
        var parse = Result.Switch<string, int, RopError>(text => int.Parse(text, CultureInfo.InvariantCulture));

        var ok = parse("12");
        var exception = TestSupport.Catch(() => parse("nope"));

        await Assert.That(ok.Expect("test")).IsEqualTo(12);
        await Assert.That(exception is FormatException).IsTrue();
    }

    [Test]
    public async Task Switch_WithNullFunction_Throws()
    {
        var exception = TestSupport.Catch(() => Result.Switch<string, int, RopError>(null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task FromNullable_ForReferenceTypes()
    {
        string? missing = null;
        string? present = "x";

        var err = Result.FromNullable(missing, () => Sample.Error("E_NULL"));
        var ok = Result.FromNullable(present, () => Sample.Error());

        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_NULL");
        await Assert.That(ok.Expect("test")).IsEqualTo("x");
    }

    [Test]
    public async Task FromNullable_DoesNotEvaluateFactoryWhenPresent()
    {
        string? present = "x";
        var called = false;

        var result = Result.FromNullable(present, () =>
        {
            called = true;
            return Sample.Error();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.Expect("test")).IsEqualTo("x");
    }

    [Test]
    public async Task FromNullableValue_ForValueTypes()
    {
        int? missing = null;
        int? present = 5;

        var err = Result.FromNullableValue(missing, () => Sample.Error("E_AGE_NULL"));
        var ok = Result.FromNullableValue(present, () => Sample.Error());

        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_AGE_NULL");
        await Assert.That(ok.Expect("test")).IsEqualTo(5);
    }

    [Test]
    public async Task FromNullable_WithNullFactory_Throws()
    {
        string? present = "x";

        var exception = TestSupport.Catch(() => Result.FromNullable(present, null!));

        await Assert.That(exception is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task FromNullable_WithFactoryReturningNull_Throws()
    {
        string? missing = null;

        var exception = TestSupport.Catch(() => Result.FromNullable(missing, () => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }
}
