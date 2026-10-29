using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Validators;

namespace Sandlada.RopSharp.Benchmarks.Infrastructure;

/// <summary>
/// Shared configuration for every benchmark class. The job, diagnoser, exporters and validators live here so that a
/// run never depends on command-line flags. Debug builds and groups without exactly one baseline are rejected.
/// </summary>
public sealed class BenchConfig : ManualConfig
{
    public const string ArtifactsDirectory = "artifacts/benchmarks";

    public BenchConfig()
    {
        AddJob(Job.ShortRun.WithId("short"));

        AddDiagnoser(MemoryDiagnoser.Default);

        AddExporter(JsonExporter.FullCompressed);

        AddLogger(ConsoleLogger.Default);

        AddValidator(JitOptimizationsValidator.FailOnError);
        AddValidator(BaselineValidator.FailOnError);

        WithArtifactsPath(ArtifactsDirectory);
    }
}
