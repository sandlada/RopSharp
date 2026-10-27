namespace Sandlada.RopSharp.Sample;

/// <summary>
/// Prints a scenario header, runs the scenario and keeps a blank line between scenarios so the
/// console output stays scannable.
/// </summary>
internal static class ScenarioRunner
{
    public static void Run(string title, Action scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        WriteHeader(title);
        scenario();
        Console.WriteLine();
    }

    public static async Task RunAsync(string title, Func<Task> scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        WriteHeader(title);
        await scenario();
        Console.WriteLine();
    }

    private static void WriteHeader(string title) => Console.WriteLine($"=== {title} ===");
}
