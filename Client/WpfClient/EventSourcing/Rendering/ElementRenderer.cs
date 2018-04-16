using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class ElementRenderer
    {
        public static NodeVm Map(Node node)
        {
            return new NodeVm()
            {
                Id = node.NodeId,
                Title = node.Title,
                Position = node.Position,
                Type = node.TypeOfLastAddedEquipment,
                AccidentOnTraceVmId = node.AccidentOnTraceId,
                State = node.State,
            };
        }

        public static FiberVm Map(Fiber fiber, List<NodeVm> nodesForRendering)
        {
            return new FiberVm()
            {
                Id = fiber.FiberId,
                Node1 = nodesForRendering.First(n => n.Id == fiber.NodeId1),
                Node2 = nodesForRendering.First(n => n.Id == fiber.NodeId2),
                States = new Dictionary<Guid, FiberState>(),
                TracesWithExceededLossCoeff = new Dictionary<Guid, FiberState>(),
            };
        }
    }
}