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
            this T start, T end, Func<T, IEnumerable<T>> getAdjacentNodes)
        {
            if (Equals(start, end)) return new[] { start };
            Dictionary<T, int> distances;
            var maxDistance = DiscoverDistances(start, end, getAdjacentNodes, out distances);
            return distances.ContainsKey(end) 
                ? TraverseBack(end, maxDistance, getAdjacentNodes, distances).Reverse() 
                : Enumerable.Empty<T>();
        }

        private static int DiscoverDistances<T>(T start, T end,
            Func<T, IEnumerable<T>> getAdjacentNodes, out Dictionary<T, int> distancesToNodes)
        {
            distancesToNodes = new Dictionary<T, int>();
            var currentGeneration = new HashSet<T> { start };
            for (var distance = 0; currentGeneration.Count != 0; distance++)
            {
                foreach (var node in currentGeneration)
                    distancesToNodes[node] = distance;

                if (currentGeneration.Contains(end))
                    return distance;

                currentGeneration = NextGeneration(currentGeneration, distancesToNodes, getAdjacentNodes)
                    .ToHashSet();
            }
            return -1;
        }

        private static IEnumerable<T> TraverseBack<T>(T current, int pathLength,
            Func<T, IEnumerable<T>> adjacentNodes, Dictionary<T, int> distances)
        {
            yield return current;
            for (var i = pathLength - 1; i >= 0; i--)
                yield return current = adjacentNodes(current)
                    .First(n => distances[n] == i);
        }

        private static IEnumerable<T> NextGeneration<T>(HashSet<T> source, Dictionary<T, int> distances, Func<T, IEnumerable<T>> adjacentNodes)
        {
            return from node in source
                from adjacent in adjacentNodes(node)
                where !distances.ContainsKey(adjacent)
                where !source.Contains(adjacent)
                select adjacent;
        }
        private static HashSet<T> ToHashSet<T>(this IEnumerable<T> src)
            => new HashSet<T>(src);
    }
}