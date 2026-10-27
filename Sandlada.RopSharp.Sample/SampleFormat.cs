namespace Sandlada.RopSharp.Sample;

/// <summary>
/// Shared console formatting for the scenario output.
/// </summary>
internal static class SampleFormat
{
    public static string Errors(IEnumerable<RopError> errors) =>
        string.Join("; ", errors.Select(static error => error.ToString()));
}
