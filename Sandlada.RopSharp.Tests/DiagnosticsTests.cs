using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class DiagnosticsTests
{
    [Test]
    public async Task RopErrorKind_AppendsNewValuesWithoutRenumbering()
    {
        (RopErrorKind Kind, int Value)[] expected =
        [
            (RopErrorKind.Failure, 0),
            (RopErrorKind.Validation, 1),
            (RopErrorKind.NotFound, 2),
            (RopErrorKind.Conflict, 3),
            (RopErrorKind.Unauthorized, 4),
            (RopErrorKind.Forbidden, 5),
            (RopErrorKind.Unexpected, 6),
            (RopErrorKind.Timeout, 7),
            (RopErrorKind.Unavailable, 8),
            (RopErrorKind.RateLimited, 9),
        ];

        foreach (var (kind, value) in expected)
        {
            await Assert.That((int)kind).IsEqualTo(value);
        }
    }

    [Test]
    public async Task Create_WithNewKind_PreservesKind()
    {
        var error = RopError.Create("E_BUSY", "try later", RopErrorKind.RateLimited);

        await Assert.That(error.Kind).IsEqualTo(RopErrorKind.RateLimited);
    }

    [Test]
    public async Task Parse_WhenSuccessful_DoesNotCreateError()
    {
        var result = Rop.Parse<int>("42");

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task Parse_WithInvalidValue_EchoesInputInMessageAndDetails()
    {
        var error = Rop.Parse<int>("not-a-number").ErrOrThrow();

        await Assert.That(error.Message).IsEqualTo("'not-a-number' is not a valid Int32.");
        await Assert.That(error.Details).IsEqualTo("input=\"not-a-number\"");
    }

    [Test]
    public async Task Parse_WithNullValue_ReportsNullInsteadOfEmptyQuotes()
    {
        var error = Rop.Parse<int>(null).ErrOrThrow();

        await Assert.That(error.Message).IsEqualTo("The value is null, which is not a valid Int32.");
        await Assert.That(error.Details).IsEqualTo("input=null");
    }

    [Test]
    public async Task Parse_WithEmptyValue_EchoesEmptyQuotes()
    {
        var error = Rop.Parse<int>(string.Empty).ErrOrThrow();

        await Assert.That(error.Message).IsEqualTo("'' is not a valid Int32.");
        await Assert.That(error.Details).IsEqualTo("input=\"\"");
    }

    [Test]
    public async Task Parse_WithCustomCode_UsesProvidedCode()
    {
        var error = Rop.Parse<int>("nope", "E_QUANTITY").ErrOrThrow();

        await Assert.That(error.Code).IsEqualTo("E_QUANTITY");
    }

    [Test]
    public async Task Parse_WithBlankCode_Throws()
    {
        var exception = TestSupport.Catch(() => Rop.Parse<int>("42", " "));

        await Assert.That(exception is ArgumentException).IsTrue();
    }

    [Test]
    public async Task Parse_WithControlCharacters_EscapesThemInMessageAndDetails()
    {
        var error = Rop.Parse<int>("a\nb\rc\td\u0001e").ErrOrThrow();
        var details = error.Details!;

        await Assert.That(error.Message.Contains("\\n")).IsTrue();
        await Assert.That(details.Contains("\\n")).IsTrue();
        await Assert.That(details.Contains("\\r")).IsTrue();
        await Assert.That(details.Contains("\\t")).IsTrue();
        await Assert.That(details.Contains("\\u0001")).IsTrue();
        await Assert.That(details.Contains('\n')).IsFalse();
        await Assert.That(details.Contains('\r')).IsFalse();
    }

    [Test]
    public async Task Parse_WithQuotesAndBackslashes_EscapesThem()
    {
        var error = Rop.Parse<int>("a\"b\\c").ErrOrThrow();
        var details = error.Details!;

        await Assert.That(details.Contains("\\\"")).IsTrue();
        await Assert.That(details.Contains("\\\\")).IsTrue();
    }

    [Test]
    public async Task Parse_WithLongValue_TruncatesTheEcho()
    {
        var input = new string('a', DiagnosticTextProbe.MaxEchoLength + 10);

        var error = Rop.Parse<int>(input).ErrOrThrow();
        var details = error.Details!;

        await Assert.That(details.EndsWith("...\"", StringComparison.Ordinal)).IsTrue();
        await Assert.That(details.Contains(new string('a', DiagnosticTextProbe.MaxEchoLength))).IsTrue();
        await Assert.That(details.Contains(new string('a', DiagnosticTextProbe.MaxEchoLength + 1))).IsFalse();
    }

    [Test]
    public async Task Parse_WithValueAtTheEchoLimit_DoesNotTruncate()
    {
        var input = new string('b', DiagnosticTextProbe.MaxEchoLength);

        var error = Rop.Parse<int>(input).ErrOrThrow();

        await Assert.That(error.Details).IsEqualTo($"input=\"{input}\"");
    }

    [Test]
    public async Task Parse_WithSurrogatePairAtTheEchoLimit_DoesNotTruncate()
    {
        var input = new string('a', DiagnosticTextProbe.MaxEchoLength - 1) + "😀";

        var error = Rop.Parse<int>(input).ErrOrThrow();

        await Assert.That(error.Details).IsEqualTo($"input=\"{input}\"");
    }

    [Test]
    public async Task Parse_WithAstralCharacters_TruncatesByCodePoint()
    {
        var prefix = new string('a', DiagnosticTextProbe.MaxEchoLength - 1);
        var input = prefix + "😀😀";

        var error = Rop.Parse<int>(input).ErrOrThrow();

        await Assert.That(error.Details).IsEqualTo($"input=\"{prefix}😀...\"");
    }

    [Test]
    public async Task Parse_WithLoneSurrogate_EscapesItInsteadOfEmittingInvalidText()
    {
        var error = Rop.Parse<int>("a\uD800b").ErrOrThrow();
        var details = error.Details!;

        await Assert.That(details.Contains("\\uD800")).IsTrue();
        await Assert.That(details.Contains('\uD800')).IsFalse();
    }

    [Test]
    public async Task Parse_WithUnicodeLineSeparators_EscapesThem()
    {
        var error = Rop.Parse<int>("a\u2028b\u2029c").ErrOrThrow();
        var details = error.Details!;

        await Assert.That(details.Contains("\\u2028")).IsTrue();
        await Assert.That(details.Contains("\\u2029")).IsTrue();
        await Assert.That(details.Contains('\u2028')).IsFalse();
        await Assert.That(details.Contains('\u2029')).IsFalse();
    }

    [Test]
    public async Task Parse_WithBidiControls_EscapesThem()
    {
        var error = Rop.Parse<int>("a\u202Eb\u2066c\u200Fd").ErrOrThrow();
        var details = error.Details!;

        await Assert.That(details.Contains("\\u202E")).IsTrue();
        await Assert.That(details.Contains("\\u2066")).IsTrue();
        await Assert.That(details.Contains("\\u200F")).IsTrue();
        await Assert.That(details.Contains('\u202E')).IsFalse();
        await Assert.That(details.Contains('\u2066')).IsFalse();
        await Assert.That(details.Contains('\u200F')).IsFalse();
    }
}

/// <summary>Mirrors the internal echo limit so the tests can describe boundaries without duplicating magic numbers.</summary>
internal static class DiagnosticTextProbe
{
    internal const int MaxEchoLength = 64;
}
