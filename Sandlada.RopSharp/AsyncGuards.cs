namespace Sandlada.RopSharp;

/// <summary>
/// Validates that a user callback supplied to an asynchronous combinator returned a task.
/// </summary>
/// <remarks>
/// Returning <see langword="null"/> from a <see cref="Task"/>-returning callback is a programming error. The library
/// surfaces it as <see cref="InvalidOperationException"/> with the callback role in the message instead of letting
/// the runtime throw a <see cref="NullReferenceException"/> while awaiting.
/// </remarks>
internal static class AsyncGuards
{
    internal static Task<T> Require<T>(Task<T>? task, string callback) =>
        task ?? throw new InvalidOperationException($"The {callback} returned a null task.");

    internal static Task Require(Task? task, string callback) =>
        task ?? throw new InvalidOperationException($"The {callback} returned a null task.");
}
