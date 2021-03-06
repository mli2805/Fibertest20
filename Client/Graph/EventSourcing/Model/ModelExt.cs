﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class ModelExt
    {
        public static TraceModelForBaseRef GetTraceComponentsByIds(this Model model, Trace trace)
        {
            var nodes = model.GetTraceNodes(trace).ToArray();
            var rtu = model.Rtus.First(r => r.Id == trace.RtuId);
            var equipList =
                new List<Equipment>()
                {
                    new Equipment() {Type = EquipmentType.Rtu, Title = rtu.Title}
                }; // fake RTU, just for indexes match
            equipList.AddRange(model.GetTraceEquipments(trace).ToList()); // without RTU
            var fibers = model.GetTraceFibers(trace).ToArray();

            return new TraceModelForBaseRef
            {
                NodeArray = nodes,
                EquipArray = equipList.ToArray(),
                FiberArray = fibers,
            };
        }

        public static Guid GetFiberIdBetweenNodes(this Model model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.NodeId1 == node1 && f.NodeId2 == node2 ||
                     f.NodeId1 == node2 && f.NodeId2 == node1).FiberId;
        }

        public static Fiber GetFiberBetweenNodes(this Model model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.NodeId1 == node1 && f.NodeId2 == node2 ||
                     f.NodeId1 == node2 && f.NodeId2 == node1);
        }

        // public static IEnumerable<Guid> GetTraceFiberIdsByNodes(this Model model, List<Guid> nodes)
        // {
        //     for (int i = 1; i < nodes.Count; i++)
        //         yield return model.GetFiberIdBetweenNodes(nodes[i - 1], nodes[i]);
        // }
        //
        // public static IEnumerable<Fiber> GetTraceFibersByNodes(this Model model, List<Guid> nodes)
        // {
        //     for (int i = 1; i < nodes.Count; i++)
        //         yield return model.GetFiberBetweenNodes(nodes[i - 1], nodes[i]);
        // }


        public static IEnumerable<Fiber> GetTraceFibers(this Model model, Trace trace)
        {
            //            return trace.FiberIds.Select(i => model.Fibers.First(f => f.FiberId == i));

            foreach (var fiberId in trace.FiberIds)
            {
                var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
                if (fiber != null)
                    yield return fiber;
            }
        }

        public static IEnumerable<Guid> GetFibersAtTraceCreation(this Model model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
            {
                var fiber = model.Fibers.FirstOrDefault(
                    f => f.NodeId1 == nodes[i - 1] && f.NodeId2 == nodes[i] ||
                         f.NodeId1 == nodes[i] && f.NodeId2 == nodes[i - 1]);

                if (fiber != null)
                    yield return fiber.FiberId;
            }
        }

        public static IEnumerable<Trace> GetTracesPassingFiber(this Model model, Guid fiberId)
        {
            var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
            if (fiber == null) yield break;
            foreach (var pair in fiber.States)
                yield return model.Traces.First(t => t.TraceId == pair.Key);
        }

        private static IEnumerable<Node> GetTraceNodes(this Model model, Trace trace)
        {
            return trace.NodeIds.Select(i => model.Nodes.First(eq => eq.NodeId == i));
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

        // on graph could be more than one fiber between landmarks
        // so we should exclude AdjustmentPoints to find nodes corresponding to landmarks
        // and return all fibers between those nodes
        public static IEnumerable<Guid> GetTraceFibersBetweenLandmarks(this Model model, Guid traceId, int leftLmIndex, int rightLmIndex)
        {
            if (leftLmIndex == -1 || rightLmIndex == -1)
            {
                yield break;
            }
            var trace = model.Traces.First(t => t.TraceId == traceId);
            var nodesWithoutAdjustmentPoints = model.GetTraceNodesExcludingAdjustmentPoints(traceId).ToList();
            var leftNodeId = nodesWithoutAdjustmentPoints[leftLmIndex];
            var rightNodeId = nodesWithoutAdjustmentPoints[rightLmIndex];

            var leftNodeIndexInFull = trace.NodeIds.IndexOf(leftNodeId);
            var rightNodeIndexInFull = trace.NodeIds.IndexOf(rightNodeId);

            for (int i = leftNodeIndexInFull; i < rightNodeIndexInFull; i++)
            {
                yield return model.Fibers.First(
                    f => f.NodeId1 == trace.NodeIds[i] && f.NodeId2 == trace.NodeIds[i + 1] ||
                         f.NodeId1 == trace.NodeIds[i + 1] && f.NodeId2 == trace.NodeIds[i]).FiberId;
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

        public static void GetTraceForAccidentDefine(this Model model, Guid traceId,
            out Rtu rtu, out List<Node> nodes, out List<Equipment> equipments)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            rtu = model.Rtus.First(r => r.Id == trace.RtuId);
            nodes = new List<Node>();
            foreach (var traceNodeId in trace.NodeIds)
            {
                var node = model.Nodes.First(n => n.NodeId == traceNodeId);
                if (node.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint)
                    nodes.Add(node);
            }
            equipments = new List<Equipment>();
            foreach (var equipmentId in trace.EquipmentIds.Skip(1)) // 0 - RTU
            {
                var equipment = model.Equipments.First(e => e.EquipmentId == equipmentId);
                if (equipment.Type != EquipmentType.AdjustmentPoint)
                    equipments.Add(equipment);
            }
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

        public static string ChangeResponsibilities(this Model model, ResponsibilitiesChanged e)
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

            return null;
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