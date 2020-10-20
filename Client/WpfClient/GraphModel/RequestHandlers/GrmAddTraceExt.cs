using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public static class GrmAddTraceExt
    {
        public static async Task AddTrace(this GraphReadModel model, RequestAddTrace request)
        {
            if (!model.Validate(request))
                return;

            List<Guid> traceNodes = await model.GetPath(request);
            if (traceNodes == null)
                return;

            var traceId = Guid.NewGuid();
            var fiberIds = model.ReadModel.GetFibersAtTraceCreation(traceNodes).ToList();
            model.ChangeFutureTraceColor(traceId, fiberIds, FiberState.HighLighted);
            try
            {
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation, Resources.SID_Accept_the_path);
                model.WindowManager.ShowDialogWithAssignedOwner(vm);

                if (!vm.IsAnswerPositive) return;

                List<Guid> traceEquipments = model.CollectEquipment(traceNodes);

                if (traceEquipments == null)
                    return;

                var traceAddViewModel = model.GlobalScope.Resolve<TraceInfoViewModel>();
                traceAddViewModel.Initialize(traceId, traceEquipments, traceNodes, true);
                model.WindowManager.ShowDialogWithAssignedOwner(traceAddViewModel);
            }
            finally
            {
                model.ChangeFutureTraceColor(traceId, fiberIds, FiberState.NotInTrace);
            }
        }

        private static bool Validate(this GraphReadModel model, RequestAddTrace request)
        {
            if (model.ReadModel.Equipments.Any(e => e.NodeId == request.LastNodeId && e.Type > EquipmentType.CableReserve)) return true;

            model.WindowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
            return false;
        }

        private static async Task<List<Guid>> GetPath(this GraphReadModel model, RequestAddTrace request)
        {
            var vm = new WaitViewModel();
            vm.Initialize(LongOperation.PathFinding);
            model.WindowManager.ShowWindowWithAssignedOwner(vm);

            var pathFinder = new PathFinder(model);
            var path = await Task.Factory.StartNew(() => pathFinder.FindPath(request.NodeWithRtuId, request.LastNodeId).Result);
         
            vm.TryClose();

            if (path == null)
            {
                var strs = new List<string>()
                {
                    Resources.SID_Path_couldn_t_be_found,
                    "",
                    Resources.SID_Load_additional_data_,
                };
                model.WindowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, strs));
            }

            return path;
        }


        public static List<Guid> CollectEquipment(this GraphReadModel model, List<Guid> nodes)
        {
            var equipments = new List<Guid> { model.ReadModel.Rtus.First(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var equipmentId = model.ChooseEquipmentForNode(nodeId, nodeId == nodes.Last(), out var _);
                if (equipmentId == Guid.Empty)
                    return null;
                equipments.Add(equipmentId);
            }
            return equipments;
        }

        public static Guid ChooseEquipmentForNode(this GraphReadModel model, Guid nodeId, bool isLastNode, out string dualName)
        {
            dualName = null;
            var nodeVm = model.Data.Nodes.First(n => n.Id == nodeId);
            var allEquipmentInNode = model.ReadModel.Equipments.Where(e => e.NodeId == nodeId).ToList();

            if (allEquipmentInNode.Count == 1 && allEquipmentInNode[0].Type == EquipmentType.AdjustmentPoint)
                return allEquipmentInNode[0].EquipmentId;

            if (allEquipmentInNode.Count == 1 && !string.IsNullOrEmpty(nodeVm.Title))
            {
                dualName = nodeVm.Title;
                var equipment =
                    model.ReadModel.Equipments.First(e => e.EquipmentId == allEquipmentInNode[0].EquipmentId);
                if (!string.IsNullOrEmpty(equipment.Title))
                    dualName = dualName + @" / " + equipment.Title;
                return allEquipmentInNode[0].EquipmentId;
            }

            var node = model.ReadModel.Nodes.First(n => n.NodeId == nodeId);
            nodeVm.IsHighlighted = true;
            var traceContentChoiceViewModel = model.GlobalScope.Resolve<TraceContentChoiceViewModel>();
            traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, isLastNode);
            model.WindowManager.ShowDialogWithAssignedOwner(traceContentChoiceViewModel);
            model.ExtinguishNodes();
            if (!traceContentChoiceViewModel.ShouldWeContinue) // user left the process
                return Guid.Empty;

            var selectedEquipmentGuid = traceContentChoiceViewModel.GetSelectedEquipmentGuid();
            dualName = traceContentChoiceViewModel.GetSelectedDualName();
            return selectedEquipmentGuid;

        }

    }
}