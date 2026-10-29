using BenchmarkDotNet.Attributes;
using Sandlada.RopSharp;
using Sandlada.RopSharp.Benchmarks.Infrastructure;

namespace Sandlada.RopSharp.Benchmarks;

/// <summary>
/// Asynchronous combinators. A completed task and a genuinely asynchronous source are measured separately because
/// the state machine cost differs: the completed path avoids the await suspension, the pending path pays for it.
/// </summary>
[Config(typeof(BenchConfig))]
public class AsyncBenchmarks
{
    private static readonly RopError Error = RopError.Create("E_BENCH", "failure");

    private readonly Result<int, RopError> _ok = Rop.Ok(42);
    private readonly Result<int, RopError> _err = Rop.Err<int>(Error);

    private Task<int> _completed = Task.FromResult(42);
    private Task<int> _pending = null!;

    [GlobalSetup]
    public void Setup() => _pending = CreatePending();

    private static Task<int> CreatePending()
    {
        var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.Run(async () =>
        {
            await Task.Yield();
            source.SetResult(42);
        });

        return source.Task;
    }

    [Benchmark(Baseline = true)]
    public async Task<Result<int, RopError>> MapOkAsync_Completed() =>
        await _ok.MapOkAsync(static value => Task.FromResult(value + 1));

    [Benchmark]
    public async Task<Result<int, RopError>> MapOkAsync_Pending() =>
        await _ok.MapOkAsync(_ => _pending);

    [Benchmark]
    public async Task<Result<int, RopError>> BindOkAsync_Completed() =>
        await _ok.BindOkAsync(static value => Task.FromResult(Rop.Ok(value + 1)));

    [Benchmark]
    public async Task<Result<int, RopError>> BindOkAsync_Pending() =>
        await _ok.BindOkAsync(_ => _pending.ContinueWith(static t => Rop.Ok(t.Result), TaskContinuationOptions.ExecuteSynchronously));

    [Benchmark]
    public async Task<Result<int, RopError>> MapErrAsync_OnErr() =>
        await _err.MapErrAsync(static error => Task.FromResult(error.WithCode("E_OTHER")));

    [Benchmark]
    public async Task<Result<int, RopError>> MapOkAsync_OnErr() =>
        await _err.MapOkAsync(static value => Task.FromResult(value + 1));
}
