using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public static class GraphReadModelExt
    {
        public static NodeVm GetNodeByLandmarkIndex(this GraphReadModel model, Guid traceId, int landmarkIndex)
        {
            var trace = model.ReadModel.Traces.First(t => t.TraceId == traceId);
            var i = -1;
            foreach (var nodeId in trace.NodeIds)
            {
                var nodeVm = model.Data.Nodes.First(n => n.Id == nodeId);
                if (nodeVm.Type != EquipmentType.AdjustmentPoint) i++;

                if (i == landmarkIndex)
                    return nodeVm;
            }

            return null;
        }

        public static FiberVm GetFiberByLandmarkIndexes(this GraphReadModel model, Guid traceId,
            int leftLandmarkIndex, int rightLandmarkIndex)
        {
            var trace = model.ReadModel.Traces.First(t => t.TraceId == traceId);
            List<Guid> traceNodesWithoutAdjustmentPoints = new List<Guid>();
            foreach (var nodeId in trace.NodeIds)
            {
                var nodeVm = model.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (nodeVm != null && nodeVm.Type != EquipmentType.AdjustmentPoint)
                    traceNodesWithoutAdjustmentPoints.Add(nodeVm.Id);
            }
            return model.GetFiberByNodes(traceNodesWithoutAdjustmentPoints[leftLandmarkIndex],
                traceNodesWithoutAdjustmentPoints[rightLandmarkIndex]);
        }


        private static FiberVm GetFiberByNodes(this GraphReadModel model, Guid node1, Guid node2)
        {
            return model.Data.Fibers.FirstOrDefault(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1);
        }

        // start and finish are NOT included
        public static bool FindPathWhereAdjustmentPointsOnly(this GraphReadModel model, Guid start, Guid finish, out List<Guid> pathNodeIds)
        {
            pathNodeIds = new List<Guid>();

            foreach (var nodeVm in model.GetNeighbours(start))
            {
                pathNodeIds = new List<Guid>();
                var previousNodeId = start;
                var currentNodeVm = nodeVm;

                while (true)
                {
                    if (currentNodeVm.Id == finish) return true;
                    if (currentNodeVm.Type != EquipmentType.AdjustmentPoint) break;

                    pathNodeIds.Add(currentNodeVm.Id);

                    var fiber = model.Data.Fibers.First(f =>
                        f.Node1.Id == currentNodeVm.Id && f.Node2.Id != previousNodeId ||
                        f.Node2.Id == currentNodeVm.Id && f.Node1.Id != previousNodeId);
                    currentNodeVm = fiber.Node1.Id == currentNodeVm.Id ? fiber.Node2 : fiber.Node1;
                }
            }

            return false;
        }

        private static List<NodeVm> GetNeighbours(this GraphReadModel model, Guid nodeId)
        {
            var nodeVms = model.Data.Fibers.Where(f => f.Node1.Id == nodeId).Select(f => f.Node2).ToList();
            nodeVms.AddRange(model.Data.Fibers.Where(f => f.Node2.Id == nodeId).Select(f => f.Node1));
            return nodeVms;
        }

        // if some of neighbours are AdjustmentPoints - step farther a find first node on this way
        public static List<Tuple<NodeVm, List<FiberVm>>> GetNeiboursPassingThroughAdjustmentPoints(this GraphReadModel model, Guid nodeId)
        {
            var res = new List<Tuple<NodeVm, List<FiberVm>>>();

            var fiberVms = model.Data.Fibers.Where(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId);
            foreach (var fiberVm in fiberVms)
            {
                var fibersOfOneDestination = new List<FiberVm>();
                var previousNodeId = nodeId;
                var currentFiberVm = fiberVm;
                NodeVm neighbourVm;
                while (true)
                {
                    fibersOfOneDestination.Add(currentFiberVm);
                    neighbourVm = currentFiberVm.Node1.Id == previousNodeId ? currentFiberVm.Node2 : currentFiberVm.Node1;
                    if (neighbourVm.Type != EquipmentType.AdjustmentPoint)
                        break;

                    previousNodeId = neighbourVm.Id;
                    currentFiberVm = model.GetAnotherFiberOfAdjustmentPoint(neighbourVm, currentFiberVm.Id);
                }
                res.Add(new Tuple<NodeVm, List<FiberVm>>(neighbourVm, fibersOfOneDestination));
            }

            return res;
        }

        public static FiberVm GetAnotherFiberOfAdjustmentPoint(this GraphReadModel model, NodeVm adjustmentPoint, Guid fiberId)
        {
            return model.Data.Fibers.First(f => (f.Node1 == adjustmentPoint || f.Node2 == adjustmentPoint) && f.Id != fiberId);
        }

        public static void CleanAccidentPlacesOnTrace(this GraphReadModel model, Guid traceId)
        {
            var nodeVmsIndexes = new List<int>();
            for (int i = 0; i < model.Data.Nodes.Count; i++)
            {
                if (model.Data.Nodes[i].AccidentOnTraceVmId == traceId)
                    nodeVmsIndexes.Add(i);
            }

            for (int i = nodeVmsIndexes.Count-1; i>= 0 ; i--)
            {
                model.Data.Nodes.RemoveAt(nodeVmsIndexes[i]);
            }

            model.LogFile.AppendLine($@"{nodeVmsIndexes.Count} accident nodes were cleaned");

            foreach (var fiberVm in model.Data.Fibers)
            {
                fiberVm.RemoveBadSegment(traceId);
            }
        }
    }
}