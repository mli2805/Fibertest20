using System;
using System.Linq;

namespace Iit.Fibertest.Graph
{
    public static class ModelTryGetExt
    {
        public static bool TryGetRtu(this Model model, Guid rtuId, out Rtu rtu)
        {
            rtu = model.Rtus.FirstOrDefault(r => r.Id == rtuId);
            return rtu != null;
        }

        public static bool TryGetTrace(this Model model, Guid traceId, out Trace trace)
        {
            trace = model.Traces.FirstOrDefault(t => t.TraceId == traceId);
            return trace != null;
        }
    }
}