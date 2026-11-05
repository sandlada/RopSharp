using System;
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

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var symbols = new ContractSymbols(compilationContext.Compilation);
            if (!symbols.HasRopSharp)
            {
                return;
            }

            compilationContext.RegisterOperationAction(ctx => AnalyzeDefaultValue(ctx, symbols), OperationKind.DefaultValue);
            compilationContext.RegisterOperationAction(ctx => AnalyzeObjectCreation(ctx, symbols), OperationKind.ObjectCreation);
            compilationContext.RegisterOperationAction(ctx => AnalyzeExpressionStatement(ctx, symbols), OperationKind.ExpressionStatement);
            compilationContext.RegisterOperationAction(ctx => AnalyzeAssignment(ctx, symbols), OperationKind.SimpleAssignment);
            compilationContext.RegisterOperationAction(ctx => AnalyzeInvocation(ctx, symbols), OperationKind.Invocation);
        });
    }

    private static void AnalyzeDefaultValue(OperationAnalysisContext context, ContractSymbols symbols)
    {
        if (context.Operation is not IDefaultValueOperation defaultValue
            || ContractSymbols.IsExcludedDefaultContext(defaultValue))
        {
            return;
        }

        if (!symbols.IsContractType(defaultValue.Type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.UninitializedContractValue,
            defaultValue.Syntax.GetLocation(),
            defaultValue.Type!.Name));
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context, ContractSymbols symbols)
    {
        if (context.Operation is not IObjectCreationOperation creation)
        {
            return;
        }

        if (creation.Constructor is not null
            && creation.Constructor.Parameters.Length == 0
            && creation.Arguments.Length == 0)
        {
            if (symbols.IsContractType(creation.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    RopDiagnosticDescriptors.UninitializedContractValue,
                    creation.Syntax.GetLocation(),
                    creation.Type!.Name));
            }

            return;
        }

        if (SymbolEqualityComparer.Default.Equals(creation.Type?.OriginalDefinition, symbols.ParseFailure))
        {
            foreach (var argument in creation.Arguments)
            {
                if (argument.Parameter?.Name == "code")
                {
                    CheckErrorCodeArgument(context, argument);
                }
            }
        }
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

    private static void AnalyzeExpressionStatement(OperationAnalysisContext context, ContractSymbols symbols)
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

        if (!symbols.IsContractType(type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.DiscardedContractValue,
            statement.Syntax.GetLocation(),
            type!.Name));
    }

    private static void AnalyzeAssignment(OperationAnalysisContext context, ContractSymbols symbols)
    {
        if (context.Operation is not IAssignmentOperation { Target: IDiscardOperation discard })
        {
            return;
        }

        var type = context.Operation.Type;

        if (!symbols.IsContractType(type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.DiscardedContractValue,
            discard.Syntax.GetLocation(),
            type!.Name));
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, ContractSymbols symbols)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        AnalyzeErrorCodeInvocation(context, invocation, symbols);

        foreach (var argument in invocation.Arguments)
        {
            AnalyzeTaskLikeArgument(context, argument, invocation, symbols);
        }
    }

    private static void AnalyzeErrorCodeInvocation(
        OperationAnalysisContext context,
        IInvocationOperation invocation,
        ContractSymbols symbols)
    {
        var method = invocation.TargetMethod;
        if (!symbols.IsSandladaRopSharpMethod(method))
        {
            return;
        }

        var containingType = method.ContainingType?.OriginalDefinition;
        var isTargetMethod =
            (SymbolEqualityComparer.Default.Equals(containingType, symbols.RopError) && method.Name is "Create" or "FromException" or "WithCode")
            || (SymbolEqualityComparer.Default.Equals(containingType, symbols.ResultStatic) && method.Name is "Try" or "TryAsync" or "Parse");

        if (!isTargetMethod)
        {
            return;
        }

        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter?.Name == "code")
            {
                CheckErrorCodeArgument(context, argument);
            }
        }
    }

    private static void CheckErrorCodeArgument(OperationAnalysisContext context, IArgumentOperation argument)
    {
        if (argument.IsImplicit)
        {
            return;
        }

        var code = TryGetCompileTimeString(argument.Value);
        if (code is null || ErrorCodePattern.IsMatch(code))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.InvalidErrorCode,
            argument.Syntax.GetLocation(),
            code));
    }

    private static void AnalyzeTaskLikeArgument(
        OperationAnalysisContext context,
        IArgumentOperation argument,
        IInvocationOperation invocation,
        ContractSymbols symbols)
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

        if (IsAsyncBoundaryMethod(method) || !symbols.IsSandladaRopSharpMethod(method))
        {
            return;
        }

        if (!ReturnsTaskLike(argument.Value, symbols))
        {
            return;
        }

        if (symbols.ExpectsSynchronousReturn(argument.Parameter.Type))
        {
            ReportTaskLikeSyncDelegate(context, argument, method);
            return;
        }

        if (IsKnownSyncCombinator(method.Name) && !symbols.IsAsyncComputationType(GetReceiverType(invocation)))
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

    private static bool IsKnownSyncCombinator(string name) =>
        name is "MapOk" or "BindOk" or "MapSome" or "BindSome" or "Map" or "MapErr" or "MapLeft" or "MapRight"
            or "Switch" or "TrySwitch" or "Choose" or "TeeOk" or "TeeErr" or "TeeSome" or "TeeNone";

    private static bool IsAsyncBoundaryMethod(IMethodSymbol method) =>
        method.Name.EndsWith("Async", StringComparison.Ordinal);

    private static bool ReturnsTaskLike(IOperation? operation, ContractSymbols symbols)
    {
        switch (operation)
        {
            case null:
                return false;

            case IAnonymousFunctionOperation anonymous:
                if (symbols.IsTaskLike(anonymous.Symbol.ReturnType))
                {
                    return true;
                }

                return ReturnsTaskLike(anonymous.Body, symbols);

            case IBlockOperation block:
                return BlockReturnsTaskLike(block, symbols);

            case IReturnOperation { ReturnedValue: var returned }:
                if (returned is not null && symbols.IsTaskLike(returned.Type))
                {
                    return true;
                }
                return ReturnsTaskLike(returned, symbols);

            case IMethodReferenceOperation { Method.ReturnType: var methodReturn }:
                return symbols.IsTaskLike(methodReturn);

            case IConversionOperation conversion:
                return ReturnsTaskLike(conversion.Operand, symbols);

            case IDelegateCreationOperation { Target: var target }:
                return ReturnsTaskLike(target, symbols);

            case IInvocationOperation invocation:
                return symbols.IsTaskLike(invocation.Type);

            default:
                if (operation.Type is { } opType && symbols.IsTaskLike(opType))
                {
                    return true;
                }
                return false;
        }
    }

    private static bool BlockReturnsTaskLike(IBlockOperation block, ContractSymbols symbols)
    {
        foreach (var op in block.Operations)
        {
            switch (op)
            {
                case IReturnOperation ret:
                    if (ReturnsTaskLike(ret, symbols))
                    {
                        return true;
                    }
                    break;

                case IBlockOperation nestedBlock:
                    if (BlockReturnsTaskLike(nestedBlock, symbols))
                    {
                        return true;
                    }
                    break;

                case IAnonymousFunctionOperation or ILocalFunctionOperation:
                    break;

                default:
                    foreach (var descendant in op.Descendants())
                    {
                        if (descendant is IAnonymousFunctionOperation or ILocalFunctionOperation)
                        {
                            continue;
                        }

                        if (descendant is IReturnOperation childRet && ReturnsTaskLike(childRet, symbols))
                        {
                            return true;
                        }
                    }
                    break;
            }
        }

        return false;
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
