using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Parsing is the only path that handles text, so it is the most likely place for accidental allocations. The
/// failure path also renders diagnostics, which truncates and escapes the echoed input.
/// </summary>
[Config(typeof(BenchConfig))]
public class ParsingBenchmarks
{
    private string _valid = string.Empty;
    private string _invalid = string.Empty;
    private string _longInvalid = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _valid = "2147483647";
        _invalid = "12x34";
        _longInvalid = new string('9', 512) + "x";
    }

    [Benchmark(Baseline = true)]
    public Result<int, RopError> Parse_Int_Success() => Rop.Parse<int>(_valid);

    [Benchmark]
    public Result<int, RopError> Parse_Int_Failure() => Rop.Parse<int>(_invalid);

    [Benchmark]
    public Result<int, RopError> Parse_Int_LongFailure() => Rop.Parse<int>(_longInvalid);

    [Benchmark]
    public Result<long, RopError> Parse_Long_Success() => Rop.Parse<long>("9223372036854775807");

    [Benchmark]
    public Option<int> TryParse_Int_Success() => Rop.TryParse<int>(_valid);

    [Benchmark]
    public Option<int> TryParse_Int_Failure() => Rop.TryParse<int>(_invalid);

    [Benchmark]
    public Result<Guid, RopError> Parse_Guid_Success() => Rop.Parse<Guid>("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Benchmark]
    public Result<DateTime, RopError> Parse_DateTime_Success() => Rop.Parse<DateTime>("2026-10-29T00:00:00Z");
}
