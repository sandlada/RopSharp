using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sandlada.RopSharp.Tests.Documentation;

/// <summary>
/// Type checks every XML documentation example of the library.
/// </summary>
/// <remarks>
/// The examples are the primary usage documentation of <c>Sandlada.RopSharp</c>. A drifting example is worse
/// than no example at all, so every fragment is extracted from the library sources and compiled with Roslyn
/// against the real assembly. The same test also enforces the quality rules the documentation guide states:
/// an example must compose with at least one other API and must spell out the outcome of every track.
/// </remarks>
public sealed class DocExampleCompilationTests
{
    private static readonly string[] TrackMarkers = ["Ok(", "Err(", "Some(", "None", "Left(", "Right("];

    [Test]
    public async Task Library_DocumentsAtLeastOneExamplePerPublicApi()
    {
        var examples = DocExampleExtractor.Extract();

        await Assert.That(examples.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task EveryDocumentedExample_Compiles()
    {
        var examples = DocExampleExtractor.Extract();
        var compilation = DocExampleCompilation.Create(examples);

        var errors = compilation.GetErrors();
        var report = errors
            .Select(error =>
            {
                var span = error.Location.GetLineSpan();

                return $"{error.Id} {error.GetMessage()} @ line {span.StartLinePosition.Line + 1}: " +
                    compilation.GetSourceLine(error);
            })
            .ToArray();

        await Assert.That(report).IsEmpty();
    }

    [Test]
    public async Task EveryDocumentedExample_ComposesWithAnotherApi()
    {
        var examples = DocExampleExtractor.Extract();
        var compilation = DocExampleCompilation.Create(examples);

        var libraryAssembly = typeof(Rop).Assembly.GetName().Name;
        var offenders = new List<string>();

        for (var index = 0; index < examples.Count; index++)
        {
            var example = examples[index];
            var method = compilation.GetSnippetMethod(index);

            if (method is null)
            {
                offenders.Add($"{example.FileName}:{example.LineNumber} {example.MemberName}: the example has no host method.");
                continue;
            }

            var referenced = new HashSet<string>(StringComparer.Ordinal);

            foreach (var identifier in method.DescendantNodes().OfType<SimpleNameSyntax>())
            {
                var symbol = compilation.SemanticModel.GetSymbolInfo(identifier).Symbol;

                if (symbol is not null
                    && symbol.ContainingAssembly is not null
                    && string.Equals(symbol.ContainingAssembly.Identity.Name, libraryAssembly, StringComparison.Ordinal)
                    && IsMember(symbol))
                {
                    referenced.Add(NameOf(symbol));
                }
            }

            if (referenced.Count < 2)
            {
                offenders.Add(
                    $"{example.FileName}:{example.LineNumber} {example.MemberName}: " +
                    $"the example only touches {referenced.Count} library member(s) " +
                    $"[{string.Join(", ", referenced)}]; it must show how the API composes with the rest of the library.");
            }
        }

        WriteOffenders("doc-example-composition.txt", offenders);

        await Assert.That(offenders.Count).IsEqualTo(0);
    }

    [Test]
    public async Task EveryDocumentedExample_DocumentsEveryTrack()
    {
        var examples = DocExampleExtractor.Extract();
        var offenders = new List<string>();

        foreach (var example in examples)
        {
            var comments = string.Join(
                " ",
                Regex.Matches(example.Code, "//[^\n]*").Select(static match => match.Value));

            var markers = TrackMarkers
                .Select(marker => (Marker: marker, Count: CountOccurrences(comments, marker)))
                .Where(static entry => entry.Count > 0)
                .ToArray();

            if (markers.Sum(static entry => entry.Count) < 2)
            {
                offenders.Add(
                    $"{example.FileName}:{example.LineNumber} {example.MemberName}: " +
                    "the example must state the outcome of both tracks in a comment " +
                    $"(found {markers.Sum(static entry => entry.Count)} marker(s) " +
                    $"[{string.Join(", ", markers.Select(static entry => entry.Marker))}]).");
            }
        }

        await Assert.That(offenders).IsEmpty();
    }

    [Test]
    public async Task EveryDocumentedExample_AvoidsTopLevelReturn()
    {
        var examples = DocExampleExtractor.Extract();
        var offenders = new List<string>();

        foreach (var example in examples)
        {
            if (Regex.IsMatch(example.Code, @"^\s*return\b", RegexOptions.Multiline))
            {
                offenders.Add($"{example.FileName}:{example.LineNumber} {example.MemberName}: examples must not use 'return'.");
            }
        }

        await Assert.That(offenders).IsEmpty();
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = 0;

        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }

    private static bool IsMember(ISymbol symbol) =>
        symbol is IMethodSymbol or IPropertySymbol or IFieldSymbol or IEventSymbol;

    private static string NameOf(ISymbol symbol) => symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
        ? constructor.ContainingType.Name
        : symbol.Name;

    private static void WriteOffenders(string fileName, IReadOnlyList<string> entries)
    {
        var directory = Path.Combine(RepositoryLayout.RootDirectory, "TestResults");
        Directory.CreateDirectory(directory);

        File.WriteAllLines(
            Path.Combine(directory, fileName),
            entries.OrderBy(static entry => entry, StringComparer.Ordinal));
    }
}
