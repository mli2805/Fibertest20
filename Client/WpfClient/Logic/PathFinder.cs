using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Client
{
    public class PathFinder
    {
        private readonly GraphReadModel _model;

        public PathFinder(GraphReadModel model)
        {
            _model = model;
        }

        private IEnumerable<Guid> GetAdjacentNodes(Guid nodeId)
        {
            foreach (var fiber in _model.Data.Fibers)
            {
                if (fiber.Node1.Id == nodeId)
                    yield return fiber.Node2.Id;
                if (fiber.Node2.Id == nodeId)
                    yield return fiber.Node1.Id;
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
                FindPathRecursive(end, path);

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