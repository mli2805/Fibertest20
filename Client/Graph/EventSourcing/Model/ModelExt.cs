using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class ModelExt
    {
        public static Guid GetFiberIdBetweenNodes(this Model model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.NodeId1 == node1 && f.NodeId2 == node2 ||
                     f.NodeId1 == node2 && f.NodeId2 == node1).FiberId;
        }
      
        private static IEnumerable<Fiber> GetNodeFibers(this Model model, Node node)
        {
            foreach (var fiber in model.Fibers)
                if (fiber.NodeId1 == node.NodeId || fiber.NodeId2 == node.NodeId)
                    yield return fiber;
        }

        public static Fiber GetAnotherFiberOfAdjustmentPoint(this Model model, Node adjustmentPoint, Guid fiberId)
        {
            return model.GetNodeFibers(adjustmentPoint).First(f => f.FiberId != fiberId);
        }

        public static void RemoveFiberUptoRealNodesNotPoints(this Model model, Fiber fiber)
        {
            var leftNode = model.Nodes.First(n => n.NodeId == fiber.NodeId1);
            while (leftNode.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                var leftFiber = model.GetAnotherFiberOfAdjustmentPoint(leftNode, fiber.FiberId);
                model.Nodes.Remove(leftNode);
                var nextLeftNodeId = leftFiber.NodeId1 == leftNode.NodeId ? leftFiber.NodeId2 : leftFiber.NodeId1;
                model.Fibers.Remove(leftFiber);
                leftNode = model.Nodes.First(n => n.NodeId == nextLeftNodeId);
            }

            var rightNode = model.Nodes.First(n => n.NodeId == fiber.NodeId2);
            while (rightNode.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                var rightFiber = model.GetAnotherFiberOfAdjustmentPoint(rightNode, fiber.FiberId);
                model.Nodes.Remove(rightNode);
                var nextRightNodeId = rightFiber.NodeId1 == rightNode.NodeId ? rightFiber.NodeId2 : rightFiber.NodeId1;
                model.Fibers.Remove(rightFiber);
                rightNode = model.Nodes.First(n => n.NodeId == nextRightNodeId);
            }

            model.Fibers.Remove(fiber);
        }

        public static string RemoveNodeWithAllHisFibersUptoRealNode(this Model model, Guid nodeId)
        {
            foreach (var fiber in model.Fibers.Where(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId).ToList())
            {
                var fiberForDeletion = fiber;
                var nodeForDeletionId = nodeId;
                while (true)
                {
                    var anotherNodeId = fiberForDeletion.NodeId1 == nodeForDeletionId
                        ? fiberForDeletion.NodeId2
                        : fiberForDeletion.NodeId1;
                    model.Fibers.Remove(fiberForDeletion);
                    if (!model.IsAdjustmentPoint(anotherNodeId)) break;

                    fiberForDeletion =
                        model.Fibers.First(f => f.NodeId1 == anotherNodeId || f.NodeId2 == anotherNodeId);
                    model.Nodes.RemoveAll(n => n.NodeId == anotherNodeId);
                    model.Equipments.RemoveAll(e => e.NodeId == anotherNodeId);
                    nodeForDeletionId = anotherNodeId;
                }
            }

            model.Equipments.RemoveAll(e => e.NodeId == nodeId);
            model.Nodes.RemoveAll(n => n.NodeId == nodeId);
            return null;
        }

        public static string RemoveNodeWithAllHisFibers(this Model model, Guid nodeId)
        {
            model.Fibers.RemoveAll(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId);
            model.Equipments.RemoveAll(e => e.NodeId == nodeId);
            model.Nodes.RemoveAll(n => n.NodeId == nodeId);
            return null;
        }

        private static bool IsAdjustmentPoint(this Model model, Guid nodeId)
        {
            return model.Equipments.FirstOrDefault(e =>
                       e.NodeId == nodeId && e.Type == EquipmentType.AdjustmentPoint) != null;
        }

        // returns true if there's a fiber between start and finish or they are separated by adjustment points only
        public static bool HasDirectFiberDontMindPoints(this Model model, Guid start, Guid finish)
        {
            foreach (var neighbourNodeId in model.Fibers.Where(f => f.NodeId1 == start || f.NodeId2 == start)
                .Select(n => n.NodeId1 == start ? n.NodeId2 : n.NodeId1))
            {
                var previousNodeId = start;
                var currentNodeId = neighbourNodeId;

                while (true)
                {
                    if (currentNodeId == finish) return true;
                    if (!model.IsAdjustmentPoint(currentNodeId)) break;

                    var fiber = model.Fibers.First(f => f.NodeId1 == currentNodeId && f.NodeId2 != previousNodeId
                                                        || f.NodeId2 == currentNodeId && f.NodeId1 != previousNodeId);
                    previousNodeId = currentNodeId;
                    currentNodeId = fiber.NodeId1 == currentNodeId ? fiber.NodeId2 : fiber.NodeId1;
                }
            }

            return false;
        }
    }
}