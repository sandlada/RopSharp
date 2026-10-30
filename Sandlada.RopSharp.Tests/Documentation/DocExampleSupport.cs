using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sandlada.RopSharp.Tests.Documentation;

/// <summary>
/// Locates the repository files the documentation tests need.
/// </summary>
internal static class RepositoryLayout
{
    private static readonly Lazy<string> Root = new(FindRepositoryRoot, isThreadSafe: true);

    /// <summary>Gets the absolute path of the repository root.</summary>
    internal static string RootDirectory => Root.Value;

    /// <summary>Gets the directory holding the library sources.</summary>
    internal static string LibrarySourceDirectory => Path.Combine(RootDirectory, "Sandlada.RopSharp");

    /// <summary>Gets the file holding the shared documentation domain model.</summary>
    internal static string DomainSourceFile =>
        Path.Combine(RootDirectory, "Sandlada.RopSharp.Tests", "DocExamples", "DocExampleDomain.cs");

    /// <summary>Gets the library source files, sorted for deterministic diagnostics.</summary>
    internal static IReadOnlyList<string> LibrarySourceFiles() =>
        Directory.GetFiles(LibrarySourceDirectory, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RopSharp.sln"))
                && Directory.Exists(Path.Combine(directory.FullName, "Sandlada.RopSharp")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate the repository root starting from '{AppContext.BaseDirectory}'.");
    }
}

/// <summary>
/// A single documentation example extracted from an XML documentation comment.
/// </summary>
/// <param name="FileName">The file the example lives in.</param>
/// <param name="LineNumber">The one-based line the example starts on.</param>
/// <param name="MemberName">The documented member the example belongs to.</param>
/// <param name="Code">The C# fragment, with comment prefixes stripped.</param>
internal sealed record DocExample(string FileName, int LineNumber, string MemberName, string Code);

/// <summary>
/// Extracts the <c>&lt;code&gt;&lt;![CDATA[ ... ]]&gt;&lt;/code&gt;</c> blocks that document every public API.
/// </summary>
/// <remarks>
/// Examples are looked up through the Roslyn syntax tree rather than through plain text so that each fragment
/// stays attached to the member that documents it.
/// </remarks>
internal static class DocExampleExtractor
{
    internal const string CodeOpenTag = "<code><![CDATA[";
    internal const string CodeCloseTag = "]]></code>";

