using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sandlada.RopSharp.Benchmarks.Infrastructure;

/// <summary>
/// Prints the commit and host details before a run so that exported results can be traced back to their source.
/// </summary>
public static class EnvironmentProvenance
{
    public static void Print()
    {
        Console.WriteLine($"Commit     : {CurrentCommit()}");
        Console.WriteLine($"Runtime    : {RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"OS         : {RuntimeInformation.OSDescription}");
        Console.WriteLine($"Arch       : {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"Processors : {Environment.ProcessorCount}");
        Console.WriteLine($"Timestamp  : {DateTimeOffset.UtcNow:O}");
        Console.WriteLine();
    }

    private static string CurrentCommit()
    {
        var fromCi = Environment.GetEnvironmentVariable("GITHUB_SHA");
        if (!string.IsNullOrWhiteSpace(fromCi))
        {
            return fromCi;
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo("git", "rev-parse --short HEAD")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });

            if (process is null)
            {
                return "unknown";
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return process.ExitCode == 0 && output.Length > 0 ? output : "unknown";
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return "unknown";
        }
    }
}
