using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

/// <summary>
/// Seeded randomized checks that the documented algebraic laws hold across many values,
/// not just the hand-picked cases in <see cref="ResultLawTests"/>.
/// </summary>
public sealed class RandomizedLawTests
{
    private const int Iterations = 250;

    private static Result<int, string> RandomResult(Random random) =>
        random.Next(2) == 0
            ? Rop.Ok<int, string>(random.Next(-50, 50))
            : Rop.Err<int, string>($"E_{random.Next(5)}");

    [Test]
    public async Task MapOk_FunctorLaws_HoldForRandomInputs()
    {
        var random = new Random(20261027);

        for (var index = 0; index < Iterations; index++)
        {
            var result = RandomResult(random);
            Func<int, int> first = value => (value * 3) + 1;
            Func<int, int> second = value => value % 7;

            await Assert.That(result.MapOk(value => value)).IsEqualTo(result);
            await Assert.That(result.MapOk(first).MapOk(second))
                .IsEqualTo(result.MapOk(value => second(first(value))));
        }
    }

    [Test]
    public async Task BindOk_MonadLaws_HoldForRandomInputs()
    {
        var random = new Random(42);
        Func<int, Result<int, string>> bound = value =>
            value % 3 == 0
                ? Rop.Err<int, string>($"E_{Math.Abs(value) % 5}")
                : Rop.Ok<int, string>(((value * 2) + 1) % 11);
        Func<int, Result<int, string>> chained = value =>
            value % 2 == 0
                ? Rop.Ok<int, string>(value - 1)
                : Rop.Err<int, string>($"E_{Math.Abs(value) % 3}");

        for (var index = 0; index < Iterations; index++)
        {
            var value = random.Next(-25, 25);
            var result = RandomResult(random);

            // Left identity
            await Assert.That(Rop.Ok<int, string>(value).BindOk(bound)).IsEqualTo(bound(value));

            // Right identity
            await Assert.That(result.BindOk<int, int, string>(Rop.Ok<int, string>)).IsEqualTo(result);

            // Associativity
            await Assert.That(result.BindOk(bound).BindOk(chained))
                .IsEqualTo(result.BindOk(current => bound(current).BindOk(chained)));
        }
    }

    [Test]
    public async Task BindErr_MonadLaws_HoldForRandomInputs()
    {
        var random = new Random(7);
        Func<string, Result<int, string>> recover = code =>
            code.EndsWith('0')
                ? Rop.Ok<int, string>(code.Length)
                : Rop.Err<int, string>($"{code}_retry");
        Func<string, Result<int, string>> again = code =>
            Rop.Err<int, string>($"{code}_again");

        for (var index = 0; index < Iterations; index++)
        {
            var error = $"E_{random.Next(10)}";
            var result = RandomResult(random);

            await Assert.That(Rop.Err<int, string>(error).BindErr(recover)).IsEqualTo(recover(error));
            await Assert.That(result.BindErr<int, string>(Rop.Err<int, string>)).IsEqualTo(result);
            await Assert.That(result.BindErr(recover).BindErr(again))
                .IsEqualTo(result.BindErr(code => recover(code).BindErr(again)));
        }
    }

    [Test]
    public async Task Option_MirrorLaws_HoldForRandomInputs()
    {
        var random = new Random(99);
        Func<int, Option<int>> bound = value =>
            value % 4 == 0 ? Opt.None<int>() : Opt.Some(value + 1);

        for (var index = 0; index < Iterations; index++)
        {
            var value = random.Next(-30, 30);
            Option<int> option = random.Next(2) == 0 ? Opt.Some(value) : Opt.None<int>();

            await Assert.That(Opt.Some(value).BindSome(bound)).IsEqualTo(bound(value));
            await Assert.That(option.BindSome<int, int>(Opt.Some)).IsEqualTo(option);
            await Assert.That(option.Filter(static _ => false).IsNone).IsTrue();
        }
    }

    [Test]
    public async Task Validate_CollectsExactlyTheErrorsOfItsInputs()
    {
        var random = new Random(1234);

        for (var index = 0; index < Iterations; index++)
        {
            var first = RandomResult(random);
            var second = RandomResult(random);

            var result = Rop.Validate(first, second, static (a, b) => a + b);

            var expected = new List<string>();
            if (first.IsErr)
            {
                expected.Add(first.ErrOrThrow());
            }

            if (second.IsErr)
            {
                expected.Add(second.ErrOrThrow());
            }

            if (expected.Count == 0)
            {
                await Assert.That(result.IsOk).IsTrue();
            }
            else
            {
                await TestSupport.AssertSequenceAsync(result.ErrOrThrow(), [.. expected]);
            }
        }
    }

    [Test]
    public async Task Partition_IsTheComplementOfSequence()
    {
        var random = new Random(555);

        for (var index = 0; index < 100; index++)
        {
            var results = Enumerable.Range(0, random.Next(0, 8)).Select(_ => RandomResult(random)).ToArray();

            var (oks, errs) = Rop.Partition(results);
            var sequenced = Rop.Sequence(results);

            await Assert.That(oks.Count + errs.Count).IsEqualTo(results.Length);
            await Assert.That(sequenced.IsOk).IsEqualTo(errs.Count == 0);
        }
    }
}
