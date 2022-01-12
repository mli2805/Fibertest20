using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public static class GraphReadModelExt
    {
        // start and finish are NOT included
        public static bool FindPathWhereAdjustmentPointsOnly(this GraphReadModel model, Guid start, Guid finish, out List<Guid> pathNodeIds)
        {
            pathNodeIds = new List<Guid>();

            foreach (var nodeVm in model.GetNeighbours(start))
            {
                pathNodeIds.Clear();
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
                    previousNodeId = currentNodeVm.Id;
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

        public static FiberVm GetAnotherFiberOfAdjustmentPoint(this GraphReadModel model, NodeVm adjustmentPoint, Guid fiberId)
        {
            return model.Data.Fibers.First(f => (f.Node1.Id == adjustmentPoint.Id || f.Node2.Id == adjustmentPoint.Id) && f.Id != fiberId);
        }

        public static void CleanAccidentPlacesOnTrace(this GraphReadModel model, Guid traceId)
        {
            var nodeVmsIndexes = new List<int>();
            for (int i = 0; i < model.Data.Nodes.Count; i++)
            {
                if (model.Data.Nodes[i].AccidentOnTraceVmId == traceId)
                    nodeVmsIndexes.Add(i);
            }

            for (int i = nodeVmsIndexes.Count - 1; i >= 0; i--)
            {
                model.Data.Nodes.RemoveAt(nodeVmsIndexes[i]);
            }

            model.LogFile.AppendLine($@"{nodeVmsIndexes.Count} accident nodes were cleaned");

            foreach (var fiberVm in model.Data.Fibers)
            {
                fiberVm.RemoveBadSegment(traceId);
            }
        }

        public static Guid ChooseEquipmentForNode(this GraphReadModel model, Guid nodeId, bool isLastNode, out string dualName)
        {
            dualName = null;
            var node = model.ReadModel.Nodes.First(n => n.NodeId == nodeId);
            var nodeVm = model.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (nodeVm != null)
                nodeVm.IsHighlighted = true;

            var allEquipmentInNode = model.ReadModel.Equipments.Where(e => e.NodeId == nodeId).ToList();

            if (allEquipmentInNode.Count == 1 && allEquipmentInNode[0].Type == EquipmentType.AdjustmentPoint)
                return allEquipmentInNode[0].EquipmentId;

            if (allEquipmentInNode.Count == 1 && !string.IsNullOrEmpty(node.Title))
            {
                dualName = node.Title;
                var equipment =
                    model.ReadModel.Equipments.First(e => e.EquipmentId == allEquipmentInNode[0].EquipmentId);
                if (!string.IsNullOrEmpty(equipment.Title))
                    dualName = dualName + @" / " + equipment.Title;
                return allEquipmentInNode[0].EquipmentId;
            }

            var traceContentChoiceViewModel = model.GlobalScope.Resolve<TraceContentChoiceViewModel>();
            traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, isLastNode);
            model.WindowManager.ShowDialogWithAssignedOwner(traceContentChoiceViewModel);
            model.ExtinguishAllNodes();
            if (!traceContentChoiceViewModel.ShouldWeContinue) // user left the process
                return Guid.Empty;

            var selectedEquipmentGuid = traceContentChoiceViewModel.GetSelectedEquipmentGuid();
            dualName = traceContentChoiceViewModel.GetSelectedDualName();
            return selectedEquipmentGuid;

        }
    }
}