﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Graph
{
    public static class Topo
    {
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
