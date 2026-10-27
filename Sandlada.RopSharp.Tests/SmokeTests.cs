using Sandlada.RopSharp;

namespace Sandlada.RopSharp.Tests;

public sealed class SmokeTests
{
    [Test]
    public async Task Result_Ok_ExposesValue()
    {
        var result = Rop.Ok(42);

        await Assert.That(result.IsOk).IsTrue();
        await Assert.That(result.IsErr).IsFalse();
        await Assert.That(result.OrThrow()).IsEqualTo(42);
    }

    [Test]
    public async Task LoadedAssembly_IsTheLibrary()
    {
        await Assert.That(typeof(Rop).Assembly.GetName().Name).IsEqualTo("Sandlada.RopSharp");
    }
}
