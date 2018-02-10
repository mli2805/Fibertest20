using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class GraphModelExt
    {
        public static IEnumerable<Guid> GetFibersByNodes(this GraphReadModel model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(model, nodes[i - 1], nodes[i]);
        }

        private static Guid GetFiberByNodes(this GraphReadModel model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1).Id;
        }
    }
}