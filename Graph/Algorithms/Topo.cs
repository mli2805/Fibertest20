using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
