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
            var fibers = new List<Fiber>();
            var previousNode = 0;

            for (int i = 1; i <= tEquips.Count; i++)
            {
                if (tEquips[i-1].Type >= upToType || i == tEquips.Count && upToType == EquipmentType.RtuAndEot)
                {
                    nodes.Add(trace.NodeIds[i]);
                    if (i - previousNode == 1)
                    {
                        var fiber = readModel.Fibers.First(f => f.FiberId == trace.FiberIds[i - 1]);
                        fibers.Add(fiber);
                    }
                    else
                    {
                        var fiber = new Fiber() 
                            {FiberId = Guid.NewGuid(), NodeId1 = nodes[nodes.Count-2], NodeId2 = nodes[nodes.Count-1]};
                        for (int j = previousNode; j < i; j++)
                        {
                            var fiber1 = readModel.Fibers.First(f => f.FiberId == trace.FiberIds[j]);
                            foreach (var fiber1State in fiber1.States)
                            {
                                if (!fiber.States.ContainsKey(fiber1State.Key))
                                    fiber.States.Add(fiber1State.Key, fiber1State.Value);
                                else if (fiber1State.Value > fiber.States[fiber1State.Key])
                                    fiber.States[fiber1State.Key] = fiber1State.Value;
                            }
                        }

                        fibers.Add(fiber);
                    }
                    previousNode = i;
                }
            }

            return new TraceDetalization() { Nodes = nodes, Fibers = fibers };
        }
    }
}
