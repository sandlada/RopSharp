using System.Globalization;
using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

internal sealed record AppError(string Code, string Message);

internal readonly record struct StructError(string Code, int Status);

public sealed class CustomErrorTypeTests
{
    private static AppError App(string code = "E_TEST", string message = "failed") => new(code, message);

    [Test]
    public async Task Result_WithCustomClassError_StoresTheErrorValue()
    {
        var error = App("E_INVALID", "invalid input");

        Result<int, AppError> result = Result.Err<int, AppError>(error);

        await Assert.That(result.IsErr).IsTrue();
        await Assert.That(result.IsOk).IsFalse();
        await Assert.That(result.TryGetErr(out var actual)).IsTrue();
        await Assert.That(ReferenceEquals(actual, error)).IsTrue();
    }

    [Test]
    public async Task Result_WithCustomClassError_UsesTheErrorValueEquality()
    {
        var left = Result.Err<int, AppError>(new AppError("E", "m"));
        var right = Result.Err<int, AppError>(new AppError("E", "m"));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task Result_WithCustomClassError_SupportsTheCombinatorSurface()
    {
        Result<int, AppError> source = Result.Ok<int, AppError>(2);

        Result<string, AppError> result = source
            .MapOk(value => value * 3)
            .BindOk(value => value > 0
                ? Result.Ok<string, AppError>($"v{value}")
                : Result.Err<string, AppError>(App("E_NEG")))
            .Ensure(value => value.Length > 1, App("E_SHORT"))
            .MapErr(error => error with { Code = "E_MAPPED" });

        await Assert.That(result.Expect("test")).IsEqualTo("v6");
    }

    [Test]
    public async Task Result_WithCustomClassError_MapErrChangesOnlyTheErrorTrack()
    {
        Result<int, AppError> failed = Result.Err<int, AppError>(App("E_ORIGINAL"));

        var mapped = failed.MapErr(error => error with { Message = "mapped" });

        await Assert.That(mapped.ExpectErr().Code).IsEqualTo("E_ORIGINAL");
        await Assert.That(mapped.ExpectErr().Message).IsEqualTo("mapped");
    }

    [Test]
    public async Task Result_WithCustomStructError_UsesValueEquality()
    {
        var left = Result.Err<int, StructError>(new StructError("E", 400));
        var right = Result.Err<int, StructError>(new StructError("E", 400));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task Result_WithCustomStructError_AcceptsTheDefaultErrorValue()
    {
        var result = Result.Err<int, StructError>(default);

        await Assert.That(result.IsErr).IsTrue();
        await Assert.That(result.TryGetErr(out var error)).IsTrue();
        await Assert.That(error).IsEqualTo(default(StructError));
    }

    [Test]
    public async Task FromNullable_WithCustomError()
    {
        string? missing = null;
        string? present = "value";

        var err = Result.FromNullable(missing, () => App("E_NULL"));
        var ok = Result.FromNullable(present, () => App("E_NULL"));

        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_NULL");
        await Assert.That(ok.Expect("test")).IsEqualTo("value");
    }

    [Test]
    public async Task FromNullableValue_WithCustomError()
    {
        int? missing = null;
        int? present = 5;

        var err = Result.FromNullableValue(missing, () => App("E_MISSING"));
        var ok = Result.FromNullableValue(present, () => App("E_MISSING"));

        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_MISSING");
        await Assert.That(ok.Expect("test")).IsEqualTo(5);
    }

    [Test]
    public async Task SuccessIfAndFailureIf_WithCustomError()
    {
        var success = Result.SuccessIf(true, 1, App("E_CONDITION"));
        var successFailure = Result.SuccessIf(false, 1, App("E_CONDITION"));
        var failure = Result.FailureIf(true, 2, App("E_REJECT"));
        var failureSuccess = Result.FailureIf(false, 2, App("E_REJECT"));

        await Assert.That(success.Expect("test")).IsEqualTo(1);
        await Assert.That(successFailure.ExpectErr().Code).IsEqualTo("E_CONDITION");
        await Assert.That(failure.ExpectErr().Code).IsEqualTo("E_REJECT");
        await Assert.That(failureSuccess.Expect("test")).IsEqualTo(2);
    }

    [Test]
    public async Task SuccessIf_WithCustomError_DoesNotEvaluateTheFactoryOnSuccess()
    {
        var called = false;

        var result = Result.SuccessIf(true, 1, () =>
        {
            called = true;
            return App();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task RequireNotNull_WithCustomError()
    {
        Result<string?, AppError> source =
            Result.Ok<string, AppError>("x").MapOk<string, string?, AppError>(_ => null);

        var result = source.RequireNotNull(() => App("E_NULL", "name required"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_NULL");
    }

    [Test]
    public async Task RequireNotNull_WithCustomError_PassesTheOriginalErrorThrough()
    {
        Result<string?, AppError> source = Result.Err<string?, AppError>(App("E_ORIGINAL"));

        var result = source.RequireNotNull(() => App("E_NULL"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_ORIGINAL");
    }

    [Test]
    public async Task Either_ToResult_WithCustomError()
    {
        Either<AppError, int> left = Either.Left<AppError, int>(App("E_LEFT"));
        Either<AppError, int> right = Either.Right<AppError, int>(42);

        var err = left.ToResult(error => error);
        var ok = right.ToResult(error => error);

        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_LEFT");
        await Assert.That(ok.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task Validate_WithCustomError_AggregatesEveryErrorInArgumentOrder()
    {
        Result<int, AppError> first = Result.Err<int, AppError>(App("E_A"));
        Result<int, AppError> second = Result.Err<int, AppError>(App("E_B"));

        var validated = Result.Validate(first, second, (a, b) => a + b);

        await Assert.That(validated.TryGetErr(out var errors)).IsTrue();
        var captured = errors!;
        await Assert.That(captured.Count).IsEqualTo(2);
        await Assert.That(captured[0].Code).IsEqualTo("E_A");
        await Assert.That(captured[1].Code).IsEqualTo("E_B");
    }

    [Test]
    public async Task SequenceValidation_WithCustomError_AggregatesEveryError()
    {
        Result<int, AppError>[] results =
        [
            Result.Err<int, AppError>(App("E_A")),
            Result.Ok<int, AppError>(2),
            Result.Err<int, AppError>(App("E_B")),
        ];

        var sequence = Result.SequenceValidation(results);

        await Assert.That(sequence.TryGetErr(out var errors)).IsTrue();
        var captured = errors!;
        await Assert.That(captured.Count).IsEqualTo(2);
        await Assert.That(captured[0].Code).IsEqualTo("E_A");
        await Assert.That(captured[1].Code).IsEqualTo("E_B");
    }

    [Test]
    public async Task ToSingleError_WithCustomError_TakesTheFirstError()
    {
        Result<int, AppError>[] results =
        [
            Result.Err<int, AppError>(App("E_A")),
            Result.Err<int, AppError>(App("E_B")),
        ];

        var single = Result.SequenceValidation(results).ToSingleError();

        await Assert.That(single.ExpectErr().Code).IsEqualTo("E_A");
    }

    [Test]
    public async Task ToSingleError_WithCustomError_UsesTheSuppliedCombine()
    {
        Result<int, AppError>[] results =
        [
            Result.Err<int, AppError>(App("E_A")),
            Result.Err<int, AppError>(App("E_B")),
        ];

        var single = Result.SequenceValidation(results)
            .ToSingleError(errors => new AppError("E_MULTIPLE", string.Join(";", errors.Select(error => error.Code))));

        await Assert.That(single.ExpectErr().Code).IsEqualTo("E_MULTIPLE");
        await Assert.That(single.ExpectErr().Message).IsEqualTo("E_A;E_B");
    }

    [Test]
    public async Task Linq_WithCustomError_ShortCircuitsOnTheErrorTrack()
    {
        var sum = from a in Result.Ok<int, AppError>(1)
                  from b in Result.Ok<int, AppError>(2)
                  select a + b;

        var failed = from a in Result.Err<int, AppError>(App("E_LINQ"))
                     from b in Result.Ok<int, AppError>(2)
                     select a + b;

        await Assert.That(sum.Expect("test")).IsEqualTo(3);
        await Assert.That(failed.ExpectErr().Code).IsEqualTo("E_LINQ");
    }

    [Test]
    public async Task Try_WithCustomError_ReturnsOk()
    {
        var result = Result.Try(() => 42, _ => App("E_FAIL"), () => App("E_NULL"));

        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task Try_WithCustomError_ConvertsExceptions()
    {
        var result = Result.Try<int, AppError>(
            () => throw new FormatException("bad"),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_PARSE");
        await Assert.That(result.ExpectErr().Message).IsEqualTo("bad");
    }

    [Test]
    public async Task Try_WithCustomError_UsesTheNullResultFactory()
    {
        var called = 0;

        var result = Result.Try<string, AppError>(
            () => null!,
            _ => App("E_FAIL"),
            () =>
            {
                called++;
                return App("E_NULL", "null result");
            });

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_NULL");
        await Assert.That(called).IsEqualTo(1);
    }

    [Test]
    public async Task Try_WithCustomError_RethrowsOperationCanceledException()
    {
        var exception = TestSupport.Catch(() =>
            Result.Try<int, AppError>(
                () => throw new OperationCanceledException(),
                _ => App(),
                () => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task Try_WithCustomError_GuardsEveryArgument()
    {
        var nullFunction = TestSupport.Catch(() =>
            Result.Try<int, AppError>(null!, _ => App(), () => App()));
        var nullFactory = TestSupport.Catch(() =>
            Result.Try<int, AppError>(() => 1, null!, () => App()));
        var nullResultFactory = TestSupport.Catch(() =>
            Result.Try<int, AppError>(() => 1, _ => App(), null!));

        await Assert.That(nullFunction is ArgumentNullException).IsTrue();
        await Assert.That(nullFactory is ArgumentNullException).IsTrue();
        await Assert.That(nullResultFactory is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task Try_WithCustomError_ThrowsWhenTheFactoryReturnsNull()
    {
        var exception = TestSupport.Catch(() =>
            Result.Try<int, AppError>(
                () => throw new InvalidOperationException("boom"),
                _ => null!,
                () => App()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAction_WithCustomError_ReturnsUnit()
    {
        var executed = false;

        var result = Result.Try(() => { executed = true; }, exception => App("E_ACTION", exception.Message));

        await Assert.That(executed).IsTrue();
        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.Expect("test")).IsEqualTo(Unit.Value);
    }

    [Test]
    public async Task TryAction_WithCustomError_ConvertsExceptions()
    {
        var result = Result.Try(
            () => throw new FormatException("bad"),
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_ACTION");
    }

    [Test]
    public async Task TrySwitch_WithCustomError_LiftsTheFunction()
    {
        var lifted = Result.TrySwitch<string, int, AppError>(
            text => int.Parse(text, CultureInfo.InvariantCulture),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        var ok = lifted("12");
        var err = lifted("nope");

        await Assert.That(ok.Expect("test")).IsEqualTo(12);
        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task TrySwitch_WithCustomError_UsesTheNullResultFactory()
    {
        var lifted = Result.TrySwitch<string, string, AppError>(
            _ => null!,
            _ => App("E_FAIL"),
            () => App("E_NULL"));

        var result = lifted("x");

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_NULL");
    }

    [Test]
    public async Task TryAsync_WithCustomError_ReturnsOk()
    {
        var result = await Result.TryAsync<int, AppError>(
            () => Task.FromResult(42),
            _ => App("E_FAIL"),
            () => App("E_NULL"));

        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task TryAsync_WithCustomError_ConvertsExceptions()
    {
        var result = await Result.TryAsync<int, AppError>(
            () => Task.FromException<int>(new FormatException("bad")),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task TryAsync_WithCustomError_UsesTheNullResultFactory()
    {
        var result = await Result.TryAsync<string, AppError>(
            () => Task.FromResult<string>(null!),
            _ => App("E_FAIL"),
            () => App("E_NULL", "null result"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_NULL");
    }

    [Test]
    public async Task TryAsync_WithCustomError_ThrowsForANullTask()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync<int, AppError>(() => null!, _ => App(), () => App()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCustomError_ObservesAnAlreadyCancelledToken()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync<int, AppError>(
                () => Task.FromResult(1),
                _ => App(),
                () => App(),
                source.Token));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCustomError_ObservesCancellationWhileAwaiting()
    {
        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var source = new CancellationTokenSource();

        var task = Result.TryAsync<int, AppError>(
            () => pending.Task,
            _ => App(),
            () => App(),
            source.Token);

        await source.CancelAsync();
        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ReturnsUnit()
    {
        var result = await Result.TryAsync<AppError>(
            () => Task.CompletedTask,
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.Expect("test")).IsEqualTo(Unit.Value);
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ConvertsExceptions()
    {
        var result = await Result.TryAsync<AppError>(
            () => Task.FromException(new InvalidOperationException("bad")),
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_ACTION");
    }

    [Test]
    public async Task TrySwitchAsync_WithCustomError_LiftsTheFunction()
    {
        var lifted = Result.TrySwitchAsync<string, int, AppError>(
            text => Task.FromResult(int.Parse(text, CultureInfo.InvariantCulture)),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        var ok = await lifted("12");
        var err = await lifted("nope");

        await Assert.That(ok.Expect("test")).IsEqualTo(12);
        await Assert.That(err.ExpectErr().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task Parse_WithCustomError_ReturnsOk()
    {
        var result = Result.Parse<int, AppError>("42", failure => new AppError(failure.Code, failure.Message));

        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task Parse_WithCustomError_MapsTheCompleteDiagnosticPayload()
    {
        ParseFailure? captured = null;

        var result = Result.Parse<int, AppError>("abc", failure =>
        {
            captured = failure;
            return new AppError(failure.Code, failure.Message);
        });

        var diagnostic = captured!.Value;
        await Assert.That(result.IsErr).IsTrue();
        await Assert.That(diagnostic.Code).IsEqualTo("E_PARSE");
        await Assert.That(diagnostic.Input).IsEqualTo("abc");
        await Assert.That(diagnostic.TypeName).IsEqualTo("Int32");
        await Assert.That(diagnostic.Message).IsEqualTo("'abc' is not a valid Int32.");
        await Assert.That(diagnostic.Details).IsEqualTo("input=\"abc\"");
    }

    [Test]
    public async Task Parse_WithCustomError_DistinguishesNullFromEmptyInput()
    {
        ParseFailure? nullFailure = null;
        ParseFailure? emptyFailure = null;

        Result.Parse<int, AppError>(null, failure =>
        {
            nullFailure = failure;
            return App();
        });
        Result.Parse<int, AppError>(string.Empty, failure =>
        {
            emptyFailure = failure;
            return App();
        });

        await Assert.That(nullFailure!.Value.Input).IsNull();
        await Assert.That(nullFailure.Value.Message).IsEqualTo("The value is null, which is not a valid Int32.");
        await Assert.That(nullFailure.Value.Details).IsEqualTo("input=null");

        await Assert.That(emptyFailure!.Value.Input).IsEqualTo(string.Empty);
        await Assert.That(emptyFailure.Value.Message).IsEqualTo("'' is not a valid Int32.");
        await Assert.That(emptyFailure.Value.Details).IsEqualTo("input=\"\"");
    }

    [Test]
    public async Task Parse_WithCustomError_EscapesAndBoundsTheDiagnostics()
    {
        ParseFailure? escapedFailure = null;
        ParseFailure? truncatedFailure = null;
        var longInput = new string('a', 100);

        Result.Parse<int, AppError>("a\"b\nc", failure =>
        {
            escapedFailure = failure;
            return App();
        });
        Result.Parse<int, AppError>(longInput, failure =>
        {
            truncatedFailure = failure;
            return App();
        });

        await Assert.That(escapedFailure!.Value.Details.Contains("\\\"")).IsTrue();
        await Assert.That(escapedFailure.Value.Details.Contains("\\n")).IsTrue();
        await Assert.That(escapedFailure.Value.Details.Contains('\n')).IsFalse();

        await Assert.That(truncatedFailure!.Value.Input!.Length).IsEqualTo(100);
        await Assert.That(truncatedFailure.Value.Details.Contains("...")).IsTrue();
        await Assert.That(truncatedFailure.Value.Details.Contains(new string('a', 65))).IsFalse();
    }

    [Test]
    public async Task Parse_WithCustomError_HonorsTheCustomCode()
    {
        ParseFailure? captured = null;

        Result.Parse<int, AppError>(
            "x",
            failure =>
            {
                captured = failure;
                return App();
            },
            "E_QUANTITY");

        await Assert.That(captured!.Value.Code).IsEqualTo("E_QUANTITY");
    }

    [Test]
    public async Task Parse_WithCustomError_DoesNotEvaluateTheFactoryOnSuccess()
    {
        var called = false;

        var result = Result.Parse<int, AppError>("42", _ =>
        {
            called = true;
            return App();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.Expect("test")).IsEqualTo(42);
    }

    [Test]
    public async Task Parse_WithCustomError_GuardsTheFactoryAndCode()
    {
        var nullFactory = TestSupport.Catch(() => Result.Parse<int, AppError>("1", null!));
        var blankCode = TestSupport.Catch(() => Result.Parse<int, AppError>("1", _ => App(), " "));

        await Assert.That(nullFactory is ArgumentNullException).IsTrue();
        await Assert.That(blankCode is ArgumentException).IsTrue();
    }

    [Test]
    public async Task Parse_WithCustomError_ThrowsWhenTheFactoryReturnsNull()
    {
        var exception = TestSupport.Catch(() => Result.Parse<int, AppError>("x", _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task FailureIf_WithCustomError_DoesNotEvaluateTheFactoryOnSuccess()
    {
        var called = false;

        var result = Result.FailureIf(false, 1, () =>
        {
            called = true;
            return App();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.Expect("test")).IsEqualTo(1);
    }

    [Test]
    public async Task FailureIf_WithCustomError_UsesTheFactoryWhenTheConditionHolds()
    {
        var result = Result.FailureIf(true, 1, () => App("E_REJECT"));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_REJECT");
    }

    [Test]
    public async Task TryAction_WithCustomError_RethrowsOperationCanceledException()
    {
        var exception = TestSupport.Catch(() =>
            Result.Try(() => throw new OperationCanceledException(), _ => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ConvertsSynchronousExceptions()
    {
        var result = await Result.TryAsync<AppError>(
            () => throw new InvalidOperationException("sync"),
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.ExpectErr().Code).IsEqualTo("E_ACTION");
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ThrowsForANullTask()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync<AppError>(() => null!, _ => App()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_RethrowsOperationCanceledException()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync<AppError>(() => throw new OperationCanceledException(), _ => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ObservesCancellationWhileAwaiting()
    {
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var source = new CancellationTokenSource();

        var task = Result.TryAsync<AppError>(() => pending.Task, _ => App(), source.Token);

        await source.CancelAsync();
        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCustomError_RethrowsSynchronousOperationCanceledException()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync<int, AppError>(
                () => throw new OperationCanceledException(),
                _ => App(),
                () => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task Try_WithCustomError_PropagatesNullResultFactoryFailures()
    {
        var exception = TestSupport.Catch(() =>
            Result.Try<string, AppError>(
                () => null!,
                _ => App(),
                () => throw new InvalidOperationException("factory bug")));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCustomError_PropagatesNullResultFactoryFailures()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Result.TryAsync<string, AppError>(
                () => Task.FromResult<string>(null!),
                _ => App(),
                () => throw new InvalidOperationException("factory bug")));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task ParseFailure_ToString_RendersTheDiagnostic()
    {
        var failure = new ParseFailure("E_PARSE", "bad input", "abc", "Int32");

        await Assert.That(failure.ToString()).IsEqualTo("E_PARSE: bad input (input=\"abc\")");
    }

    [Test]
    public async Task ExistingRopErrorOverloads_RemainUnchanged()
    {
        Result<int, RopError> ok = Result.Ok(1);
        Result<int, RopError> err = Result.Err<int>(Sample.Error());
        Result<string, RopError> nullable = Result.FromNullable<string>(null, () => Sample.Error());
        Result<int, RopError> conditional = Result.SuccessIf(true, 1, Sample.Error());
        Result<int, RopError> tried = Result.Try(() => 1, _ => Sample.Error());
        var triedAsync = Result.TryAsync(() => Task.FromResult(1), _ => Sample.Error());
        Result<int, RopError> validated = Result.Validate(ok, ok, (a, b) => a + b).ToSingleError();
        Result<int, RopError> fromEither = Either.Right<string, int>(1).ToResult(_ => Sample.Error());

        Result<string?, RopError> nullSuccess = Result.Ok<string, RopError>("x")
            .MapOk<string, string?, RopError>(_ => null);
        Result<string, RopError> required = nullSuccess.RequireNotNull(() => Sample.Error("E_NULL"));

        await Assert.That(ok.Expect("test")).IsEqualTo(1);
        await Assert.That(err.IsErr).IsTrue();
        await Assert.That(nullable.ExpectErr().Code).IsEqualTo("E_TEST");
        await Assert.That(conditional.Expect("test")).IsEqualTo(1);
        await Assert.That(tried.Expect("test")).IsEqualTo(1);
        await Assert.That((await triedAsync).Expect("test")).IsEqualTo(1);
        await Assert.That(validated.Expect("test")).IsEqualTo(2);
        await Assert.That(fromEither.Expect("test")).IsEqualTo(1);
        await Assert.That(required.ExpectErr().Code).IsEqualTo("E_NULL");
    }
}
