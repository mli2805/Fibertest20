using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class TraceStepByStepViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private NodeVm _currentHighlightedNode;
        public ObservableCollection<StepModel> Steps { get; set; }
        private Guid _newTraceId;
        //   private List<Guid> _fiberIds;

        public TraceStepByStepViewModel(ILifetimeScope globalScope, GraphReadModel graphReadModel, Model readModel, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public void Initialize(Guid rtuNodeId, string rtuTitle)
        {
            _newTraceId = Guid.NewGuid();
            //   _fiberIds = new List<Guid>();
            Steps = new ObservableCollection<StepModel>();
            var rtuNode = _readModel.Nodes.First(n => n.NodeId == rtuNodeId);
            _graphReadModel.MainMap.Position = rtuNode.Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == rtuNodeId);
            _currentHighlightedNode.IsHighlighted = true;
            var firstStepRtu = new StepModel()
            {
                NodeId = rtuNode.NodeId,
                Title = rtuTitle,
                EquipmentId = _readModel.Rtus.First(r => r.NodeId == rtuNodeId).Id,
                FiberVms = new List<FiberVm>(), // empty 
            };
            Steps.Add(firstStepRtu);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Step_by_step_trace_defining;
        }

        public void SemiautomaticMode()
        {
            var isButtonPressed = true;
            while (MakeStepForward(isButtonPressed))
            {
                isButtonPressed = false;
            }
        }

        public void StepBackward()
        {
            if (Steps.Count == 1) return;
            Guid backwardNodeId = Steps[Steps.Count - 2].NodeId;
            if (_readModel.Rtus.Any(r => r.NodeId == backwardNodeId))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Trace_cannot_be_terminated_by_or_pass_through_RTU_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            JustStep(_graphReadModel.Data.Nodes.First(n => n.Id == backwardNodeId), Steps.Last().FiberVms);
        }

        public bool StepForward()
        {
            return MakeStepForward(true);
        }

        private bool MakeStepForward(bool isButtonPressed)
        {
            // return a previous node among others
            var neighbours = _graphReadModel.GetNeiboursPassingThroughAdjustmentPoints(Steps.Last().NodeId);
            Guid previousNodeId = Steps.Count == 1 ? Guid.Empty : Steps[Steps.Count - 2].NodeId;

            switch (neighbours.Count)
            {
                case 1:
                    if (neighbours[0].Item1.Id != previousNodeId) return JustStep(neighbours[0].Item1, neighbours[0].Item2);
                    if (!isButtonPressed) return false;

                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                        { Resources.SID_It_s_an_end_node_, Resources.SID_If_you_need_to_continue__press__Step_backward_ }, -1);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                case 2:
                    if (previousNodeId != Guid.Empty)
                    {
                        var nextTuple = neighbours[0].Item1.Id != previousNodeId ? neighbours[0] : neighbours[1];
                        if (nextTuple.Item1.Type != EquipmentType.Rtu)
                            return JustStep(nextTuple.Item1, nextTuple.Item2);

                        var vm2 = new MyMessageBoxViewModel(MessageType.Error,
                            Resources.SID_Trace_cannot_be_terminated_by_or_pass_through_RTU_);
                        _windowManager.ShowDialogWithAssignedOwner(vm2);
                        return false;
                    }
                    else
                        return ForkIt(neighbours, previousNodeId);
                default:
                    return ForkIt(neighbours.Where(n => n.Item1.Type != EquipmentType.Rtu).ToList(), previousNodeId);
            }
        }

        private bool ForkIt(List<Tuple<NodeVm, List<FiberVm>>> neighbours, Guid previousNodeId)
        {
            _currentHighlightedNode.IsHighlighted = false;

            var vm = new StepChoiceViewModel();

            if (!vm.Initialize(neighbours.Select(e => e.Item1).ToList(), previousNodeId))
                return false;
            if (_windowManager.ShowDialogWithAssignedOwner(vm) != true)
            {
                _currentHighlightedNode.IsHighlighted = true;
                return false;
            }
            var selectedNode = vm.GetSelected();
            var selectedTuple = neighbours.First(n => n.Item1.Id == selectedNode.Id);

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(selectedNode.Id, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                _currentHighlightedNode.IsHighlighted = true;
                return false;
            }
            //            var fiberVm = _graphReadModel.Data.Fibers.First(f =>
            //                f.Node1.Id == _currentHighlightedNode.Id && f.Node2.Id == selectedNode.Id
            //                || f.Node2.Id == _currentHighlightedNode.Id && f.Node1.Id == selectedNode.Id);

            Steps.Add(new StepModel() { NodeId = selectedNode.Id, Title = titleStr, EquipmentId = equipmentId, FiberVms = selectedTuple.Item2 });
            _graphReadModel.MainMap.Position = selectedNode.Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == selectedNode.Id);
            _currentHighlightedNode.IsHighlighted = true;
            //  _fiberIds.Add(fiberVm.Id);
            foreach (var fiberVm in selectedTuple.Item2)
            {
                fiberVm.SetState(_newTraceId, FiberState.HighLighted);
            }
            return true;
        }

        private bool JustStep(NodeVm nextNode, List<FiberVm> fiberVmsToNode)
        {
            _currentHighlightedNode.IsHighlighted = false;
            //            var fiberVm = _graphReadModel.Data.Fibers.First(f =>
            //                f.Node1.Id == _currentHighlightedNode.Id && f.Node2.Id == nextNode.Id 
            //                || f.Node2.Id == _currentHighlightedNode.Id && f.Node1.Id == nextNode.Id);

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(nextNode.Id, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                _currentHighlightedNode.IsHighlighted = true;
                return false;
            }

            Steps.Add(new StepModel() { NodeId = nextNode.Id, Title = titleStr, EquipmentId = equipmentId, FiberVms = fiberVmsToNode });
            _graphReadModel.MainMap.Position = nextNode.Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == nextNode.Id);
            _currentHighlightedNode.IsHighlighted = true;
            //   _fiberIds.Add(fiberVm.Id);

            foreach (var fiberVm in fiberVmsToNode)
            {
                fiberVm.SetState(_newTraceId, FiberState.HighLighted);
            }
            return true;
        }

        public void CancelStep()
        {
            if (Steps.Count == 0) return;
            _currentHighlightedNode.IsHighlighted = false;
            //   var canceledNodeId = _currentHighlightedNode.Id;

            foreach (var fiberVm in Steps.Last().FiberVms)
            {
                fiberVm.RemoveState(_newTraceId);
            }
            Steps.Remove(Steps.Last());

            _graphReadModel.MainMap.Position = _graphReadModel.Data.Nodes.First(n => n.Id == Steps.Last().NodeId).Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == Steps.Last().NodeId);
            _currentHighlightedNode.IsHighlighted = true;

            //            var fiberVm = _graphReadModel.Data.Fibers.First(f =>
            //                f.Node1.Id == _currentHighlightedNode.Id && f.Node2.Id == canceledNodeId
            //                || f.Node2.Id == _currentHighlightedNode.Id && f.Node1.Id == canceledNodeId);
            //            _fiberIds.RemoveAt(_fiberIds.Count-1);
            //            if (!_fiberIds.Contains(fiberVm.Id))
            //                fiberVm.RemoveState(_newTraceId);
        }

        public void Accept()
        {
            if (!Validate()) return;

            GetListsAugmentedWithAdjustmentPoints(out var traceNodes, out var traceEquipments);
            var traceAddViewModel = _globalScope.Resolve<TraceInfoViewModel>();
            traceAddViewModel.Initialize(_newTraceId, traceEquipments, traceNodes, true);
            _windowManager.ShowDialogWithAssignedOwner(traceAddViewModel);

            if (!traceAddViewModel.IsSavePressed) return;

            _currentHighlightedNode.IsHighlighted = false;
            TryClose();
        }

        private void GetListsAugmentedWithAdjustmentPoints(out List<Guid> nodes, out List<Guid> equipments)
        {
            nodes = new List<Guid> { Steps.First().NodeId };
            equipments = new List<Guid>() { Steps.First().EquipmentId };

            for (int i = 1; i < Steps.Count; i++)
            {
                if (_readModel.Fibers.FirstOrDefault(f =>
                        f.NodeId1 == Steps[i - 1].NodeId && f.NodeId2 == Steps[i].NodeId ||
                        f.NodeId2 == Steps[i - 1].NodeId && f.NodeId1 == Steps[i].NodeId) == null)
                {
                    _graphReadModel.FindPathWhereAdjustmentPointsOnly(Steps[i - 1].NodeId, Steps[i].NodeId, out var pathNodeIds);
                    foreach (var nodeId in pathNodeIds)
                    {
                        nodes.Add(nodeId);
                        equipments.Add(_readModel.Equipments.First(e => e.NodeId == nodeId).EquipmentId);
                    }
                }

                nodes.Add(Steps[i].NodeId);
                equipments.Add(Steps[i].EquipmentId);
            }
        }

        private bool Validate()
        {
            if (Steps.Count <= 1) return false;
            var equipment = _readModel.Equipments.First(e => e.EquipmentId == Steps.Last().EquipmentId);
            if (equipment.Type <= EquipmentType.EmptyNode)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
                return false;
            }
            return true;
        }

        public void Cancel()
        {
            _currentHighlightedNode.IsHighlighted = false;
            //            _graphReadModel.ChangeFutureTraceColor(_newTraceId, _fiberIds, FiberState.NotInTrace);
            var fiberIds = Steps.Select(s => s.FiberVms).SelectMany(x => x).Select(f => f.Id).ToList();
            _graphReadModel.ChangeFutureTraceColor(_newTraceId, fiberIds, FiberState.NotInTrace);
            TryClose();
        }
    }
}
