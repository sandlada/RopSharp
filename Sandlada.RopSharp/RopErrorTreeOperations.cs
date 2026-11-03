namespace Sandlada.RopSharp;

internal static class RopErrorTreeOperations
{
    internal static bool StructuralEquals(RopError? left, RopError? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        var stack = new Stack<(RopError Left, RopError Right)>();
        stack.Push((left, right));
        var leftPath = new HashSet<RopError>(ReferenceEqualityComparer.Instance);
        var rightPath = new HashSet<RopError>(ReferenceEqualityComparer.Instance);

        while (stack.Count > 0)
        {
            var (currentLeft, currentRight) = stack.Pop();

            if (!ReferenceEquals(currentLeft, currentRight) && !SameHeader(currentLeft, currentRight))
            {
                return false;
            }

            if (currentLeft.InnerErrors.Count != currentRight.InnerErrors.Count)
            {
                return false;
            }

            if (currentLeft.InnerErrors.Count == 0)
            {
                continue;
            }

            if (!leftPath.Add(currentLeft))
            {
                return false;
            }

            if (!rightPath.Add(currentRight))
            {
                return false;
            }

            for (var index = currentLeft.InnerErrors.Count - 1; index >= 0; index--)
            {
                stack.Push((currentLeft.InnerErrors[index], currentRight.InnerErrors[index]));
            }

            leftPath.Remove(currentLeft);
            rightPath.Remove(currentRight);
        }

        return true;
    }

    internal static int StructuralHashCode(RopError error)
    {
        var memo = new Dictionary<RopError, int>(ReferenceEqualityComparer.Instance);
        return ComputeHash(error, memo, new HashSet<RopError>(ReferenceEqualityComparer.Instance));
    }

    internal static IReadOnlyList<RopError> FlattenLeaves(RopError error)
    {
        var leaves = new List<RopError>();
        var path = new HashSet<RopError>(ReferenceEqualityComparer.Instance);
        CollectLeaves(error, leaves, path);
        return Array.AsReadOnly(leaves.ToArray());
    }

    private static void CollectLeaves(RopError error, List<RopError> leaves, HashSet<RopError> path)
    {
        if (!path.Add(error))
        {
            return;
        }

        if (error.InnerErrors.Count == 0)
        {
            leaves.Add(error);
            path.Remove(error);
            return;
        }

        foreach (var inner in error.InnerErrors)
        {
            CollectLeaves(inner, leaves, path);
        }

        path.Remove(error);
    }

    private static int ComputeHash(
        RopError error,
        Dictionary<RopError, int> memo,
        HashSet<RopError> path)
    {
        if (memo.TryGetValue(error, out var cached))
        {
            return cached;
        }

        if (!path.Add(error))
        {
            return 0;
        }

        var childHash = 0;
        foreach (var inner in error.InnerErrors)
        {
            childHash = HashCode.Combine(childHash, ComputeHash(inner, memo, path));
        }

        path.Remove(error);

        cached = HashCode.Combine(
            (int)error.Kind,
            StringComparer.Ordinal.GetHashCode(error.Code),
            StringComparer.Ordinal.GetHashCode(error.Message),
            error.Details is null ? 0 : StringComparer.Ordinal.GetHashCode(error.Details),
            childHash);
        memo[error] = cached;
        return cached;
    }

    private static bool SameHeader(RopError left, RopError right) =>
        left.Kind == right.Kind
        && string.Equals(left.Code, right.Code, StringComparison.Ordinal)
        && string.Equals(left.Message, right.Message, StringComparison.Ordinal)
        && string.Equals(left.Details, right.Details, StringComparison.Ordinal);
}
