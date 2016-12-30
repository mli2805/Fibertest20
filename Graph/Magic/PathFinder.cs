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
            path.Push( start);
            FindPathRecursive(end, path, adjacentNodes);
            return path.Reverse();
        }

        private static void FindPathRecursive<T>(T end, Stack<T> path, 
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
                FindPathRecursive(end, path, adjacentNodes);

                if (Equals(path.Peek(), end)) return;
                path.Pop();
            }
        }
    }
}