using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Sandlada.RopSharp.Analyzers;
using Sandlada.RopSharp.Tests.Documentation;

namespace Sandlada.RopSharp.Tests.Analyzers;

public sealed class RopSharpContractAnalyzerTests
{
    private static readonly DiagnosticAnalyzer Analyzer = new RopSharpContractAnalyzer();

    [Test]
    public async Task ROP0001_DefaultContractValue_ReportsDiagnostic()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    var x = default(Result<int, RopError>);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0001")).IsTrue();
    }

    [Test]
    public async Task ROP0001_DefaultInArrayInitializer_DoesNotReport()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = new Result<int, RopError>[] { default, default };
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0001")).IsFalse();
    }

    [Test]
    public async Task ROP0002_ExpressionStatement_ReportsDiagnostic()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    Result.Ok(1);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0002")).IsTrue();
    }

    [Test]
    public async Task ROP0002_AssignedResult_DoesNotReport()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    var placed = Result.Ok(1);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0002")).IsFalse();
    }

    [Test]
    public async Task ROP0003_SyncMapWithTaskLambda_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static Task<int> MapToTask(int value) => Task.FromResult(value + 1);

                private static void M()
                {
                    var placed = Result.Ok(1).MapOk(MapToTask);
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0003")).IsTrue();
    }

    [Test]
    public async Task ROP0003_MapOkAsyncWithTaskLambda_DoesNotReport()
    {
        const string source = """
            using System.Threading.Tasks;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static async Task M()
                {
                    _ = await ResultTask.From(Result.Ok(1))
                        .MapOkAsync(static (value, _) => Task.FromResult(value + 1));
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0003")).IsFalse();
    }

    [Test]
    public async Task ROP0004_InvalidCreateCode_ReportsWarning()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = RopError.Create("bad", "message");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsTrue();
    }

    [Test]
    public async Task ROP0004_ValidCreateCode_DoesNotReport()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = RopError.Create("E_BAD", "message");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsFalse();
    }

    [Test]
    public async Task ROP0003_BlockBodiedLambdaWithTaskReturn_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = Result.Ok(1).MapOk(x =>
                    {
                        var y = x + 1;
                        return Task.FromResult(y);
                    });
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0003")).IsTrue();
    }

    [Test]
    public async Task ROP0003_BlockBodiedLambdaWithNonTaskReturn_DoesNotReport()
    {
        const string source = """
            using System.Threading.Tasks;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = Result.Ok(1).MapOk(x =>
                    {
                        var y = x + 1;
                        return y;
                    });
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0003")).IsFalse();
    }

    [Test]
    public async Task ROP0003_BlockBodiedLambdaWithNestedLocalFunctionReturningTask_DoesNotReport()
    {
        const string source = """
            using System.Threading.Tasks;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = Result.Ok(1).MapOk(x =>
                    {
                        Task<int> LocalFunc() => Task.FromResult(42);
                        return x + 1;
                    });
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0003")).IsFalse();
    }

    [Test]
    public async Task ROP0004_InvalidFromExceptionCode_ReportsWarning()
    {
        const string source = """
            using System;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = RopError.FromException(new Exception(), "invalid_code");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsTrue();
    }

    [Test]
    public async Task ROP0004_InvalidWithCode_ReportsWarning()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = RopError.Create("E_VALID", "msg").WithCode("bad_code");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsTrue();
    }

    [Test]
    public async Task ROP0004_InvalidResultTryCode_ReportsWarning()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = Result.Try(() => 42, "bad_code");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsTrue();
    }

    [Test]
    public async Task ROP0004_InvalidResultTryAsyncCode_ReportsWarning()
    {
        const string source = """
            using System.Threading.Tasks;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = Result.TryAsync(() => Task.FromResult(42), "bad_code");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsTrue();
    }

    [Test]
    public async Task ROP0004_InvalidResultParseCode_ReportsWarning()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = Result.Parse<int>("42", "bad_code");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsTrue();
    }

    [Test]
    public async Task ROP0004_InvalidParseFailureCode_ReportsWarning()
    {
        const string source = """
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = new ParseFailure("bad_code", "msg", "input", "int");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsTrue();
    }

    [Test]
    public async Task ROP0004_ValidCodesAcrossAPIs_DoNotReport()
    {
        const string source = """
            using System;
            using System.Threading.Tasks;
            using Sandlada.RopSharp;
            internal static class C
            {
                private static void M()
                {
                    _ = RopError.FromException(new Exception(), "E_EXCEPTION");
                    _ = RopError.Create("E_VALID", "msg").WithCode("E_NEW_CODE");
                    _ = Result.Try(() => 42, "E_TRY");
                    _ = Result.TryAsync(() => Task.FromResult(42), "E_TRY_ASYNC");
                    _ = Result.Parse<int>("42", "E_PARSE_CUSTOM");
                    _ = new ParseFailure("E_FAILURE", "msg", "input", "int");
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        await Assert.That(diagnostics.Any(static d => d.Id == "ROP0004")).IsFalse();
    }

    private static async Task<IReadOnlyList<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    source,
                    new CSharpParseOptions(LanguageVersion.Latest),
                    path: "Test.cs"),
            ],
            references: DocExampleCompilation.ReferenceAssemblies(),
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(Analyzer);
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);

        return diagnostics
            .Where(static diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning)
            .ToArray();
    }
}
