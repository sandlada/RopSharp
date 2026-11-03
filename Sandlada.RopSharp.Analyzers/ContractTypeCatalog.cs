using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sandlada.RopSharp.Analyzers;

internal static class ContractTypeCatalog
{
    internal const string Namespace = "Sandlada.RopSharp";

    internal static bool IsContractType(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol named)
        {
            return false;
        }

        if (!string.Equals(named.ContainingNamespace?.ToDisplayString(), Namespace, StringComparison.Ordinal))
        {
            return false;
        }

        return named.Name switch
        {
            "ParseFailure" when named.TypeParameters.Length == 0 && named.TypeKind == TypeKind.Struct => true,
            "Result" when named.TypeParameters.Length == 2 && named.TypeKind == TypeKind.Struct => true,
            "Either" when named.TypeParameters.Length == 2 && named.TypeKind == TypeKind.Struct => true,
            "Validation" when named.TypeParameters.Length == 2 && named.TypeKind == TypeKind.Struct => true,
            "ResultTask" when named.TypeParameters.Length == 2 && named.TypeKind == TypeKind.Struct => true,
            "OptionTask" when named.TypeParameters.Length == 1 && named.TypeKind == TypeKind.Struct => true,
            "ValidationTask" when named.TypeParameters.Length == 2 && named.TypeKind == TypeKind.Struct => true,
            _ => false,
        };
    }

    internal static bool IsTaskLike(ITypeSymbol? type)
    {
        if (type is null || type.SpecialType == SpecialType.System_Void)
        {
            return false;
        }

        if (type is INamedTypeSymbol named)
        {
            var original = named.OriginalDefinition;
            if (string.Equals(original.Name, "Task", StringComparison.Ordinal)
                && string.Equals(original.ContainingNamespace?.ToDisplayString(), "System.Threading.Tasks", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(original.Name, "ValueTask", StringComparison.Ordinal)
                && string.Equals(original.ContainingNamespace?.ToDisplayString(), "System.Threading.Tasks", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool ExpectsSynchronousReturn(ITypeSymbol? parameterType)
    {
        if (parameterType is null)
        {
            return false;
        }

        parameterType = parameterType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        var invoke = GetDelegateInvokeMethod(parameterType);

        return invoke is not null && !IsTaskLike(invoke.ReturnType);
    }

    internal static IMethodSymbol? GetDelegateInvokeMethod(ITypeSymbol type)
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

    internal static bool IsExcludedDefaultContext(IOperation operation)
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
