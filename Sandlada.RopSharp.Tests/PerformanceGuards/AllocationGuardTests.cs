using Sandlada.RopSharp;
using TUnit.Core;
using TUnit.Assertions;
using static Sandlada.RopSharp.Tests.PerformanceGuards.AllocationBudgets;

namespace Sandlada.RopSharp.Tests.PerformanceGuards;

/// <summary>
/// CI allocation gate for the hot paths. Zero-byte checks are exact; bounded checks cap bytes per call.
/// These run in every <c>dotnet test</c> and fail the build when a hot path starts allocating.
/// </summary>
public sealed class AllocationGuardTests
{
    private const int BoundedIterations = 10_000;

    private static readonly Result<int, RopError> Ok = Rop.Ok(42);
    private static readonly Result<int, RopError> Err = Rop.Err<int>(RopError.Create("E_GUARD", "guard"));
    private static readonly Option<int> Some = Opt.Some(42);
    private static readonly Option<int> None = Opt.None<int>();
    private static readonly Either<string, int> Right = Either.Right<string, int>(42);
    private static readonly RopError GuardError = RopError.Create("E_GUARD", "never");

    private static readonly Result<int, RopError>[] Oks16 =
        [.. Enumerable.Range(0, 16).Select(static value => Rop.Ok(value))];

    private static readonly Result<int, RopError>[] OkWithError16 =
        [.. Enumerable.Range(0, 16).Select(static value => value == 8 ? Rop.Err<int>(GuardError) : Rop.Ok(value))];

    private static readonly Result<int, RopError>[] AllErrors16 =
        [.. Enumerable.Range(0, 16).Select(static _ => Rop.Err<int>(GuardError))];

    private static readonly RopError[] Errors10 =
        [.. Enumerable.Range(0, 10).Select(static i => RopError.Create($"E_{i}", $"message {i}"))];

    // ---------------------------------------------------------------- zero-byte hot paths

    [Test]
    public async Task ResultMapOk_OnOk_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Ok.MapOk(static value => value + 1)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task ResultBindOk_OnOk_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Ok.BindOk(static value => Rop.Ok(value + 1))));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task ResultMapErr_OnErr_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Err.MapErr(static error => error)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task ResultMatch_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Ok.Match(static value => value, static _ => 0)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task ResultEnsure_OnOk_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Ok.Ensure(static value => value >= 0, GuardError)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task ResultTryGetErr_OnErr_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Err.TryGetErr(out RopError? _)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task OptionMapSome_OnSome_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Some.MapSome(static value => value + 1)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task OptionFilter_OnSome_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Some.Filter(static value => value > 0)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task OptionMapSome_OnNone_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(None.MapSome(static value => value + 1)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    [Test]
    public async Task EitherMapRight_OnRight_AllocatesNothing()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Right.MapRight(static value => value + 1)));

        await Assert.That(bytes).IsEqualTo(ZeroBytes);
    }

    // ---------------------------------------------------------------- bounded hot paths (bytes per call)

    [Test]
    public async Task ResultSequence_SuccessPath_StaysWithinBudget()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Rop.Sequence(Oks16)), iterations: BoundedIterations);
        await Assert.That(bytes).IsLessThanOrEqualTo(SequenceSuccess16PerCallBytes * BoundedIterations);
    }

    [Test]
    public async Task ResultSequence_ShortCircuitPath_StaysWithinBudget()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Rop.Sequence(OkWithError16)), iterations: BoundedIterations);
        await Assert.That(bytes).IsLessThanOrEqualTo(SequenceShortCircuit16PerCallBytes * BoundedIterations);
    }

    [Test]
    public async Task ResultSequenceValidation_AllErrors_StaysWithinBudget()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(Rop.SequenceValidation(AllErrors16)), iterations: BoundedIterations);
        await Assert.That(bytes).IsLessThanOrEqualTo(SequenceValidationAllErrors16PerCallBytes * BoundedIterations);
    }

    [Test]
    public async Task RopErrorCombine_Ten_StaysWithinBudget()
    {
        var bytes = AllocationMeter.BytesPerRun(_ => Sink.Consume(RopError.Combine(Errors10)), iterations: BoundedIterations);
        await Assert.That(bytes).IsLessThanOrEqualTo(RopErrorCombine10PerCallBytes * BoundedIterations);
    }
}
