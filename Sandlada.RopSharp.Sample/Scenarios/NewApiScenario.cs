using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Sample.Scenarios;

/// <summary>
/// A short tour of the 1.1 additions: parsing, async aggregation, action terminals, collection helpers and the
/// general <see cref="Either{TLeft,TRight}"/> union.
/// </summary>
internal static class NewApiScenario
{
    public static async Task RunAsync()
    {
        var parsed = Result.Parse<int>("41");
        var weighted = await ResultTask.Zip<int, int, int, RopError>(
            ResultTask.From(parsed),
            ResultTask.From(Result.Ok<int, RopError>(2)),
            static (value, factor) => value * factor);

        weighted
            .FailIf(
                value => value < 0,
                RopError.Create("E_NEGATIVE", "value must be non-negative", RopErrorKind.Validation))
            .Switch(
                value => Console.WriteLine($"   parsed: {value}"),
                error => Console.WriteLine($"   parsed: ERR {error.Code}"));

        var either = Either.Left<string, int>("refund");
        Console.WriteLine($"   either: {either.Match(left => left, right => right.ToString())}");

        var last = new[] { Option.Some(1), Option.None<int>(), Option.Some(3) }.Choose().TryLast();
        last.Switch(
            value => Console.WriteLine($"   option last: {value}"),
            () => Console.WriteLine("   option last: none"));

        var totalText = await Result.TryAsync(
                static async ct =>
                {
                    await Task.Yield();
                    ct.ThrowIfCancellationRequested();
                    return 42;
                },
                "E_CALC")
            .Ensure(static n => n > 0, RopError.Create("E_RANGE", "must be positive"))
            .MatchAsync(
                ok: static (n, _) => Task.FromResult($"ok:{n}"),
                err: static (e, _) => Task.FromResult($"err:{e.Code}"));
        Console.WriteLine($"   async match: {totalText}");

        var pair = await ValidationTask.Zip(
                ValidationTask.From(Validation.Success<int, RopError>(1)),
                ValidationTask.From(Validation.Success<string, RopError>("a")))
            .Collapse(RopError.Combine);
        Console.WriteLine($"   validation tuple: {pair.Match(static v => $"{v.First}:{v.Second}", static e => e.Code)}");
    }
}
