using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class FiberEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public static string AddFiber(this Model model, FiberAdded e)
        {
            model.Fibers.Add(Mapper.Map<Fiber>(e));
            return null;
        }

        public static string UpdateFiber(this Model model, FiberUpdated source)
        {
            foreach (var fiberPartId in model.GetAllParts(source.Id))
            {
                var destination = model.Fibers.FirstOrDefault(f => f.FiberId == fiberPartId);
                if (destination == null)
                {
                    return $@"FiberUpdated: Fiber {fiberPartId.First6()} not found";
                }
                Mapper.Map(source, destination);
            }

            return null;
        }

        private static IEnumerable<Guid> GetAllParts(this Model model, Guid fiberPartId)
        {
            var selectedPart = model.Fibers.FirstOrDefault(f => f.FiberId == fiberPartId);
            if (selectedPart == null) yield break;

            yield return selectedPart.FiberId;

            foreach (var guid in model.GetPartFromOneSide(fiberPartId, selectedPart.NodeId1))
                yield return guid;

            foreach (var guid in model.GetPartFromOneSide(fiberPartId, selectedPart.NodeId2))
                yield return guid;
        }

        private static IEnumerable<Guid> GetPartFromOneSide(this Model model, Guid fiberId, Guid nodeId)
        {
            var fId = fiberId;
            var node1 = model.Nodes.First(n => n.NodeId == nodeId);
            while (node1.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                var fiber = model.GetAnotherFiberOfAdjustmentPoint(node1, fId);
                yield return fiber.FiberId;

                var nId = fiber.NodeId1 == node1.NodeId ? fiber.NodeId2 : fiber.NodeId1;
                node1 = model.Nodes.First(n => n.NodeId == nId);
                fId = fiber.FiberId;
            }
        }

        public static string RemoveFiber(this Model model, FiberRemoved e)
        {
            var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (fiber == null)
            {
                return $@"FiberRemoved: Fiber {e.FiberId.First6()} not found";
            }
            model.RemoveFiberUptoRealNodesNotPoints(fiber);
            return null;
        }
    }
}