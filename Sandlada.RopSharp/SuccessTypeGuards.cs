namespace Sandlada.RopSharp;

/// <summary>
/// Guards against using synchronous combinators when the success type is task-like.
/// </summary>
internal static class SuccessTypeGuards
{
    internal static void ThrowIfTaskLike<T>(string syncCombinator)
    {
        if (!TaskLike<T>.IsTaskLike)
        {
            return;
        }

        throw new InvalidOperationException(
            $"The success type {typeof(T).Name} is task-like; use the asynchronous counterpart of {syncCombinator} instead.");
    }

    private static class TaskLike<T>
    {
        internal static readonly bool IsTaskLike = Compute();

        private static bool Compute()
        {
            var type = typeof(T);
            if (type == typeof(Task) || type == typeof(ValueTask))
            {
                return true;
            }

            if (!type.IsGenericType)
            {
                return false;
            }

            var definition = type.GetGenericTypeDefinition();
            return definition == typeof(Task<>) || definition == typeof(ValueTask<>);
        }
    }
}
