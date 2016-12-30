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
            if (Equals(start, end)) return new[] { start };
            Dictionary<T, int> distances;
            var maxDistance = DiscoverDistances(start, end, adjacentNodes, out distances);
            if (!distances.ContainsKey(end)) return Enumerable.Empty<T>();
            return TraverseBack(end, maxDistance, adjacentNodes, distances)
                .Reverse()
                .Concat(new[] { end });
        }

        private static int DiscoverDistances<T>(T start, T end,
            Func<T, IEnumerable<T>> adjacentNodes, out Dictionary<T, int> distances)
        {
            distances = new Dictionary<T, int>();
            var current = new HashSet<T> { start };
            var next = new HashSet<T>();
            for (var generation = 0; ; generation++)
            {
                foreach (var node in current)
                {
                    distances[node] = generation;
                    if (Equals(node, end)) return generation;
                }

                NextGeneration(distances, current, next, adjacentNodes);
                if (next.Count == 0) return generation;
                ClearCurrentAndSwapItWithNext(ref current, ref next);
            }
        }

        private static IEnumerable<T> TraverseBack<T>(T end, int pathLength,
            Func<T, IEnumerable<T>> adjacentNodes, Dictionary<T, int> distances)
        {
            var current = end;
            for (var i = pathLength - 1; i >= 0; i--)
                yield return current = adjacentNodes(current)
                    .First(n => distances[n] == i);
        }

        /// <summary>Discover all the new nodes adjacent to the nodes in the current generation
        ///     and push them into the <paramref name="nextGeneration"/> list
        /// </summary>
        private static void NextGeneration<T>(Dictionary<T, int> distances,
            HashSet<T> currentGeneration, HashSet<T> nextGeneration,
            Func<T, IEnumerable<T>> adjacentNodes)
        {
            foreach (var node in currentGeneration)
                foreach (var adjacent in adjacentNodes(node))
                    if (!distances.ContainsKey(adjacent))
                        if (!currentGeneration.Contains(adjacent))
                            nextGeneration.Add(adjacent);
        }

        private static void ClearCurrentAndSwapItWithNext<T>(ref HashSet<T> current, ref HashSet<T> next)
        {
            current.Clear();
            var temp = current;
            current = next;
            next = temp;
        }
    }
}