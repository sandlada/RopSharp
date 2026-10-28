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

        Result<int, AppError> result = Rop.Err<int, AppError>(error);

        await Assert.That(result.IsErr).IsTrue();
        await Assert.That(result.IsOk).IsFalse();
        await Assert.That(result.TryGetErr(out var actual)).IsTrue();
        await Assert.That(ReferenceEquals(actual, error)).IsTrue();
    }

    [Test]
    public async Task Result_WithCustomClassError_UsesTheErrorValueEquality()
    {
        var left = Rop.Err<int, AppError>(new AppError("E", "m"));
        var right = Rop.Err<int, AppError>(new AppError("E", "m"));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task Result_WithCustomClassError_SupportsTheCombinatorSurface()
    {
        Result<int, AppError> source = Rop.Ok<int, AppError>(2);

        Result<string, AppError> result = source
            .MapOk(value => value * 3)
            .BindOk(value => value > 0
                ? Rop.Ok<string, AppError>($"v{value}")
                : Rop.Err<string, AppError>(App("E_NEG")))
            .Ensure(value => value.Length > 1, App("E_SHORT"))
            .MapErr(error => error with { Code = "E_MAPPED" });

        await Assert.That(result.OrThrow()).IsEqualTo("v6");
    }

    [Test]
    public async Task Result_WithCustomClassError_MapErrChangesOnlyTheErrorTrack()
    {
        Result<int, AppError> failed = Rop.Err<int, AppError>(App("E_ORIGINAL"));

        var mapped = failed.MapErr(error => error with { Message = "mapped" });

        await Assert.That(mapped.ErrOrThrow().Code).IsEqualTo("E_ORIGINAL");
        await Assert.That(mapped.ErrOrThrow().Message).IsEqualTo("mapped");
    }

    [Test]
    public async Task Result_WithCustomStructError_UsesValueEquality()
    {
        var left = Rop.Err<int, StructError>(new StructError("E", 400));
        var right = Rop.Err<int, StructError>(new StructError("E", 400));

        await Assert.That(left).IsEqualTo(right);
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
    }

    [Test]
    public async Task Result_WithCustomStructError_AcceptsTheDefaultErrorValue()
    {
        var result = Rop.Err<int, StructError>(default);

        await Assert.That(result.IsErr).IsTrue();
        await Assert.That(result.TryGetErr(out var error)).IsTrue();
        await Assert.That(error).IsEqualTo(default(StructError));
    }

    [Test]
    public async Task FromNullable_WithCustomError()
    {
        string? missing = null;
        string? present = "value";

        var err = Rop.FromNullable(missing, () => App("E_NULL"));
        var ok = Rop.FromNullable(present, () => App("E_NULL"));

        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_NULL");
        await Assert.That(ok.OrThrow()).IsEqualTo("value");
    }

    [Test]
    public async Task FromNullableValue_WithCustomError()
    {
        int? missing = null;
        int? present = 5;

        var err = Rop.FromNullableValue(missing, () => App("E_MISSING"));
        var ok = Rop.FromNullableValue(present, () => App("E_MISSING"));

        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_MISSING");
        await Assert.That(ok.OrThrow()).IsEqualTo(5);
    }

    [Test]
    public async Task SuccessIfAndFailureIf_WithCustomError()
    {
        var success = Rop.SuccessIf(true, 1, App("E_CONDITION"));
        var successFailure = Rop.SuccessIf(false, 1, App("E_CONDITION"));
        var failure = Rop.FailureIf(true, 2, App("E_REJECT"));
        var failureSuccess = Rop.FailureIf(false, 2, App("E_REJECT"));

        await Assert.That(success.OrThrow()).IsEqualTo(1);
        await Assert.That(successFailure.ErrOrThrow().Code).IsEqualTo("E_CONDITION");
        await Assert.That(failure.ErrOrThrow().Code).IsEqualTo("E_REJECT");
        await Assert.That(failureSuccess.OrThrow()).IsEqualTo(2);
    }

    [Test]
    public async Task SuccessIf_WithCustomError_DoesNotEvaluateTheFactoryOnSuccess()
    {
        var called = false;

        var result = Rop.SuccessIf(true, 1, () =>
        {
            called = true;
            return App();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task RequireNotNull_WithCustomError()
    {
        Result<string?, AppError> source =
            Rop.Ok<string, AppError>("x").MapOk<string, string?, AppError>(_ => null);

        var result = source.RequireNotNull(() => App("E_NULL", "name required"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_NULL");
    }

    [Test]
    public async Task RequireNotNull_WithCustomError_PassesTheOriginalErrorThrough()
    {
        Result<string?, AppError> source = Rop.Err<string?, AppError>(App("E_ORIGINAL"));

        var result = source.RequireNotNull(() => App("E_NULL"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_ORIGINAL");
    }

    [Test]
    public async Task Either_ToResult_WithCustomError()
    {
        Either<AppError, int> left = Either.Left<AppError, int>(App("E_LEFT"));
        Either<AppError, int> right = Either.Right<AppError, int>(42);

        var err = left.ToResult(error => error);
        var ok = right.ToResult(error => error);

        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_LEFT");
        await Assert.That(ok.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Validate_WithCustomError_AggregatesEveryErrorInArgumentOrder()
    {
        Result<int, AppError> first = Rop.Err<int, AppError>(App("E_A"));
        Result<int, AppError> second = Rop.Err<int, AppError>(App("E_B"));

        var validated = Rop.Validate(first, second, (a, b) => a + b);

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
            Rop.Err<int, AppError>(App("E_A")),
            Rop.Ok<int, AppError>(2),
            Rop.Err<int, AppError>(App("E_B")),
        ];

        var sequence = Rop.SequenceValidation(results);

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
            Rop.Err<int, AppError>(App("E_A")),
            Rop.Err<int, AppError>(App("E_B")),
        ];

        var single = Rop.SequenceValidation(results).ToSingleError();

        await Assert.That(single.ErrOrThrow().Code).IsEqualTo("E_A");
    }

    [Test]
    public async Task ToSingleError_WithCustomError_UsesTheSuppliedCombine()
    {
        Result<int, AppError>[] results =
        [
            Rop.Err<int, AppError>(App("E_A")),
            Rop.Err<int, AppError>(App("E_B")),
        ];

        var single = Rop.SequenceValidation(results)
            .ToSingleError(errors => new AppError("E_MULTIPLE", string.Join(";", errors.Select(error => error.Code))));

        await Assert.That(single.ErrOrThrow().Code).IsEqualTo("E_MULTIPLE");
        await Assert.That(single.ErrOrThrow().Message).IsEqualTo("E_A;E_B");
    }

    [Test]
    public async Task Linq_WithCustomError_ShortCircuitsOnTheErrorTrack()
    {
        var sum = from a in Rop.Ok<int, AppError>(1)
                  from b in Rop.Ok<int, AppError>(2)
                  select a + b;

        var failed = from a in Rop.Err<int, AppError>(App("E_LINQ"))
                     from b in Rop.Ok<int, AppError>(2)
                     select a + b;

        await Assert.That(sum.OrThrow()).IsEqualTo(3);
        await Assert.That(failed.ErrOrThrow().Code).IsEqualTo("E_LINQ");
    }

    [Test]
    public async Task Try_WithCustomError_ReturnsOk()
    {
        var result = Rop.Try(() => 42, _ => App("E_FAIL"), () => App("E_NULL"));

        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Try_WithCustomError_ConvertsExceptions()
    {
        var result = Rop.Try<int, AppError>(
            () => throw new FormatException("bad"),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_PARSE");
        await Assert.That(result.ErrOrThrow().Message).IsEqualTo("bad");
    }

    [Test]
    public async Task Try_WithCustomError_UsesTheNullResultFactory()
    {
        var called = 0;

        var result = Rop.Try<string, AppError>(
            () => null!,
            _ => App("E_FAIL"),
            () =>
            {
                called++;
                return App("E_NULL", "null result");
            });

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_NULL");
        await Assert.That(called).IsEqualTo(1);
    }

    [Test]
    public async Task Try_WithCustomError_RethrowsOperationCanceledException()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Try<int, AppError>(
                () => throw new OperationCanceledException(),
                _ => App(),
                () => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task Try_WithCustomError_GuardsEveryArgument()
    {
        var nullFunction = TestSupport.Catch(() =>
            Rop.Try<int, AppError>(null!, _ => App(), () => App()));
        var nullFactory = TestSupport.Catch(() =>
            Rop.Try<int, AppError>(() => 1, null!, () => App()));
        var nullResultFactory = TestSupport.Catch(() =>
            Rop.Try<int, AppError>(() => 1, _ => App(), null!));

        await Assert.That(nullFunction is ArgumentNullException).IsTrue();
        await Assert.That(nullFactory is ArgumentNullException).IsTrue();
        await Assert.That(nullResultFactory is ArgumentNullException).IsTrue();
    }

    [Test]
    public async Task Try_WithCustomError_ThrowsWhenTheFactoryReturnsNull()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Try<int, AppError>(
                () => throw new InvalidOperationException("boom"),
                _ => null!,
                () => App()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAction_WithCustomError_ReturnsUnit()
    {
        var executed = false;

        var result = Rop.Try(() => { executed = true; }, exception => App("E_ACTION", exception.Message));

        await Assert.That(executed).IsTrue();
        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OrThrow()).IsEqualTo(Unit.Value);
    }

    [Test]
    public async Task TryAction_WithCustomError_ConvertsExceptions()
    {
        var result = Rop.Try(
            () => throw new FormatException("bad"),
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_ACTION");
    }

    [Test]
    public async Task TrySwitch_WithCustomError_LiftsTheFunction()
    {
        var lifted = Rop.TrySwitch<string, int, AppError>(
            text => int.Parse(text, CultureInfo.InvariantCulture),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        var ok = lifted("12");
        var err = lifted("nope");

        await Assert.That(ok.OrThrow()).IsEqualTo(12);
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task TrySwitch_WithCustomError_UsesTheNullResultFactory()
    {
        var lifted = Rop.TrySwitch<string, string, AppError>(
            _ => null!,
            _ => App("E_FAIL"),
            () => App("E_NULL"));

        var result = lifted("x");

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_NULL");
    }

    [Test]
    public async Task TryAsync_WithCustomError_ReturnsOk()
    {
        var result = await Rop.TryAsync<int, AppError>(
            () => Task.FromResult(42),
            _ => App("E_FAIL"),
            () => App("E_NULL"));

        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task TryAsync_WithCustomError_ConvertsExceptions()
    {
        var result = await Rop.TryAsync<int, AppError>(
            () => Task.FromException<int>(new FormatException("bad")),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task TryAsync_WithCustomError_UsesTheNullResultFactory()
    {
        var result = await Rop.TryAsync<string, AppError>(
            () => Task.FromResult<string>(null!),
            _ => App("E_FAIL"),
            () => App("E_NULL", "null result"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_NULL");
    }

    [Test]
    public async Task TryAsync_WithCustomError_ThrowsForANullTask()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<int, AppError>(() => null!, _ => App(), () => App()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCustomError_ObservesAnAlreadyCancelledToken()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<int, AppError>(
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

        var task = Rop.TryAsync<int, AppError>(
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
        var result = await Rop.TryAsync<AppError>(
            () => Task.CompletedTask,
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OrThrow()).IsEqualTo(Unit.Value);
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ConvertsExceptions()
    {
        var result = await Rop.TryAsync<AppError>(
            () => Task.FromException(new InvalidOperationException("bad")),
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_ACTION");
    }

    [Test]
    public async Task TrySwitchAsync_WithCustomError_LiftsTheFunction()
    {
        var lifted = Rop.TrySwitchAsync<string, int, AppError>(
            text => Task.FromResult(int.Parse(text, CultureInfo.InvariantCulture)),
            exception => App("E_PARSE", exception.Message),
            () => App("E_NULL"));

        var ok = await lifted("12");
        var err = await lifted("nope");

        await Assert.That(ok.OrThrow()).IsEqualTo(12);
        await Assert.That(err.ErrOrThrow().Code).IsEqualTo("E_PARSE");
    }

    [Test]
    public async Task Parse_WithCustomError_ReturnsOk()
    {
        var result = Rop.Parse<int, AppError>("42", failure => new AppError(failure.Code, failure.Message));

        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Parse_WithCustomError_MapsTheCompleteDiagnosticPayload()
    {
        ParseFailure? captured = null;

        var result = Rop.Parse<int, AppError>("abc", failure =>
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

        Rop.Parse<int, AppError>(null, failure =>
        {
            nullFailure = failure;
            return App();
        });
        Rop.Parse<int, AppError>(string.Empty, failure =>
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

        Rop.Parse<int, AppError>("a\"b\nc", failure =>
        {
            escapedFailure = failure;
            return App();
        });
        Rop.Parse<int, AppError>(longInput, failure =>
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

        Rop.Parse<int, AppError>(
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

        var result = Rop.Parse<int, AppError>("42", _ =>
        {
            called = true;
            return App();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Parse_WithCustomError_GuardsTheFactoryAndCode()
    {
        var nullFactory = TestSupport.Catch(() => Rop.Parse<int, AppError>("1", null!));
        var blankCode = TestSupport.Catch(() => Rop.Parse<int, AppError>("1", _ => App(), " "));

        await Assert.That(nullFactory is ArgumentNullException).IsTrue();
        await Assert.That(blankCode is ArgumentException).IsTrue();
    }

    [Test]
    public async Task Parse_WithCustomError_ThrowsWhenTheFactoryReturnsNull()
    {
        var exception = TestSupport.Catch(() => Rop.Parse<int, AppError>("x", _ => null!));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task FailureIf_WithCustomError_DoesNotEvaluateTheFactoryOnSuccess()
    {
        var called = false;

        var result = Rop.FailureIf(false, 1, () =>
        {
            called = true;
            return App();
        });

        await Assert.That(called).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(1);
    }

    [Test]
    public async Task FailureIf_WithCustomError_UsesTheFactoryWhenTheConditionHolds()
    {
        var result = Rop.FailureIf(true, 1, () => App("E_REJECT"));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_REJECT");
    }

    [Test]
    public async Task TryAction_WithCustomError_RethrowsOperationCanceledException()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Try(() => throw new OperationCanceledException(), _ => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ConvertsSynchronousExceptions()
    {
        var result = await Rop.TryAsync<AppError>(
            () => throw new InvalidOperationException("sync"),
            exception => App("E_ACTION", exception.Message));

        await Assert.That(result.ErrOrThrow().Code).IsEqualTo("E_ACTION");
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ThrowsForANullTask()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<AppError>(() => null!, _ => App()));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_RethrowsOperationCanceledException()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<AppError>(() => throw new OperationCanceledException(), _ => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsyncAction_WithCustomError_ObservesCancellationWhileAwaiting()
    {
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var source = new CancellationTokenSource();

        var task = Rop.TryAsync<AppError>(() => pending.Task, _ => App(), source.Token);

        await source.CancelAsync();
        var exception = await TestSupport.CatchAsync(() => task);

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCustomError_RethrowsSynchronousOperationCanceledException()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<int, AppError>(
                () => throw new OperationCanceledException(),
                _ => App(),
                () => App()));

        await Assert.That(exception is OperationCanceledException).IsTrue();
    }

    [Test]
    public async Task Try_WithCustomError_PropagatesNullResultFactoryFailures()
    {
        var exception = TestSupport.Catch(() =>
            Rop.Try<string, AppError>(
                () => null!,
                _ => App(),
                () => throw new InvalidOperationException("factory bug")));

        await Assert.That(exception is InvalidOperationException).IsTrue();
    }

    [Test]
    public async Task TryAsync_WithCustomError_PropagatesNullResultFactoryFailures()
    {
        var exception = await TestSupport.CatchAsync(() =>
            Rop.TryAsync<string, AppError>(
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
        Result<int, RopError> ok = Rop.Ok(1);
        Result<int, RopError> err = Rop.Err<int>(Sample.Error());
        Result<string, RopError> nullable = Rop.FromNullable<string>(null, () => Sample.Error());
        Result<int, RopError> conditional = Rop.SuccessIf(true, 1, Sample.Error());
        Result<int, RopError> tried = Rop.Try(() => 1, _ => Sample.Error());
        var triedAsync = Rop.TryAsync(() => Task.FromResult(1), _ => Sample.Error());
        Result<int, RopError> validated = Rop.Validate(ok, ok, (a, b) => a + b).ToSingleError();
        Result<int, RopError> fromEither = Either.Right<string, int>(1).ToResult(_ => Sample.Error());

        Result<string?, RopError> nullSuccess = Rop.Ok<string, RopError>("x")
            .MapOk<string, string?, RopError>(_ => null);
        Result<string, RopError> required = nullSuccess.RequireNotNull(() => Sample.Error("E_NULL"));

        await Assert.That(ok.OrThrow()).IsEqualTo(1);
        await Assert.That(err.IsErr).IsTrue();
        await Assert.That(nullable.ErrOrThrow().Code).IsEqualTo("E_TEST");
        await Assert.That(conditional.OrThrow()).IsEqualTo(1);
        await Assert.That(tried.OrThrow()).IsEqualTo(1);
        await Assert.That((await triedAsync).OrThrow()).IsEqualTo(1);
        await Assert.That(validated.OrThrow()).IsEqualTo(2);
        await Assert.That(fromEither.OrThrow()).IsEqualTo(1);
        await Assert.That(required.ErrOrThrow().Code).IsEqualTo("E_NULL");
    }
}
