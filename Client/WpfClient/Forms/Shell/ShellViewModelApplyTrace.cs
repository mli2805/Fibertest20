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
    public partial class ShellViewModel
    {
        private void PrepareCommand(RequestAddTrace request)
        {
            if (!Validate(request))
                return;

            List<Guid> traceNodes = GetPath(request);
            if (traceNodes == null)
                return;

            var traceId = Guid.NewGuid();
            ChangeTraceColor(traceId, traceNodes, FiberState.HighLighted);

            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, Resources.SID_Accept_the_path);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            ChangeTraceColor(traceId, traceNodes, FiberState.NotInTrace);

            if (!vm.IsAnswerPositive)
                return;

            List<Guid> traceEquipments = CollectEquipment(traceNodes);
            if (traceEquipments == null)
                return;

            var traceAddViewModel = GlobalScope.Resolve<TraceInfoViewModel>();
            traceAddViewModel.Initialize(Guid.Empty, traceEquipments, traceNodes);
            _windowManager.ShowDialogWithAssignedOwner(traceAddViewModel);
        }

        private bool Validate(RequestAddTrace request)
        {
            if (ReadModel.Equipments.Any(e => e.NodeId == request.LastNodeId && e.Type > EquipmentType.CableReserve)) return true;

            _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
            return false;
        }

        private List<Guid> GetPath(RequestAddTrace request)
        {
            List<Guid> path;
            if (!new PathFinder(ReadModel).FindPath(request.NodeWithRtuId, request.LastNodeId, out path))
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Path_couldn_t_be_found));

            return path;
        }

        private void ChangeTraceColor(Guid traceId, List<Guid> nodes, FiberState state)
        {
            var fibers = ReadModel.GetFibersByNodes(nodes);
            foreach (var fiber in fibers)
            {
                var fiberVm = GraphReadModel.Fibers.First(f => f.Id == fiber);
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

        public List<Guid> CollectEquipment(List<Guid> nodes)
        {
            var equipments = new List<Guid> { ReadModel.Rtus.First(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var allEquipmentInNode = ReadModel.Equipments.Where(e => e.NodeId == nodeId).ToList();
                if (allEquipmentInNode.Count == 1) // EmptyNode or AdjustmentPoint
                {
                    equipments.Add(allEquipmentInNode[0].Id);
                    continue;
                }
                var node = ReadModel.Nodes.First(n => n.Id == nodeId);

                _traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, nodeId == nodes.Last());
                _windowManager.ShowDialogWithAssignedOwner(_traceContentChoiceViewModel);

                // пользователь прервал процесс, отказавшись выбирать оборудование
                if (!_traceContentChoiceViewModel.ShouldWeContinue)
                    return null;

                var selectedEquipmentGuid = _traceContentChoiceViewModel.GetSelectedEquipmentGuid();
                equipments.Add(selectedEquipmentGuid);
            }
            return equipments;
        }

    }

}
