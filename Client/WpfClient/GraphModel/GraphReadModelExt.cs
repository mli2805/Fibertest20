using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public static class GraphReadModelExt
    {
        public static IEnumerable<FiberVm> GetFibersByNodes(this GraphReadModel model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(model, nodes[i - 1], nodes[i]);
        }

        public static NodeVm GetNodeByLandmarkIndex(this GraphReadModel model, TraceVm traceVm, int landmarkIndex)
        {
            var i = -1;
            foreach (var nodeId in traceVm.Nodes)
            {
                var nodeVm = model.Data.Nodes.First(n => n.Id == nodeId);
                if (nodeVm.Type != EquipmentType.AdjustmentPoint) i++;

                if (i == landmarkIndex)
                    return nodeVm;
            }

            return null;
        }

        public static FiberVm GetFiberByLandmarkIndexes(this GraphReadModel model, TraceVm traceVm,
            int leftLandmarkIndex, int rightLandmarkIndex)
        {
            List<Guid> traceNodesWithoutAdjustmentPoints = new List<Guid>();
            foreach (var nodeId in traceVm.Nodes)
            {
                var nodeVm = model.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (nodeVm != null && nodeVm.Type != EquipmentType.AdjustmentPoint)
                    traceNodesWithoutAdjustmentPoints.Add(nodeVm.Id);
            }
            return model.GetFiberByNodes(traceNodesWithoutAdjustmentPoints[leftLandmarkIndex],
                traceNodesWithoutAdjustmentPoints[rightLandmarkIndex]);
        }


        public static FiberVm GetFiberByNodes(this GraphReadModel model, Guid node1, Guid node2)
        {
            return model.Data.Fibers.FirstOrDefault(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1);
        }

    
        public static List<NodeVm> GetNeighbours(this GraphReadModel model, Guid nodeId)
        {
            var nodes = model.Data.Fibers.Where(f => f.Node1.Id == nodeId).Select(f => f.Node2).ToList();
            nodes.AddRange(model.Data.Fibers.Where(f => f.Node2.Id == nodeId).Select(f => f.Node1));
            return nodes;
        }

        public static void CleanAccidentPlacesOnTrace(this GraphReadModel model, TraceVm traceVm)
        {
            var nodeVms = model.Data.Nodes.Where(n => n.AccidentOnTraceVmId == traceVm.Id).ToList();
            foreach (var nodeVm in nodeVms)
            {
                model.Data.Nodes.Remove(nodeVm);
            }

            foreach (var fiberVm in model.Data.Fibers)
            {
                fiberVm.CleanBadSegment(traceVm.Id);
            }
        }
    }
}