using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sandlada.RopSharp.Tests.Documentation;

/// <summary>
/// Guards the documentation coverage of the public API surface.
/// </summary>
/// <remarks>
/// Every public member of <c>Sandlada.RopSharp</c> must ship a worked example. This test enumerates the public
/// surface with Roslyn and fails when a member - a type, a method, an overload, a property, an operator or an
/// enum value - has no example attached.
/// </remarks>
public sealed class DocExampleCoverageTests
{
    [Test]
    public async Task EveryPublicMember_DocumentsAWorkingExample()
    {
        var compilation = BuildLibraryCompilation();

        var missing = new List<string>();

        foreach (var type in EnumeratePublicTypes(compilation))
        {
            foreach (var member in EnumeratePublicMembers(type))
            {
                var references = member.DeclaringSyntaxReferences.ToArray();

                if (references.Length == 0)
                {
                    // Implicitly declared members (a struct parameterless constructor, the enum backing field)
                    // carry no documentation of their own.
                    continue;
                }

                // A partial type is documented in one of its declarations; it is enough that one of them
                // carries the example.
                var documented = references.Any(reference =>
                    reference.GetSyntax()
                        .GetLeadingTrivia()
                        .Where(static trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                        .Select(static trivia => trivia.ToFullString())
                        .Any(static text => text.Contains(DocExampleExtractor.CodeOpenTag, StringComparison.Ordinal)));

                if (!documented)
                {
                    var reference = references[0];
                    missing.Add($"{Path.GetFileName(reference.SyntaxTree.FilePath)}: {Describe(member)}");
                }
            }
        }

        WriteInventory("public-api-missing.txt", missing);

        await Assert.That(missing.Count).IsEqualTo(0);
    }

    [Test]
    public async Task EveryPublicMember_ExampleIsWrappedInCData()
    {
        var compilation = BuildLibraryCompilation();

        var offenders = new List<string>();

        foreach (var type in EnumeratePublicTypes(compilation))
        {
            foreach (var member in EnumeratePublicMembers(type))
            {
                foreach (var reference in member.DeclaringSyntaxReferences)
            {
                var documentation = string.Concat(
                    reference.GetSyntax()
                        .GetLeadingTrivia()
                        .Where(static trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                        .Select(static trivia => trivia.ToFullString()));

                var open = documentation.IndexOf("<code>", StringComparison.Ordinal);

                if (open < 0)
                {
                    continue;
                }

                var close = documentation.IndexOf("</code>", open, StringComparison.Ordinal);
                var body = documentation.Substring(open, close + "</code>".Length - open);

                if (!body.Contains(DocExampleExtractor.CodeOpenTag, StringComparison.Ordinal)
                    || !body.Contains(DocExampleExtractor.CodeCloseTag, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetFileName(reference.SyntaxTree.FilePath)}: {Describe(member)}");
                }
            }
            }
        }

        await Assert.That(offenders).IsEmpty();
    }

    private static void WriteInventory(string fileName, IReadOnlyList<string> entries)
    {
        var directory = Path.Combine(RepositoryLayout.RootDirectory, "TestResults");
        Directory.CreateDirectory(directory);

        File.WriteAllLines(
            Path.Combine(directory, fileName),
            entries.OrderBy(static entry => entry, StringComparer.Ordinal));
    }

    private static CSharpCompilation BuildLibraryCompilation()
    {
        var trees = RepositoryLayout.LibrarySourceFiles()
            .Select(static file => CSharpSyntaxTree.ParseText(
                File.ReadAllText(file),
                new CSharpParseOptions(LanguageVersion.Latest),
                path: file))
            .ToArray();

        return CSharpCompilation.Create(
            "Sandlada.RopSharp.Documentation",
            trees,
            DocExampleCompilation.ReferenceAssemblies(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static IEnumerable<INamedTypeSymbol> EnumeratePublicTypes(CSharpCompilation compilation)
    {
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var tree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(tree);

            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (model.GetDeclaredSymbol(node) is not INamedTypeSymbol type)
                {
                    continue;
                }

                if (type.DeclaredAccessibility is not Accessibility.Public || type.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (type.IsTupleType)
                {
                    // A tuple is a return shape, not a type the caller can name.
                    continue;
                }

                if (type.ContainingType is not null && type.ContainingType.DeclaredAccessibility is not Accessibility.Public)
                {
                    continue;
                }

                if (!seen.Add(type))
                {
                    // A partial type is declared in several files; its members must be reported only once.
                    continue;
                }

                yield return type;
            }
        }
    }

    private static IEnumerable<ISymbol> EnumeratePublicMembers(INamedTypeSymbol type)
    {
        yield return type;

        foreach (var member in type.GetMembers())
        {
            if (member.DeclaredAccessibility is not Accessibility.Public)
            {
                continue;
            }

            if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } method
                && method.IsImplicitlyDeclared)
            {
                continue;
            }

            if (member is IMethodSymbol
                {
                    MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet
                        or MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise,
                })
            {
                // Accessors are documented together with their property or event.
                continue;
            }

            yield return member;
        }
    }

    private static string Describe(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol type => $"{type.ToDisplayString()} (type)",
        IMethodSymbol method => $"{method.ToDisplayString()} (method)",
        IPropertySymbol property => $"{property.ToDisplayString()} (property)",
        IFieldSymbol field => $"{field.ToDisplayString()} (field)",
        _ => symbol.ToDisplayString(),
    };
}
