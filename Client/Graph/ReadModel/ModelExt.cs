using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Graph
{
    public static class ModelExt
    {
        public static TraceModelForBaseRef GetTraceComponentsByIds(this IModel model, Trace trace)
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

        public static IEnumerable<Fiber> GetTraceFibers(this IModel model, Trace trace)
        {
            return model.GetFibersByNodes(trace.Nodes).Select(i => model.Fibers.Single(f=>f.Id == i));
        }

        public static IEnumerable<Guid> GetFibersByNodes(this IModel model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(model, nodes[i - 1], nodes[i]);
        }

        public static Guid GetFiberByNodes(this IModel model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.Node1 == node1 && f.Node2 == node2 ||
                     f.Node1 == node2 && f.Node2 == node1).Id;
        }

        public static IEnumerable<Trace> GetTracesPassingFiber(this IModel model, Guid fiberId)
        {
            foreach (var trace in model.Traces)
            {
                if (model.GetFibersByNodes(trace.Nodes).Contains(fiberId))
                    yield return trace;
            }
        }

        public static IEnumerable<Node> GetTraceNodes(this IModel model, Trace trace)
        {
            try
            {
                return trace.Nodes.Select(i => model.Nodes.Single(eq => eq.Id == i));
            }
            catch (Exception e)
            {
                model.LogFile.AppendLine(e.Message);
                return null;
            }

        }

        public static IEnumerable<Guid> GetTraceNodesExcludingAdjustmentPoints(this IModel model, Guid traceId)
        {
                var trace = model.Traces.First(t => t.Id == traceId);
                foreach (var nodeId in trace.Nodes)
                {
                    var node = model.Nodes.FirstOrDefault(n =>
                        n.Id == nodeId && n.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint);
                    if (node != null)
                        yield return node.Id;
                }
        }

        public static IEnumerable<Equipment> GetTraceEquipments(this IModel model, Trace trace)
        {
            try
            {
                return trace.Equipments.Skip(1).Select(i => model.Equipments.Single(eq => eq.Id == i));
            }
            catch (Exception e)
            {
                model.LogFile.AppendLine(e.Message);
                return null;
            }
        }

        public static IEnumerable<Equipment> GetTraceEquipmentsExcludingAdjustmentPoints(this IModel model, Guid traceId)
        {
            var trace = model.Traces.First(t => t.Id == traceId);
            foreach (var equipmentId in trace.Equipments.Skip(1)) // 0 - RTU
            {
                var equipment = model.Equipments.First(e => e.Id == equipmentId);
                if (equipment.Type != EquipmentType.AdjustmentPoint)
                    yield return equipment;
            }
        }

        private static IEnumerable<Fiber> GetNodeFibers(this IModel model, Node node)
        {
            foreach (var fiber in model.Fibers)
                if (fiber.Node1 == node.Id || fiber.Node2 == node.Id) yield return fiber;
        }

        public static Fiber GetAnotherFiberOfAdjustmentPoint(this IModel model, Node adjustmentPoint, Guid fiberId)
        {
            return model.GetNodeFibers(adjustmentPoint).First(f => f.Id != fiberId);
        }

        public static List<Guid> GetNeighbours(this IModel model, Guid nodeId)
        {
            var nodes = model.Fibers.Where(f => f.Node1 == nodeId).Select(f => f.Node2).ToList();
            nodes.AddRange(model.Fibers.Where(f => f.Node2 == nodeId).Select(f => f.Node1));
            return nodes;
        }


        public static void RemoveFiberUptoRealNodesNotPoints(this IModel model, Fiber fiber)
        {
            var leftNode = model.Nodes.First(n => n.Id == fiber.Node1);
            while (leftNode.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                var leftFiber = model.GetAnotherFiberOfAdjustmentPoint(leftNode, fiber.Id);
                model.Nodes.Remove(leftNode);
                var nextLeftNodeId = leftFiber.Node1 == leftNode.Id ? leftFiber.Node2 : leftFiber.Node1;
                model.Fibers.Remove(leftFiber);
                leftNode = model.Nodes.First(n => n.Id == nextLeftNodeId);
            }

            var rightNode = model.Nodes.First(n => n.Id == fiber.Node2);
            while (rightNode.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                var rightFiber = model.GetAnotherFiberOfAdjustmentPoint(rightNode, fiber.Id);
                model.Nodes.Remove(rightNode);
                var nextRightNodeId = rightFiber.Node1 == rightNode.Id ? rightFiber.Node2 : rightFiber.Node1;
                model.Fibers.Remove(rightFiber);
                rightNode = model.Nodes.First(n => n.Id == nextRightNodeId);
            }

            model.Fibers.Remove(fiber);
        }

        public static void ChangeResponsibilities(this IModel model, ResponsibilitiesChanged e)
        {
            foreach (var pair in e.ResponsibilitiesDictionary)
            {
                var rtu = model.Rtus.FirstOrDefault(r => r.Id == pair.Key);
                if (rtu != null)
                {
                    rtu.ZoneIds = ApplyChanges(rtu.ZoneIds, pair.Value);
                    continue;
                }

                var trace = model.Traces.First(t => t.Id == pair.Key);
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
    }
}