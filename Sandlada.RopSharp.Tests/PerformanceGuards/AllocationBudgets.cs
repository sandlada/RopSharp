namespace Sandlada.RopSharp.Tests.PerformanceGuards;

/// <summary>
/// Allocation budgets for the hot paths. Zero-byte paths are exact guarantees. Bounded paths are measured once and
/// set to 1.2x the observed value (see docs/performance-budgets.md). Raising a budget requires an ADR or CHANGELOG entry.
/// </summary>
internal static class AllocationBudgets
{
    public const long ZeroBytes = 0;

    public const long SequenceSuccess16PerCallBytes = 327;
    public const long SequenceShortCircuit16PerCallBytes = 192;
    public const long SequenceValidationAllErrors16PerCallBytes = 653;
    public const long RopErrorCombine10PerCallBytes = 1_834;
}
