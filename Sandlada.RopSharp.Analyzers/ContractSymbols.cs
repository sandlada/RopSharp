using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sandlada.RopSharp.Analyzers;

internal sealed class ContractSymbols
{
    internal const string Namespace = "Sandlada.RopSharp";

    internal INamedTypeSymbol? Result2 { get; }
    internal INamedTypeSymbol? Either2 { get; }
    internal INamedTypeSymbol? Validation2 { get; }
    internal INamedTypeSymbol? ResultTask2 { get; }
    internal INamedTypeSymbol? OptionTask1 { get; }
    internal INamedTypeSymbol? ValidationTask2 { get; }
    internal INamedTypeSymbol? ParseFailure { get; }
    internal INamedTypeSymbol? RopError { get; }
    internal INamedTypeSymbol? ResultStatic { get; }
    internal INamedTypeSymbol? OptionStatic { get; }
    internal INamedTypeSymbol? Task { get; }
    internal INamedTypeSymbol? Task1 { get; }
    internal INamedTypeSymbol? ValueTask { get; }
    internal INamedTypeSymbol? ValueTask1 { get; }
    internal IAssemblySymbol? RopSharpAssembly { get; }
    internal IAssemblySymbol CompilationAssembly { get; }

    public ContractSymbols(Compilation compilation)
    {
        CompilationAssembly = compilation.Assembly;
        Result2 = compilation.GetTypeByMetadataName("Sandlada.RopSharp.Result`2");
        Either2 = compilation.GetTypeByMetadataName("Sandlada.RopSharp.Either`2");
        Validation2 = compilation.GetTypeByMetadataName("Sandlada.RopSharp.Validation`2");
        ResultTask2 = compilation.GetTypeByMetadataName("Sandlada.RopSharp.ResultTask`2");
        OptionTask1 = compilation.GetTypeByMetadataName("Sandlada.RopSharp.OptionTask`1");
        ValidationTask2 = compilation.GetTypeByMetadataName("Sandlada.RopSharp.ValidationTask`2");
        ParseFailure = compilation.GetTypeByMetadataName("Sandlada.RopSharp.ParseFailure");
        RopError = compilation.GetTypeByMetadataName("Sandlada.RopSharp.RopError");
        ResultStatic = compilation.GetTypeByMetadataName("Sandlada.RopSharp.Result");
        OptionStatic = compilation.GetTypeByMetadataName("Sandlada.RopSharp.Option");
        Task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        Task1 = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        ValueTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        ValueTask1 = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        RopSharpAssembly = Result2?.ContainingAssembly ?? RopError?.ContainingAssembly;
    }

    public bool HasRopSharp =>
        RopSharpAssembly is not null || string.Equals(CompilationAssembly.Name, "Sandlada.RopSharp", StringComparison.Ordinal);

    public bool IsContractType(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return false;
        }

        var original = named.OriginalDefinition;
        return SymbolEqualityComparer.Default.Equals(original, Result2)
            || SymbolEqualityComparer.Default.Equals(original, Either2)
            || SymbolEqualityComparer.Default.Equals(original, Validation2)
            || SymbolEqualityComparer.Default.Equals(original, ResultTask2)
            || SymbolEqualityComparer.Default.Equals(original, OptionTask1)
            || SymbolEqualityComparer.Default.Equals(original, ValidationTask2)
            || SymbolEqualityComparer.Default.Equals(original, ParseFailure);
    }

    public bool IsTaskLike(ITypeSymbol? type)
    {
        if (type is null || type.SpecialType == SpecialType.System_Void)
        {
            return false;
        }

        if (type is INamedTypeSymbol named)
        {
            var original = named.OriginalDefinition;
            return SymbolEqualityComparer.Default.Equals(original, Task)
                || SymbolEqualityComparer.Default.Equals(original, Task1)
                || SymbolEqualityComparer.Default.Equals(original, ValueTask)
                || SymbolEqualityComparer.Default.Equals(original, ValueTask1);
        }

        return false;
    }

    public bool IsAsyncComputationType(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return false;
        }

        var original = named.OriginalDefinition;
        return SymbolEqualityComparer.Default.Equals(original, ResultTask2)
            || SymbolEqualityComparer.Default.Equals(original, OptionTask1)
            || SymbolEqualityComparer.Default.Equals(original, ValidationTask2);
    }

    public bool IsSandladaRopSharpMethod(IMethodSymbol method)
    {
        var containingAssembly = method.ContainingAssembly;
        if (containingAssembly is null)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(containingAssembly, RopSharpAssembly)
            || SymbolEqualityComparer.Default.Equals(containingAssembly, CompilationAssembly);
    }

    public bool ExpectsSynchronousReturn(ITypeSymbol? parameterType)
    {
        if (parameterType is null)
        {
            return false;
        }

        parameterType = parameterType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        var invoke = GetDelegateInvokeMethod(parameterType);

        return invoke is not null && !IsTaskLike(invoke.ReturnType);
    }

    public static IMethodSymbol? GetDelegateInvokeMethod(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Delegate)
        {
            return type.GetMembers(WellKnownMemberNames.DelegateInvokeName).OfType<IMethodSymbol>().FirstOrDefault();
        }

        if (type is INamedTypeSymbol named && named.DelegateInvokeMethod is not null)
        {
            return named.DelegateInvokeMethod;
        }

        return null;
    }

    public static bool IsExcludedDefaultContext(IOperation operation)
    {
        for (var parent = operation.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent.Kind == OperationKind.ArrayInitializer)
            {
                return true;
            }
        }

        return false;
    }
}
