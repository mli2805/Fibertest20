using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Graph.Magic
{
    public static class PathFinder
    {
        public static Func<Guid, IEnumerable<Guid>> GetAdjacentNodes(
            this ReadModel readModel)
        {
            return nodeId => GetAdjacentNodes(readModel, nodeId);
        }

        private static IEnumerable<Guid> GetAdjacentNodes(this ReadModel readModel, Guid nodeId)
        {
            foreach (var fiber in readModel.Fibers)
            {
                if (fiber.Node1 == nodeId)
                    yield return fiber.Node2;
                if (fiber.Node2 == nodeId)
                    yield return fiber.Node1;
            }
        }

        public static IEnumerable<T> BadFindPathTo<T>(
            this T start, T end, Func<T, IEnumerable<T>> adjacentNodes)
        {
            var path = new Stack<T>();
            path.Push(start);
            BadFindPathRecursive(end, path, adjacentNodes);
            return path.Reverse();
        }
        private static void BadFindPathRecursive<T>(T end, Stack<T> path,
            Func<T, IEnumerable<T>> adjacentNodes)
        {
            var previous = path.Peek();
            foreach (var adjacent in adjacentNodes(previous))
            {
                if (Equals(adjacent, end))
                {
                    path.Push(end);
                    return;
                }
                if (path.Contains(adjacent))
                    continue;

                path.Push(adjacent);
                BadFindPathRecursive(end, path, adjacentNodes);

                if (Equals(path.Peek(), end)) return;
                path.Pop();
            }
        }

        public static IEnumerable<T> FindPathTo<T>(
            this T start, T end, Func<T, IEnumerable<T>> adjacentNodes)
        {
            if (Equals(start, end)) return new[] {start};
            var stepNumber = 0;
            var distances = new Dictionary<T, int> { { start, 0 } };
            var current = new HashSet<T> { start };
            var next = new HashSet<T>();
            while (current.Count != 0)
            {
                DiscoverNextGeneration(stepNumber++, distances, current, next, adjacentNodes);
                ClearAndSwap(ref current, ref next);
            }
            if (!distances.ContainsKey(end)) return Enumerable.Empty<T>();
            return TraverseBack(end, stepNumber - 1, adjacentNodes, distances)
                .Reverse()
                .Concat(new[] { end });
        }

        private static IEnumerable<T> TraverseBack<T>(T end, int pathLength,
            Func<T, IEnumerable<T>> adjacentNodes, Dictionary<T, int> distances)
        {
            var current = end;
            for (var i = pathLength - 1; i >= 0; i--)
                yield return current = adjacentNodes(current)
                    .First(n => distances[n] == i);
        }

        private static void DiscoverNextGeneration<T>(int stepNumber,
            Dictionary<T, int> distances,
            HashSet<T> current, HashSet<T> next,
            Func<T, IEnumerable<T>> adjacentNodes)
        {
            foreach (var node in current)
            {
                distances[node] = stepNumber;
                if (Equals(current, node)) return;
                foreach (var adjacent in adjacentNodes(node))
                    if (!distances.ContainsKey(adjacent))
                        if (!current.Contains(adjacent))
                            next.Add(adjacent);
            }
        }

        private static void ClearAndSwap<T>(ref HashSet<T> current, ref HashSet<T> next)
        {
            current.Clear();
            var temp = current;
            current = next;
            next = temp;
        }
    }
}