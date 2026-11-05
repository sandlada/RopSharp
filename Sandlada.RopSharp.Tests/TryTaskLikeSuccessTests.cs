using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class TryTaskLikeSuccessTests
{
    [Test]
    public async Task Try_WithTaskSuccess_ThrowsInvalidOperationException()
    {
        await Assert
            .That(() => Result.Try<Task>(() => Task.CompletedTask, "E_BOUNDARY"))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Ok_WithTaskSuccess_RemainsOk()
    {
        var task = Task.CompletedTask;
        var result = Result.Ok(task);

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.TryGetOk(out var ok)).IsTrue();
        await Assert.That(ReferenceEquals(ok, task)).IsTrue();
    }

    [Test]
    public async Task SynchronousCombinators_WithTaskLikeSuccess_ThrowInvalidOperationException()
    {
        var result = Result.Ok(1);
        await Assert.That(() => result.MapOk(static x => Task.FromResult(x))).Throws<InvalidOperationException>();
        await Assert.That(() => result.BindOk(static x => Result.Ok(Task.FromResult(x)))).Throws<InvalidOperationException>();

        var option = Option.Some(1);
        await Assert.That(() => option.MapSome(static x => Task.FromResult(x))).Throws<InvalidOperationException>();
        await Assert.That(() => option.BindSome(static x => Option.Some(Task.FromResult(x)))).Throws<InvalidOperationException>();

        var validation = Validation.Success<int, RopError>(1);
        await Assert.That(() => validation.Map(static x => Task.FromResult(x))).Throws<InvalidOperationException>();
    }
}
