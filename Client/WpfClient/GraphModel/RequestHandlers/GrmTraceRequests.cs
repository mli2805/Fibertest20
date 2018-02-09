using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class GrmTraceRequests
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;

        public GrmTraceRequests(ILifetimeScope globalScope, ReadModel readModel, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public void AddTrace(RequestAddTrace request)
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

            if (!vm.IsAnswerPositive) return;

            List<Guid> traceEquipments = CollectEquipment(traceNodes);
            ChangeTraceColor(traceId, traceNodes, FiberState.NotInTrace);

            if (traceEquipments == null)
                return;

            var traceAddViewModel = _globalScope.Resolve<TraceInfoViewModel>();
            traceAddViewModel.Initialize(Guid.Empty, traceEquipments, traceNodes);
            _windowManager.ShowDialogWithAssignedOwner(traceAddViewModel);
        }

        private bool Validate(RequestAddTrace request)
        {
            if (_readModel.Equipments.Any(e => e.NodeId == request.LastNodeId && e.Type > EquipmentType.CableReserve)) return true;

            _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
            return false;
        }

        private List<Guid> GetPath(RequestAddTrace request)
        {
            List<Guid> path;
            if (!new PathFinder(_readModel).FindPath(request.NodeWithRtuId, request.LastNodeId, out path))
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Path_couldn_t_be_found));

            return path;
        }

        private void ChangeTraceColor(Guid traceId, List<Guid> nodes, FiberState state)
        {
            var fiberIds = _readModel.GetFibersByNodes(nodes);
            foreach (var fiberId in fiberIds)
            {
                var fiber = _readModel.Fibers.First(f => f.Id == fiberId);
//                if (state != FiberState.NotInTrace)
//                    fiberVm.SetState(traceId, state);
//                else
//                    fiberVm.RemoveState(traceId);
            }
        }

        public List<Guid> CollectEquipment(List<Guid> nodes)
        {
            var equipments = new List<Guid> { _readModel.Rtus.First(r => r.NodeId == nodes[0]).Id };
            var traceContentChoiceViewModel = _globalScope.Resolve<TraceContentChoiceViewModel>();
            foreach (var nodeId in nodes.Skip(1))
            {
                var allEquipmentInNode = _readModel.Equipments.Where(e => e.NodeId == nodeId).ToList();
                if (allEquipmentInNode.Count == 1 && allEquipmentInNode[0].Type == EquipmentType.AdjustmentPoint)
                {
                    equipments.Add(allEquipmentInNode[0].Id);
                    continue;
                }
                var node = _readModel.Nodes.First(n => n.Id == nodeId);

                traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, nodeId == nodes.Last());
                _windowManager.ShowDialogWithAssignedOwner(traceContentChoiceViewModel);

                // пользователь прервал процесс, отказавшись выбирать оборудование
                if (!traceContentChoiceViewModel.ShouldWeContinue)
                    return null;

                var selectedEquipmentGuid = traceContentChoiceViewModel.GetSelectedEquipmentGuid();
                equipments.Add(selectedEquipmentGuid);
            }
            return equipments;
        }

    }
}