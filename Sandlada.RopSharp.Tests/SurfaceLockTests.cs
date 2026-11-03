using System.Reflection;
using System.Runtime.CompilerServices;
using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Locks the public surface after the 0.0.2 contract (no stray async-on-Task extensions, fixed type set).
/// </summary>
public sealed class SurfaceLockTests
{
    private static readonly Assembly Library = typeof(Result).Assembly;

    private static readonly HashSet<string> AllowedPublicTypes =
    [
        "Sandlada.RopSharp.Either",
        "Sandlada.RopSharp.Either<TLeft, TRight>",
        "Sandlada.RopSharp.EitherBridgeExtensions",
        "Sandlada.RopSharp.Option",
        "Sandlada.RopSharp.Option<T>",
        "Sandlada.RopSharp.OptionBridgeExtensions",
        "Sandlada.RopSharp.OptionEnumerableExtensions",
        "Sandlada.RopSharp.OptionExtensions",
        "Sandlada.RopSharp.OptionLinqExtensions",
        "Sandlada.RopSharp.OptionNullableExtensions",
        "Sandlada.RopSharp.OptionNullableValueExtensions",
        "Sandlada.RopSharp.OptionTask",
        "Sandlada.RopSharp.OptionTask<T>",
        "Sandlada.RopSharp.OptionTaskAwaiter<T>",
        "Sandlada.RopSharp.OptionTaskBridgeExtensions",
        "Sandlada.RopSharp.ParseFailure",
        "Sandlada.RopSharp.Result",
        "Sandlada.RopSharp.Result<TOk, TErr>",
        "Sandlada.RopSharp.ResultEnsureExtensions",
        "Sandlada.RopSharp.ResultExtensions",
        "Sandlada.RopSharp.ResultLinqExtensions",
        "Sandlada.RopSharp.ResultTask",
        "Sandlada.RopSharp.ResultTask<TOk, TErr>",
        "Sandlada.RopSharp.ResultTaskAwaiter<TOk, TErr>",
        "Sandlada.RopSharp.ResultTaskBridgeExtensions",
        "Sandlada.RopSharp.ResultTaskLinqExtensions",
        "Sandlada.RopSharp.ResultTerminalExtensions",
        "Sandlada.RopSharp.RopError",
        "Sandlada.RopSharp.RopErrorKind",
        "Sandlada.RopSharp.Unit",
        "Sandlada.RopSharp.Validation",
        "Sandlada.RopSharp.Validation<T, TErr>",
        "Sandlada.RopSharp.ValidationExtensions",
        "Sandlada.RopSharp.ValidationTask",
        "Sandlada.RopSharp.ValidationTask<T, TErr>",
        "Sandlada.RopSharp.ValidationTaskAwaiter<T, TErr>",
    ];

    private static readonly HashSet<string> AllowedTaskBridgeExtensionNames =
    [
        nameof(ResultTaskBridgeExtensions.ToResultTask),
        nameof(OptionTaskBridgeExtensions.ToOptionTask),
    ];

    [Test]
    public async Task PublicMethods_DoNotUseOverloadResolutionPriority()
    {
        var offenders = Library.GetExportedTypes()
            .SelectMany(static type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(static method => method.GetCustomAttributes(typeof(OverloadResolutionPriorityAttribute), inherit: false).Length > 0)
            .Select(static method => $"{method.DeclaringType!.Name}.{method.Name}")
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        await Assert.That(offenders).IsEmpty();
    }

    [Test]
    public async Task NoTaskCarrierExtensionMethodsExceptExplicitBridges()
    {
        var offenders = new List<string>();

        foreach (var type in Library.GetExportedTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (!method.IsDefined(typeof(ExtensionAttribute), inherit: false))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    continue;
                }

                if (!IsTaskCarrier(parameters[0].ParameterType))
                {
                    continue;
                }

                if (AllowedTaskBridgeExtensionNames.Contains(method.Name))
                {
                    continue;
                }

                offenders.Add($"{type.Name}.{method.Name}({parameters[0].ParameterType.Name})");
            }
        }

        await Assert.That(offenders.OrderBy(static x => x, StringComparer.Ordinal).ToArray()).IsEmpty();
    }

    [Test]
    public async Task ValidationTypes_HaveNoPublicBindMethods()
    {
        var offenders = Library.GetExportedTypes()
            .Where(static type => type.Name is "Validation" or "ValidationTask" || type.Name.StartsWith("Validation`", StringComparison.Ordinal))
            .SelectMany(static type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(static method => method.Name.StartsWith("Bind", StringComparison.Ordinal))
            .Select(static method => $"{method.DeclaringType!.Name}.{method.Name}")
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        await Assert.That(offenders).IsEmpty();
    }

    [Test]
    public async Task EveryPublicType_IsOnTheAllowList()
    {
        var actual = Library.GetExportedTypes()
            .Select(static type => GetAllowListName(type))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        var unexpected = actual.Where(name => !AllowedPublicTypes.Contains(name)).ToArray();
        var missing = AllowedPublicTypes.Where(name => !actual.Contains(name)).ToArray();

        await Assert.That(unexpected).IsEmpty();
        await Assert.That(missing).IsEmpty();
    }

    private static bool IsTaskCarrier(Type parameterType)
    {
        if (!parameterType.IsGenericType || parameterType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            return false;
        }

        var carrier = parameterType.GetGenericArguments()[0];
        if (!carrier.IsGenericType)
        {
            return false;
        }

        var definition = carrier.GetGenericTypeDefinition();
        return definition == typeof(Result<,>) || definition == typeof(Option<>) || definition == typeof(Validation<,>);
    }

    private static string GetAllowListName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.FullName!;
        }

        var name = type.GetGenericTypeDefinition().FullName!;
        var args = string.Join(", ", type.GetGenericArguments().Select(static arg => arg.Name));
        return $"{name.Split('`')[0]}<{args}>";
    }
}
