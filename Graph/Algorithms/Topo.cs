using System;
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

        public static Rtu FindRtuByTrace(this ReadModel readModel, Guid traceId)
        {
            var trace = readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            return trace == null ? null : readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trace">Trace could contain the same fiber more then once</param>
        /// <param name="fiber">The object to locate</param>
        /// <returns>The zero-based index of the first occurence of the fiber within the entire list of fibers in trace if found; otherwise, -1</returns>
        public static int GetFiberIndexInTrace(Trace trace, Fiber fiber)
        {
            var idxInTrace1 = trace.Nodes.IndexOf(fiber.Node1);
            if (idxInTrace1 == -1)
                return -1;
            var idxInTrace2 = trace.Nodes.IndexOf(fiber.Node2);
            if (idxInTrace2 == -1)
                return -1;
            if (idxInTrace2 - idxInTrace1 == 1)
                return idxInTrace1;
            if (idxInTrace1 - idxInTrace2 == 1)
                return idxInTrace2;
            return -1;
        }

    }
}
