using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Client
{
    public static class ReadModelExtensions
    {
        public static IEnumerable<Guid> GetFibersByNodes(this ReadModel readModel, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(readModel, nodes[i - 1], nodes[i]);
        }

        private static Guid GetFiberByNodes(this ReadModel readModel, Guid node1, Guid node2)
        {
            return readModel.Fibers.First(
                f => f.Node1 == node1 && f.Node2 == node2 ||
                     f.Node1 == node2 && f.Node2 == node1).Id;
        }
    }
}