    /// <summary>Extracts every documented example from the library sources.</summary>
    /// <returns>The examples, ordered by file and line.</returns>
    internal static IReadOnlyList<DocExample> Extract()
    {
        var examples = new List<DocExample>();

        foreach (var file in RepositoryLayout.LibrarySourceFiles())
        {
            var tree = CSharpSyntaxTree.ParseText(
                File.ReadAllText(file),
                new CSharpParseOptions(LanguageVersion.Latest),
                path: file);

            foreach (var member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
            {
                var trivia = member.GetLeadingTrivia()
                    .Where(static node => node.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                    .Select(static node => node.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .ToArray();

                if (trivia.Length == 0)
                {
                    continue;
                }

                var documentation = string.Concat(trivia.Select(static node => node.ToFullString()));
                var start = documentation.IndexOf(CodeOpenTag, StringComparison.Ordinal);

                if (start < 0)
                {
                    continue;
                }

                var end = documentation.IndexOf(CodeCloseTag, start, StringComparison.Ordinal);

                if (end < 0)
                {
                    continue;
                }

                var raw = documentation.Substring(start + CodeOpenTag.Length, end - start - CodeOpenTag.Length);
                var line = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                examples.Add(new DocExample(
                    Path.GetFileName(file),
                    line,
                    DescribeMember(member),
                    Normalize(raw)));
            }
        }

        return examples.OrderBy(static example => example.FileName, StringComparer.Ordinal)
            .ThenBy(static example => example.LineNumber)
            .ToArray();
    }

    /// <summary>
    /// Normalizes a raw documentation block into compilable C#: every line loses its <c>///</c> prefix and the
    /// common indentation is removed.
    /// </summary>
    /// <param name="raw">The raw text between the CDATA markers.</param>
    /// <returns>The C# fragment.</returns>
    internal static string Normalize(string raw)
    {
        var lines = raw.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var stripped = new List<string>(lines.Length);

        foreach (var line in lines)
        {
            var text = line.TrimStart();
            text = text.StartsWith("///", StringComparison.Ordinal) ? text[3..] : text;
            stripped.Add(text.StartsWith(' ') ? text[1..] : text);
        }

        while (stripped.Count > 0 && stripped[0].Length == 0)
        {
            stripped.RemoveAt(0);
        }

        while (stripped.Count > 0 && stripped[^1].Length == 0)
        {
            stripped.RemoveAt(stripped.Count - 1);
        }

        var indent = stripped
            .Where(static line => line.Length > 0)
            .Select(static line => line.Length - line.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        return string.Join("\n", stripped.Select(line => line.Length >= indent ? line[indent..] : line));
    }

    private static string DescribeMember(MemberDeclarationSyntax member) => member switch
    {
        MethodDeclarationSyntax method =>
            $"{method.Identifier.Text}({string.Join(", ", method.ParameterList.Parameters.Select(static p => p.Type?.ToString() ?? "?"))})",
        OperatorDeclarationSyntax op => $"operator {op.OperatorToken.Text}",
        ConversionOperatorDeclarationSyntax conversion => $"operator {conversion.Type}",
        PropertyDeclarationSyntax property => property.Identifier.Text,
        IndexerDeclarationSyntax indexer => "this[]",
        EventDeclarationSyntax @event => @event.Identifier.Text,
        FieldDeclarationSyntax field => string.Join(", ", field.Declaration.Variables.Select(static v => v.Identifier.Text)),
        ConstructorDeclarationSyntax constructor => constructor.Identifier.Text,
        ClassDeclarationSyntax type => type.Identifier.Text,
        StructDeclarationSyntax type => type.Identifier.Text,
        InterfaceDeclarationSyntax type => type.Identifier.Text,
        EnumDeclarationSyntax type => type.Identifier.Text,
        DelegateDeclarationSyntax type => type.Identifier.Text,
        _ => member.GetType().Name,
    };
}

/// <summary>
/// Builds the compilation used to type check every documented example.
/// </summary>
internal sealed class DocExampleCompilation
{
    private readonly CSharpCompilation _compilation;

    private DocExampleCompilation(CSharpCompilation compilation, IReadOnlyList<DocExample> examples)
    {
        _compilation = compilation;
        Examples = examples;
    }

    /// <summary>Gets the synthesized source that hosts every example, for debugging and inspection.</summary>
    internal static string LastGeneratedSource { get; private set; } = string.Empty;

    /// <summary>Gets the extracted examples.</summary>
    internal IReadOnlyList<DocExample> Examples { get; }

    /// <summary>Gets the semantic model of the synthesized example source.</summary>
    internal SemanticModel SemanticModel => _compilation.GetSemanticModel(_compilation.SyntaxTrees[^1]);

    /// <summary>Gets the synthesized method that hosts the example with the given index.</summary>
    /// <param name="index">The zero-based index of the example.</param>
    /// <returns>The method declaration, or <see langword="null"/> when it cannot be found.</returns>
    internal MethodDeclarationSyntax? GetSnippetMethod(int index)
    {
        var name = $"Example{index:D4}";

        return _compilation.SyntaxTrees[^1]
            .GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method =>
                string.Equals(method.Identifier.Text, name, StringComparison.Ordinal)
                || string.Equals(method.Identifier.Text, name + "Async", StringComparison.Ordinal));
    }

    /// <summary>Compiles every documented example in one pass.</summary>
    /// <param name="examples">The examples to compile.</param>
    /// <returns>The compilation together with the extracted examples.</returns>
    internal static DocExampleCompilation Create(IReadOnlyList<DocExample> examples)
    {
        var source = BuildExampleSource(examples);
        LastGeneratedSource = source;
        WriteGeneratedSource(source);

        var trees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(
                Usings + File.ReadAllText(RepositoryLayout.DomainSourceFile),
                new CSharpParseOptions(LanguageVersion.Latest),
                path: RepositoryLayout.DomainSourceFile),
            CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest), path: "DocExamples.g.cs"),
        };

        var compilation = CSharpCompilation.Create(
            "Sandlada.RopSharp.DocExamples",
            trees,
            ReferenceAssemblies(),
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable,
                allowUnsafe: false));

