namespace Iit.Fibertest.Graph.Algorithms
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
            var idxInTrace1 = trace.NodeIds.IndexOf(fiber.NodeId1);
            if (idxInTrace1 == -1)
                return -1;
            var idxInTrace2 = trace.NodeIds.IndexOf(fiber.NodeId2);
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
