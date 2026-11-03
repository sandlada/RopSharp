using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Sandlada.RopSharp.Analyzers;

/// <summary>Roslyn analyzers that enforce the Sandlada.RopSharp usage contract.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RopSharpContractAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex ErrorCodePattern = new(
        "^E_[A-Z0-9_]+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            RopDiagnosticDescriptors.UninitializedContractValue,
            RopDiagnosticDescriptors.DiscardedContractValue,
            RopDiagnosticDescriptors.TaskLikeSyncDelegate,
            RopDiagnosticDescriptors.InvalidErrorCode);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeDefaultValue, OperationKind.DefaultValue);
        context.RegisterOperationAction(AnalyzeObjectCreation, OperationKind.ObjectCreation);
        context.RegisterOperationAction(AnalyzeExpressionStatement, OperationKind.ExpressionStatement);
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeDefaultValue(OperationAnalysisContext context)
    {
        if (context.Operation is not IDefaultValueOperation defaultValue
            || ContractTypeCatalog.IsExcludedDefaultContext(defaultValue))
        {
            return;
        }

        if (!ContractTypeCatalog.IsContractType(defaultValue.Type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.UninitializedContractValue,
            defaultValue.Syntax.GetLocation(),
            defaultValue.Type!.Name));
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context)
    {
        if (context.Operation is not IObjectCreationOperation creation
            || creation.Constructor is null
            || creation.Constructor.Parameters.Length != 0
            || creation.Arguments.Length != 0)
        {
            return;
        }

        if (!ContractTypeCatalog.IsContractType(creation.Type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.UninitializedContractValue,
            creation.Syntax.GetLocation(),
            creation.Type!.Name));
    }

    private static bool IsUsedByParent(IOperation operation)
    {
        for (var parent = operation.Parent; parent is not null; parent = parent.Parent)
        {
            switch (parent.Kind)
            {
                case OperationKind.SimpleAssignment:
                case OperationKind.CompoundAssignment:
                case OperationKind.VariableDeclarator:
                case OperationKind.Return:
                case OperationKind.YieldReturn:
                case OperationKind.Argument:
                case OperationKind.ArrayElementReference:
                    return true;
            }
        }

        return false;
    }

    private static void AnalyzeExpressionStatement(OperationAnalysisContext context)
    {
        if (context.Operation is not IExpressionStatementOperation statement)
        {
            return;
        }

        if (statement.Operation is IAssignmentOperation)
        {
            return;
        }

        var value = statement.Operation;

        if (IsUsedByParent(value))
        {
            return;
        }

        var type = value?.Type;

        if (!ContractTypeCatalog.IsContractType(type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.DiscardedContractValue,
            statement.Syntax.GetLocation(),
            type!.Name));
    }

    private static void AnalyzeAssignment(OperationAnalysisContext context)
    {
        if (context.Operation is not IAssignmentOperation { Target: IDiscardOperation discard })
        {
            return;
        }

        var type = context.Operation.Type;

        if (!ContractTypeCatalog.IsContractType(type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.DiscardedContractValue,
            discard.Syntax.GetLocation(),
            type!.Name));
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        AnalyzeCreateInvocation(context, invocation);

        foreach (var argument in invocation.Arguments)
        {
            AnalyzeTaskLikeArgument(context, argument, invocation);
        }
    }

    private static void AnalyzeCreateInvocation(OperationAnalysisContext context, IInvocationOperation invocation)
    {
        if (invocation.TargetMethod.Name != "Create"
            || invocation.TargetMethod.ContainingType?.Name != "RopError"
            || !string.Equals(
                invocation.TargetMethod.ContainingType.ContainingNamespace?.ToDisplayString(),
                ContractTypeCatalog.Namespace,
                StringComparison.Ordinal))
        {
            return;
        }

        if (invocation.Arguments.Length == 0)
        {
            return;
        }

        var code = TryGetCompileTimeString(invocation.Arguments[0].Value);

        if (code is null || ErrorCodePattern.IsMatch(code))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.InvalidErrorCode,
            invocation.Arguments[0].Syntax.GetLocation(),
            code));
    }

    private static void AnalyzeTaskLikeArgument(
        OperationAnalysisContext context,
        IArgumentOperation argument,
        IInvocationOperation invocation)
    {
        if (argument.Parameter is null)
        {
            return;
        }

        if (argument.Value is IParameterReferenceOperation)
        {
            return;
        }

        if (argument.Value is not (IAnonymousFunctionOperation or IMethodReferenceOperation or IDelegateCreationOperation))
        {
            return;
        }

        var method = invocation.TargetMethod;

        if (IsAsyncBoundaryMethod(method) || !IsSandladaRopSharpMethod(method))
        {
            return;
        }

        if (!ReturnsTaskLike(argument.Value))
        {
            return;
        }

        if (ContractTypeCatalog.ExpectsSynchronousReturn(argument.Parameter.Type))
        {
            ReportTaskLikeSyncDelegate(context, argument, method);
            return;
        }

        if (IsKnownSyncCombinator(method.Name) && !IsAsyncComputationType(GetReceiverType(invocation)))
        {
            ReportTaskLikeSyncDelegate(context, argument, method);
        }
    }

    private static void ReportTaskLikeSyncDelegate(
        OperationAnalysisContext context,
        IArgumentOperation argument,
        IMethodSymbol method) =>
        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.TaskLikeSyncDelegate,
            argument.Syntax.GetLocation(),
            method.Name));

    private static ITypeSymbol? GetReceiverType(IInvocationOperation invocation)
    {
        if (invocation.Instance?.Type is { } instanceType)
        {
            return instanceType;
        }

        if (invocation.TargetMethod.IsExtensionMethod && invocation.Arguments.Length > 0)
        {
            return invocation.Arguments[0].Value?.Type;
        }

        return invocation.TargetMethod.ContainingType;
    }

    private static bool IsAsyncComputationType(ITypeSymbol? type) =>
        type is INamedTypeSymbol named
        && string.Equals(named.ContainingNamespace?.ToDisplayString(), ContractTypeCatalog.Namespace, StringComparison.Ordinal)
        && named is { TypeKind: TypeKind.Struct, Name: "ResultTask" or "OptionTask" or "ValidationTask" };

    private static bool IsKnownSyncCombinator(string name) =>
        name is "MapOk" or "BindOk" or "MapSome" or "BindSome" or "Map" or "MapErr" or "MapLeft" or "MapRight"
            or "Switch" or "TrySwitch" or "Choose" or "TeeOk" or "TeeErr" or "TeeSome" or "TeeNone";

    private static bool IsSandladaRopSharpMethod(IMethodSymbol method) =>
        string.Equals(method.ContainingNamespace?.ToDisplayString(), ContractTypeCatalog.Namespace, StringComparison.Ordinal)
        || string.Equals(method.ContainingType?.ContainingNamespace?.ToDisplayString(), ContractTypeCatalog.Namespace, StringComparison.Ordinal);

    private static bool IsAsyncBoundaryMethod(IMethodSymbol method) =>
        method.Name.EndsWith("Async", StringComparison.Ordinal);

    private static bool ReturnsTaskLike(IOperation? operation)
    {
        switch (operation)
        {
            case null:
                return false;
            case IAnonymousFunctionOperation anonymous:
                if (ContractTypeCatalog.IsTaskLike(anonymous.Symbol.ReturnType))
                {
                    return true;
                }

                return ReturnsTaskLike(anonymous.Body);
            case IReturnOperation { ReturnedValue: var returned }:
                return ReturnsTaskLike(returned);
            case IMethodReferenceOperation { Method.ReturnType: var methodReturn }:
                return ContractTypeCatalog.IsTaskLike(methodReturn);
            case IConversionOperation conversion:
                return ReturnsTaskLike(conversion.Operand);
            case IDelegateCreationOperation { Target: var target }:
                return ReturnsTaskLike(target);
            default:
                return false;
        }
    }

    private static string? TryGetCompileTimeString(IOperation? operation)
    {
        operation = operation switch
        {
            IConversionOperation { Operand: var operand, IsImplicit: true } => operand,
            _ => operation,
        };

        if (operation is ILiteralOperation { ConstantValue: { HasValue: true, Value: string text } })
        {
            return text;
        }

        return null;
    }
}
