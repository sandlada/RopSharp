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
}
