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
            compilationContext.RegisterOperationAction(ctx => AnalyzeDeconstructionAssignment(ctx, symbols), OperationKind.DeconstructionAssignment);
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
                case OperationKind.AnonymousFunction:
                case OperationKind.LocalFunction:
                    return false;
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

        if (!symbols.IsDiscardContractType(type))
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

        if (!symbols.IsDiscardContractType(type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            RopDiagnosticDescriptors.DiscardedContractValue,
            discard.Syntax.GetLocation(),
            type!.Name));
    }

    private static void AnalyzeDeconstructionAssignment(OperationAnalysisContext context, ContractSymbols symbols)
    {
        if (context.Operation is not IDeconstructionAssignmentOperation deconstruction)
        {
            return;
        }

        CheckDeconstructionTarget(context, symbols, deconstruction.Target, deconstruction.Value);
    }

    private static void CheckDeconstructionTarget(
        OperationAnalysisContext context,
        ContractSymbols symbols,
        IOperation target,
        IOperation? value)
    {
        while (value is IConversionOperation { IsImplicit: true, Operand: var inner })
        {
            value = inner;
        }

        if (target is IDiscardOperation discard)
        {
            var discardedType = value?.Type ?? discard.Type;
            if (symbols.IsDiscardContractType(discardedType))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    RopDiagnosticDescriptors.DiscardedContractValue,
                    discard.Syntax.GetLocation(),
                    discardedType!.Name));
            }

            return;
        }

        if (target is not ITupleOperation targetTuple)
        {
            return;
        }

        if (value is ITupleOperation valueTuple && valueTuple.Elements.Length == targetTuple.Elements.Length)
        {
            for (var i = 0; i < targetTuple.Elements.Length; i++)
            {
                CheckDeconstructionTarget(context, symbols, targetTuple.Elements[i], valueTuple.Elements[i]);
            }

            return;
        }

        if (value?.Type is INamedTypeSymbol { IsTupleType: true, TupleElements: var tupleElements }
            && tupleElements.Length == targetTuple.Elements.Length)
        {
            for (var i = 0; i < targetTuple.Elements.Length; i++)
            {
                if (targetTuple.Elements[i] is IDiscardOperation elemDiscard
                    && symbols.IsDiscardContractType(tupleElements[i].Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        RopDiagnosticDescriptors.DiscardedContractValue,
                        elemDiscard.Syntax.GetLocation(),
                        tupleElements[i].Type.Name));
                }
            }
        }
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

        if (argument.Value is not (IAnonymousFunctionOperation or IMethodReferenceOperation or IDelegateCreationOperation
            or ILocalReferenceOperation or IFieldReferenceOperation or IPropertyReferenceOperation))
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

        if (IsKnownSyncCombinator(method.Name))
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

    private static bool IsKnownSyncCombinator(string name) =>
        name is "MapOk" or "BindOk" or "MapOkValue" or "MapSome" or "BindSome" or "MapSomeValue"
            or "Map" or "MapErr" or "BindErr" or "BindErrValue" or "BindNone" or "BindNoneValue"
            or "MapLeft" or "MapRight" or "BindLeft" or "BindRight"
            or "Switch" or "SwitchFirst" or "TrySwitch" or "Match" or "MatchFirst"
            or "Choose" or "TeeOk" or "TeeErr" or "TeeSome" or "TeeNone"
            or "Ensure" or "FailIf" or "ThenEnsure" or "Collapse"
            or "Select" or "SelectMany" or "Where"
            or "Try" or "SuccessIf" or "FailureIf" or "RequireSome" or "RequireNotNull" or "RequireNotNullValue"
            or "FromNullable" or "FromNullableValue" or "When" or "Zip" or "Combine" or "Sequence" or "Traverse"
            or "Filter" or "Execute" or "ExecuteNoValue";

    private static bool IsAsyncBoundaryMethod(IMethodSymbol method) =>
        method.Name.EndsWith("Async", StringComparison.Ordinal);

    private static bool ReturnsTaskLike(IOperation? operation, ContractSymbols symbols)
    {
        switch (operation)
        {
            case null:
                return false;

            case IAnonymousFunctionOperation anonymous:
                if (symbols.IsTaskLike(anonymous.Symbol.ReturnType)
                    || (anonymous.Symbol.IsAsync && anonymous.Symbol.ReturnsVoid))
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

            case IMethodReferenceOperation { Method: var referencedMethod }:
                return symbols.IsTaskLike(referencedMethod.ReturnType)
                    || (referencedMethod.IsAsync && referencedMethod.ReturnsVoid);

            case IConversionOperation conversion:
                return ReturnsTaskLike(conversion.Operand, symbols);

            case IDelegateCreationOperation { Target: var target }:
                return ReturnsTaskLike(target, symbols);

            case IConditionalOperation conditional:
                return ReturnsTaskLike(conditional.WhenTrue, symbols)
                    || ReturnsTaskLike(conditional.WhenFalse, symbols);

            case ILocalReferenceOperation or IFieldReferenceOperation or IPropertyReferenceOperation:
                var invoke = operation.Type is null ? null : ContractSymbols.GetDelegateInvokeMethod(operation.Type);
                return invoke is not null && symbols.IsTaskLike(invoke.ReturnType);

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
        while (operation is IConversionOperation { Operand: var operand, IsImplicit: true })
        {
            operation = operand;
        }

        if (operation?.ConstantValue is { HasValue: true, Value: string text })
        {
            return text;
        }

        return null;
    }
}
