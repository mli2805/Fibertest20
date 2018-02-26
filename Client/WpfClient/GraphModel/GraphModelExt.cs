using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Client
{
    public static class GraphModelExt
    {
        public static IEnumerable<FiberVm> GetFibersByNodes(this GraphReadModel model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(model, nodes[i - 1], nodes[i]);
        }

        private static FiberVm GetFiberByNodes(this GraphReadModel model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1);
        }

        public static List<NodeVm> GetNeighbours(this GraphReadModel model, Guid nodeId)
        {
            var nodes = model.Fibers.Where(f => f.Node1.Id == nodeId).Select(f=>f.Node2).ToList();
            nodes.AddRange(model.Fibers.Where(f => f.Node2.Id == nodeId).Select(f => f.Node1));
            return nodes;
        }
    }
}