using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;

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

        public static IEnumerable<Node> GetTraceNodes(this ReadModel readModel, Trace trace)
        {
            try
            {
                return trace.Nodes.Select(i => readModel.Nodes.Single(n => n.Id == i));
            }
            catch (Exception e)
            {
                readModel.LogFile.AppendLine(e.Message);
                return null;
            }

        }
    }
}