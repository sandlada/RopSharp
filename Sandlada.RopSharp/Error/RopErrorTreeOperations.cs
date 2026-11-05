namespace Sandlada.RopSharp;

internal static class RopErrorTreeOperations
{
    private readonly struct EqualityFrame
    {
        public readonly RopError Left;
        public readonly RopError Right;
        public readonly bool IsExit;

        public EqualityFrame(RopError left, RopError right, bool isExit)
        {
            Left = left;
            Right = right;
            IsExit = isExit;
        }
    }

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

        var stack = new Stack<EqualityFrame>();
        stack.Push(new EqualityFrame(left, right, isExit: false));
        var leftPath = new HashSet<RopError>(ReferenceEqualityComparer.Instance);
        var rightPath = new HashSet<RopError>(ReferenceEqualityComparer.Instance);

        while (stack.Count > 0)
        {
            var frame = stack.Pop();
            if (frame.IsExit)
            {
                leftPath.Remove(frame.Left);
                rightPath.Remove(frame.Right);
                continue;
            }

            var currentLeft = frame.Left;
            var currentRight = frame.Right;

            if (ReferenceEquals(currentLeft, currentRight))
            {
                continue;
            }

            if (!SameHeader(currentLeft, currentRight))
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

            var leftInPath = leftPath.Contains(currentLeft);
            var rightInPath = rightPath.Contains(currentRight);

            if (leftInPath || rightInPath)
            {
                if (leftInPath && rightInPath)
                {
                    continue;
                }

                return false;
            }

            leftPath.Add(currentLeft);
            rightPath.Add(currentRight);

            stack.Push(new EqualityFrame(currentLeft, currentRight, isExit: true));
            for (var index = currentLeft.InnerErrors.Count - 1; index >= 0; index--)
            {
                stack.Push(new EqualityFrame(currentLeft.InnerErrors[index], currentRight.InnerErrors[index], isExit: false));
            }
        }

        return true;
    }

    internal static int StructuralHashCode(RopError error)
    {
        var memo = new Dictionary<RopError, int>(ReferenceEqualityComparer.Instance);
        var path = new HashSet<RopError>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<(RopError Error, bool IsExit)>();
        stack.Push((error, false));

        while (stack.Count > 0)
        {
            var (current, isExit) = stack.Pop();

            if (isExit)
            {
                path.Remove(current);
                var childHash = 0;
                for (var i = 0; i < current.InnerErrors.Count; i++)
                {
                    memo.TryGetValue(current.InnerErrors[i], out var h);
                    childHash = HashCode.Combine(childHash, h);
                }

                var cached = HashCode.Combine(
                    (int)current.Kind,
                    StringComparer.Ordinal.GetHashCode(current.Code),
                    StringComparer.Ordinal.GetHashCode(current.Message),
                    current.Details is null ? 0 : StringComparer.Ordinal.GetHashCode(current.Details),
                    childHash);
                memo[current] = cached;
                continue;
            }

            if (memo.ContainsKey(current))
            {
                continue;
            }

            if (!path.Add(current))
            {
                memo[current] = 0;
                continue;
            }

            if (current.InnerErrors.Count == 0)
            {
                path.Remove(current);
                var leafHash = HashCode.Combine(
                    (int)current.Kind,
                    StringComparer.Ordinal.GetHashCode(current.Code),
                    StringComparer.Ordinal.GetHashCode(current.Message),
                    current.Details is null ? 0 : StringComparer.Ordinal.GetHashCode(current.Details),
                    0);
                memo[current] = leafHash;
                continue;
            }

            stack.Push((current, true));
            for (var index = current.InnerErrors.Count - 1; index >= 0; index--)
            {
                stack.Push((current.InnerErrors[index], false));
            }
        }

        return memo.TryGetValue(error, out var result) ? result : 0;
    }

    internal static IReadOnlyList<RopError> FlattenLeaves(RopError error)
    {
        var leaves = new List<RopError>();
        var path = new HashSet<RopError>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<(RopError Error, bool IsExit)>();
        stack.Push((error, false));

        while (stack.Count > 0)
        {
            var (current, isExit) = stack.Pop();
            if (isExit)
            {
                path.Remove(current);
                continue;
            }

            if (!path.Add(current))
            {
                continue;
            }

            if (current.InnerErrors.Count == 0)
            {
                leaves.Add(current);
                path.Remove(current);
                continue;
            }

            stack.Push((current, true));
            for (var index = current.InnerErrors.Count - 1; index >= 0; index--)
            {
                stack.Push((current.InnerErrors[index], false));
            }
        }

        return Array.AsReadOnly(leaves.ToArray());
    }

    private static bool SameHeader(RopError left, RopError right) =>
        left.Kind == right.Kind
        && string.Equals(left.Code, right.Code, StringComparison.Ordinal)
        && string.Equals(left.Message, right.Message, StringComparison.Ordinal)
        && string.Equals(left.Details, right.Details, StringComparison.Ordinal);
}
