using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Graph
{
    public static class Topo
    {
        public static Guid FindFiberByNodes(this ReadModel readModel, Guid leftNode, Guid rightNode)
        {
            foreach (var fiber in readModel.Fibers)
            {
                if ((fiber.Node1 == leftNode || fiber.Node1 == rightNode) &&
                    (fiber.Node2 == leftNode || fiber.Node2 == rightNode))
                    return fiber.Id;

            }
            return Guid.Empty;
        }

        public static IEnumerable<Guid> FindEquipmentsByNode(this ReadModel readModel, Guid nodeId)
        {
            foreach (var equipment in readModel.Equipments)
            {
                if (equipment.NodeId == nodeId)
                    yield return equipment.Id;
            }
        }

        public static Rtu FindRtuByTrace(this ReadModel readModel, Guid traceId)
        {
            var trace = readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            return trace == null ? null : readModel.Rtus.FirstOrDefault(r => r.NodeId == trace.Nodes[0]);
        }

        public static List<Trace> GetTracesWithNodeAtLastPosition(this ReadModel readModel, Guid nodeId)
        {
            return readModel.Traces.Where(t => t.Nodes.Last() == nodeId).ToList();
        }

        public static bool CouldNodeBeRemoved(this ReadModel readModel, Guid nodeId)
        {
            foreach (var trace in readModel.GetTracesWithNodeAtLastPosition(nodeId))
            {
                if (trace.Nodes.Count == 2) return false;
                if (!readModel.FindEquipmentsByNode(trace.Nodes[trace.Nodes.Count - 2]).Any()) return false;
            }
            return true;
        }

    }
}
