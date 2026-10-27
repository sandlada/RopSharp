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
        var parsed = Rop.Parse<int>("41");
        var weighted = await Rop.CombineAsync(
            Task.FromResult(parsed),
            Task.FromResult(Rop.Ok(2)),
            (value, factor) => value * factor);

        weighted
            .FailIf(
                value => value < 0,
                RopError.Create("E_NEGATIVE", "value must be non-negative", RopErrorKind.Validation))
            .Switch(
                value => Console.WriteLine($"   parsed: {value}"),
                error => Console.WriteLine($"   parsed: ERR {error.Code}"));

        var either = Either.Left<string, int>("refund");
        Console.WriteLine($"   either: {either.Match(left => left, right => right.ToString())}");

        var last = new[] { Opt.Some(1), Opt.None<int>(), Opt.Some(3) }.Choose().TryLast();
        Console.WriteLine($"   option last: {last.Match(value => value.ToString(), () => "none")}");
    }
}
