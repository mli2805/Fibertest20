using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public static class GrmAddTraceExt
    {
        public static void AddTrace(this GraphReadModel model, RequestAddTrace request)
        {
            if (!model.Validate(request))
                return;

            List<Guid> traceNodes = model.GetPath(request);
            if (traceNodes == null)
                return;

            var traceId = Guid.NewGuid();
            model.ChangeTraceColor(traceId, traceNodes, FiberState.HighLighted);
            try
            {
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation, Resources.SID_Accept_the_path);
                model.WindowManager.ShowDialogWithAssignedOwner(vm);

                if (!vm.IsAnswerPositive) return;

                List<Guid> traceEquipments = model.CollectEquipment(traceNodes);

                if (traceEquipments == null)
                    return;

                var traceAddViewModel = model.GlobalScope.Resolve<TraceInfoViewModel>();
                traceAddViewModel.Initialize(Guid.Empty, traceEquipments, traceNodes);
                model.WindowManager.ShowDialogWithAssignedOwner(traceAddViewModel);
            }
            finally
            {
                model.ChangeTraceColor(traceId, traceNodes, FiberState.NotInTrace);
            }
        }

        private static bool Validate(this GraphReadModel model, RequestAddTrace request)
        {
            if (model.ReadModel.Equipments.Any(e => e.NodeId == request.LastNodeId && e.Type > EquipmentType.CableReserve)) return true;

            model.WindowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
            return false;
        }

        private static List<Guid> GetPath(this GraphReadModel model, RequestAddTrace request)
        {
            List<Guid> path;
            if (!new PathFinder(model.ReadModel).FindPath(request.NodeWithRtuId, request.LastNodeId, out path))
                model.WindowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Path_couldn_t_be_found));

            return path;
        }


        public static List<Guid> CollectEquipment(this GraphReadModel model, List<Guid> nodes)
        {
            var equipments = new List<Guid> { model.ReadModel.Rtus.First(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var equipmentId = model.ChooseEquipmentForNode(nodeId, nodeId == nodes.Last());
                if (equipmentId == Guid.Empty)
                    return null;
                equipments.Add(equipmentId);
            }
            return equipments;
        }

        public static Guid ChooseEquipmentForNode(this GraphReadModel model, Guid nodeId, bool isLastNode)
        {
            var nodeVm = model.Data.Nodes.First(n => n.Id == nodeId);
            var allEquipmentInNode = model.ReadModel.Equipments.Where(e => e.NodeId == nodeId).ToList();
            if (allEquipmentInNode.Count == 1 &&
                (allEquipmentInNode[0].Type == EquipmentType.AdjustmentPoint || !string.IsNullOrEmpty(nodeVm.Title)))
            {
                return allEquipmentInNode[0].Id;
            }

            var node = model.ReadModel.Nodes.First(n => n.Id == nodeId);
            nodeVm.IsHighlighted = true;
            var traceContentChoiceViewModel = model.GlobalScope.Resolve<TraceContentChoiceViewModel>();
            traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, isLastNode);
            model.WindowManager.ShowDialogWithAssignedOwner(traceContentChoiceViewModel);
            model.ExtinguishNode();
            if (!traceContentChoiceViewModel.ShouldWeContinue) // user left the process
                return Guid.Empty;

            var selectedEquipmentGuid = traceContentChoiceViewModel.GetSelectedEquipmentGuid();
            return selectedEquipmentGuid;

        }

    }
}