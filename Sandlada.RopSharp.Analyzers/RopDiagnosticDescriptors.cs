using Microsoft.CodeAnalysis;

namespace Sandlada.RopSharp.Analyzers;

internal static class RopDiagnosticDescriptors
{
    internal const string Category = "Sandlada.RopSharp";

    internal static readonly DiagnosticDescriptor UninitializedContractValue = new(
        id: "ROP0001",
        title: "Uninitialized contract value",
        messageFormat: "Contract type '{0}' must not be created with default or a parameterless constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Result, Either, Validation, ParseFailure and the async computation structs must not be created with default or an empty constructor.");

    internal static readonly DiagnosticDescriptor DiscardedContractValue = new(
        id: "ROP0002",
        title: "Discarded contract value",
        messageFormat: "The {0} value is unused. Bind it with a combinator or assign it to a variable.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Contract values must not appear as bare expression statements or be assigned to a discard.");

    internal static readonly DiagnosticDescriptor TaskLikeSyncDelegate = new(
        id: "ROP0003",
        title: "Task-like value in a synchronous combinator",
        messageFormat: "The delegate passed to '{0}' returns a task-like type. Use the asynchronous overload instead.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Synchronous Func parameters must not receive lambdas or method groups that return Task, Task<T>, ValueTask or ValueTask<T>.");

    internal static readonly DiagnosticDescriptor InvalidErrorCode = new(
        id: "ROP0004",
        title: "Invalid RopError code",
        messageFormat: "The error code '{0}' is not stable. Use a compile-time string matching ^E_[A-Z0-9_]+$.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "RopError.Create expects a stable machine-readable code beginning with E_.");
}
