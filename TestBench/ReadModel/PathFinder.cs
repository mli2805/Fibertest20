using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.TestBench
{
    public class PathFinder
    {
        private readonly ReadModel _readModel;

        public PathFinder(ReadModel readModel)
        {
            _readModel = readModel;
        }

        private IEnumerable<Guid> GetAdjacentNodes(Guid nodeId)
        {
            foreach (var fiber in _readModel.Fibers)
            {
                if (fiber.Node1 == nodeId)
                    yield return fiber.Node2;
                if (fiber.Node2 == nodeId)
                    yield return fiber.Node1;
            }
        }

        private void FindPathRecursive(Guid end, List<Guid> path)
        {
            var previous = path.Last();

            foreach (var nodeId in GetAdjacentNodes(previous))
            {
                if (nodeId == end)
                {
                    path.Add(end);
                    return;
                }
                if (path.Contains(nodeId))
                    continue;

                path.Add(nodeId);
                FindPathRecursive(end,path);

                if (path.Last() != end)
                    path.Remove(nodeId);
                else return;
            }
        }

        public bool FindPath(Guid start, Guid end, out List<Guid> path)
        {
            path = new List<Guid> {start};

            FindPathRecursive(end, path);
            if (path.Last() != end)
                path = null;
            return path != null;
        }
    }
}
