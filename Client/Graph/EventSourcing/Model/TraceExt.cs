using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class TraceExt
    {
        public static TraceDetalization GetTraceDetalization(this Model readModel, Trace trace, EquipmentType upToType)
        {
            var tEquips = trace.EquipmentIds.Skip(1)
                .Select(i => readModel.Equipments.Single(eq => eq.EquipmentId == i)).ToList();

            var nodes = new List<Guid>() { trace.NodeIds[0] }; // RTU
            var fibers = new List<Guid>();
            var previousNode = 0;

            for (int i = 0; i < tEquips.Count; i++)
            {
                if (tEquips[i].Type >= upToType || i == tEquips.Count - 1 && upToType == EquipmentType.RtuAndEot)
                {
                    nodes.Add(trace.NodeIds[i]);
                    if (i - previousNode == 1)
                    {
                        fibers.Add(trace.FiberIds[i-1]);
                    }
                    else
                    {
                        fibers.Add(Guid.NewGuid());
                    }
                }
            }


            return new TraceDetalization() { Nodes = nodes, Fibers = fibers };
        }
    }
}