        return new DocExampleCompilation(compilation, examples);
    }

    /// <summary>Gets the error diagnostics produced by the compilation.</summary>
    /// <returns>The diagnostics with a severity of error.</returns>
    internal IReadOnlyList<Diagnostic> GetErrors() =>
        _compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Error)
            .ToArray();

    /// <summary>Gets the source line of a diagnostic, so the report points at real code.</summary>
    /// <param name="diagnostic">The diagnostic to locate.</param>
    /// <returns>The text of the line the diagnostic points at.</returns>
    internal string GetSourceLine(Diagnostic diagnostic)
    {
        var tree = diagnostic.Location.SourceTree;

        if (tree is null)
        {
            return string.Empty;
        }

        var lines = tree.GetText().ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var index = diagnostic.Location.GetLineSpan().StartLinePosition.Line;

        return index >= 0 && index < lines.Length ? lines[index].Trim() : string.Empty;
    }

    /// <summary>
    /// Gets the reference assemblies used for the example compilation: the runtime implementation assemblies of
    /// the current process plus the freshly built library.
    /// </summary>
    /// <returns>The metadata references.</returns>
    internal static ImmutableArray<MetadataReference> ReferenceAssemblies()
    {
        var references = new List<MetadataReference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trusted)
        {
            foreach (var path in trusted.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!File.Exists(path) || !seen.Add(path))
                {
                    continue;
                }

                try
                {
                    references.Add(MetadataReference.CreateFromFile(path));
                }
                catch (Exception exception) when (exception is IOException or BadImageFormatException or UnauthorizedAccessException)
                {
                    // A runtime assembly that cannot be read as metadata is simply not referenced.
                }
            }
        }

        var library = typeof(Rop).Assembly.Location;

        if (File.Exists(library) && seen.Add(library))
        {
            references.Add(MetadataReference.CreateFromFile(library));
        }

        return references.ToImmutableArray();
    }

    private const string Usings = """
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;

        """;

    private static void WriteGeneratedSource(string source)
    {
        var directory = Path.Combine(RepositoryLayout.RootDirectory, "TestResults");
        Directory.CreateDirectory(directory);

        // The documentation tests run in parallel; the dump is a convenience, so a lost race is not a failure.
        lock (GeneratedSourceLock)
        {
            try
            {
                File.WriteAllText(Path.Combine(directory, "doc-examples.generated.cs"), source);
            }
            catch (IOException)
            {
            }
        }
    }

    private static readonly object GeneratedSourceLock = new();

    private static string BuildExampleSource(IReadOnlyList<DocExample> examples)
    {
        var builder = new System.Text.StringBuilder();

        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Globalization;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using System.Threading;");
        builder.AppendLine("using System.Threading.Tasks;");
        builder.AppendLine("using Sandlada.RopSharp;");
        builder.AppendLine("using Sandlada.RopSharp.DocExamples;");
        builder.AppendLine();
        builder.AppendLine("internal static class DocExampleSnippets");
        builder.AppendLine("{");

        for (var index = 0; index < examples.Count; index++)
        {
            var example = examples[index];
            var isAsync = example.Code.Contains("await ", StringComparison.Ordinal);

            builder.AppendLine($"    // {example.FileName}:{example.LineNumber} {example.MemberName}");
            builder.AppendLine(isAsync
                ? $"    private static async Task Example{index:D4}Async()"
                : $"    private static void Example{index:D4}()");
            builder.AppendLine("    {");

            foreach (var line in example.Code.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
            {
                builder.AppendLine($"        {line}");
            }

            builder.AppendLine("    }");
            builder.AppendLine();
        }

        builder.AppendLine("}");

        return builder.ToString();
    }
}
