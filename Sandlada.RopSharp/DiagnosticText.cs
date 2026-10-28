using System.Globalization;
using System.Text;

namespace Sandlada.RopSharp;

/// <summary>
/// Renders caller-supplied text for diagnostics without letting it break message structure, log lines or memory.
/// </summary>
/// <remarks>
/// Diagnostic output is bounded and escaped: quotes, backslashes and control characters are encoded, and long input is
/// truncated on a Unicode code-point boundary so a surrogate pair is never split. This keeps a single malformed value
/// from forging log entries or inflating an error payload.
/// </remarks>
internal static class DiagnosticText
{
    /// <summary>The maximum number of Unicode code points echoed before truncation.</summary>
    internal const int MaxEchoLength = 64;

    /// <summary>
    /// Escapes control characters, quotes and backslashes, truncating to <see cref="MaxEchoLength"/> code points.
    /// </summary>
    /// <param name="value">The text to escape.</param>
    /// <returns>A single-line rendering suitable for a message or details field.</returns>
    /// <remarks>
    /// The returned string is always well-formed UTF-16: a lone surrogate is rendered as <c>\uXXXX</c> instead of
    /// being copied through. Unicode line and paragraph separators and bidirectional control characters are escaped
    /// too, because they are invisible and can otherwise forge a log line or reorder terminal output.
    /// </remarks>
    internal static string Escape(string value)
    {
        var builder = new StringBuilder(Math.Min(value.Length, MaxEchoLength) + 8);
        var remaining = MaxEchoLength;
        var index = 0;

        while (index < value.Length)
        {
            if (remaining == 0)
            {
                builder.Append("...");
                break;
            }

            var character = value[index];

            // A valid pair counts as one code point and is copied verbatim so it is never split.
            if (char.IsHighSurrogate(character)
                && index + 1 < value.Length
                && char.IsLowSurrogate(value[index + 1]))
            {
                builder.Append(character).Append(value[index + 1]);
                index += 2;
                remaining--;
                continue;
            }

            // A lone surrogate is not a code point and would make the output ill-formed; escape it instead.
            if (char.IsSurrogate(character))
            {
                AppendUnicodeEscape(builder, character);
                index++;
                remaining--;
                continue;
            }

            AppendEscaped(builder, character);
            index++;
            remaining--;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Quotes text for a machine-readable details field, or returns the literal <c>null</c>.
    /// </summary>
    /// <param name="value">The text to quote.</param>
    /// <returns><c>null</c> or a quoted, escaped, bounded rendering.</returns>
    internal static string Quote(string? value) =>
        value is null ? "null" : $"\"{Escape(value)}\"";

    private static void AppendEscaped(StringBuilder builder, char character)
    {
        switch (character)
        {
            case '\\':
                builder.Append("\\\\");
                break;
            case '"':
                builder.Append("\\\"");
                break;
            case '\r':
                builder.Append("\\r");
                break;
            case '\n':
                builder.Append("\\n");
                break;
            case '\t':
                builder.Append("\\t");
                break;
            // Unicode line/paragraph separators are invisible to char.IsControl but still break log lines.
            case '\u2028':
            case '\u2029':
                AppendUnicodeEscape(builder, character);
                break;
            default:
                if (char.IsControl(character) || IsBidiControl(character))
                {
                    AppendUnicodeEscape(builder, character);
                }
                else
                {
                    builder.Append(character);
                }

                break;
        }
    }

    // Bidirectional formatting characters can visually reorder a log line, so they are escaped like controls.
    private static bool IsBidiControl(char character) =>
        character is '\u061C' or '\u200E' or '\u200F'
        || character is >= '\u202A' and <= '\u202E'
        || character is >= '\u2066' and <= '\u2069';

    private static void AppendUnicodeEscape(StringBuilder builder, char character) =>
        builder.Append("\\u").Append(((int)character).ToString("X4", CultureInfo.InvariantCulture));
}
