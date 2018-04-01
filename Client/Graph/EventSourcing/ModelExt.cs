using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Graph
{
    public static class ModelExt
    {
        public static TraceModelForBaseRef GetTraceComponentsByIds(this Model model, Trace trace)
        {
            var nodes = model.GetTraceNodes(trace).ToArray();
            var equipList =
                new List<Equipment>() { new Equipment() { Type = EquipmentType.Rtu } }; // fake RTU, just for indexes match
            equipList.AddRange(model.GetTraceEquipments(trace).ToList()); // without RTU
            var fibers = model.GetTraceFibers(trace).ToArray();

            return new TraceModelForBaseRef
            {
                NodeArray = nodes,
                EquipArray = equipList.ToArray(),
                FiberArray = fibers,
            };
        }

        public static IEnumerable<Fiber> GetTraceFibers(this Model model, Trace trace)
        {
            return model.GetFibersByNodes(trace.NodeIds).Select(i => model.Fibers.Single(f => f.FiberId == i));
        }

        public static IEnumerable<Guid> GetFibersByNodes(this Model model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(model, nodes[i - 1], nodes[i]);
        }

        public static Guid GetFiberByNodes(this Model model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.NodeId1 == node1 && f.NodeId2 == node2 ||
                     f.NodeId1 == node2 && f.NodeId2 == node1).FiberId;
        }

        public static IEnumerable<Trace> GetTracesPassingFiber(this Model model, Guid fiberId)
        {
            foreach (var trace in model.Traces)
            {
                if (model.GetFibersByNodes(trace.NodeIds).Contains(fiberId))
                    yield return trace;
            }
        }

        public static IEnumerable<Node> GetTraceNodes(this Model model, Trace trace)
        {
            return trace.NodeIds.Select(i => model.Nodes.Single(eq => eq.NodeId == i));
        }

        public static IEnumerable<Guid> GetTraceNodesExcludingAdjustmentPoints(this Model model, Guid traceId)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            foreach (var nodeId in trace.NodeIds)
            {
                var node = model.Nodes.FirstOrDefault(n =>
                    n.NodeId == nodeId && n.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint);
                if (node != null)
                    yield return node.NodeId;
            }
        }

        public static IEnumerable<Equipment> GetTraceEquipments(this Model model, Trace trace)
        {
            return trace.EquipmentIds.Skip(1).Select(i => model.Equipments.Single(eq => eq.EquipmentId == i));
        }

        public static IEnumerable<Equipment> GetTraceEquipmentsExcludingAdjustmentPoints(this Model model, Guid traceId)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            foreach (var equipmentId in trace.EquipmentIds.Skip(1)) // 0 - RTU
            {
                var equipment = model.Equipments.First(e => e.EquipmentId == equipmentId);
                if (equipment.Type != EquipmentType.AdjustmentPoint)
                    yield return equipment;
            }
        }

        private static IEnumerable<Fiber> GetNodeFibers(this Model model, Node node)
        {
            foreach (var fiber in model.Fibers)
                if (fiber.NodeId1 == node.NodeId || fiber.NodeId2 == node.NodeId) yield return fiber;
        }

        public static Fiber GetAnotherFiberOfAdjustmentPoint(this Model model, Node adjustmentPoint, Guid fiberId)
        {
            return model.GetNodeFibers(adjustmentPoint).First(f => f.FiberId != fiberId);
        }

        public static List<Guid> GetNeighbours(this Model model, Guid nodeId)
        {
            var nodes = model.Fibers.Where(f => f.NodeId1 == nodeId).Select(f => f.NodeId2).ToList();
            nodes.AddRange(model.Fibers.Where(f => f.NodeId2 == nodeId).Select(f => f.NodeId1));
            return nodes;
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

        public static void ChangeResponsibilities(this Model model, ResponsibilitiesChanged e)
        {
            foreach (var pair in e.ResponsibilitiesDictionary)
            {
                var rtu = model.Rtus.FirstOrDefault(r => r.Id == pair.Key);
                if (rtu != null)
                {
                    rtu.ZoneIds = ApplyChanges(rtu.ZoneIds, pair.Value);
                    continue;
                }

                var trace = model.Traces.First(t => t.TraceId == pair.Key);
                trace.ZoneIds = ApplyChanges(trace.ZoneIds, pair.Value);
            }
        }

        private static List<Guid> ApplyChanges(List<Guid> oldList, List<Guid> changes)
        {
            var newList = oldList;
            foreach (var zoneId in changes)
            {
                if (newList.Contains(zoneId))
                    newList.Remove(zoneId);
                else
                    newList.Add(zoneId);
            }
            return newList;
        }

        public static string RemoveNodeWithAllHis(this Model model, Guid nodeId)
        {
            model.Fibers.RemoveAll(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId);
            model.Equipments.RemoveAll(e => e.NodeId == nodeId);
            var node = model.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node != null)
            {
                model.Nodes.Remove(node);
                return null;
            }

            return $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
        }

    }
}