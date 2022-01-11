using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        public bool IsOpen { get; set; }

        private readonly ILifetimeScope _globalScope;
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly StepChoiceViewModel _stepChoiceViewModel;
        private readonly IWindowManager _windowManager;
        private NodeVm _currentHighlightedNodeVm;
        public ObservableCollection<StepModel> Steps { get; set; }
        private Guid _newTraceId;

        private bool _isButtonsEnabled = true;
        public bool IsButtonsEnabled
        {
            get { return _isButtonsEnabled; }
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceStepByStepViewModel(ILifetimeScope globalScope, IWindowManager windowManager,
            GraphReadModel graphReadModel, Model readModel, StepChoiceViewModel stepChoiceViewModel)
        {
            _globalScope = globalScope;
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _stepChoiceViewModel = stepChoiceViewModel;
            _windowManager = windowManager;
        }

        public async Task<int> Initialize(Guid rtuNodeId, string rtuTitle)
        {
            _newTraceId = Guid.NewGuid();
            Steps = new ObservableCollection<StepModel>();

            var rtuNode = _readModel.Nodes.First(n => n.NodeId == rtuNodeId);
            if (_graphReadModel.CurrentGis.ThresholdZoom > _graphReadModel.MainMap.Zoom)
                _graphReadModel.MainMap.Zoom = _graphReadModel.CurrentGis.ThresholdZoom;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(rtuNode.Position);

            var nodeCount = await _graphReadModel.RefreshVisiblePart();

            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == rtuNodeId);
            _currentHighlightedNodeVm.IsHighlighted = true;
            var firstStepRtu = new StepModel()
            {
                NodeId = rtuNodeId,
                Title = rtuTitle,
                EquipmentId = _readModel.Rtus.First(r => r.NodeId == rtuNodeId).Id,
                FiberVms = new List<FiberVm>(), // empty 
            };
            Steps.Add(firstStepRtu);

            return nodeCount;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Step_by_step_trace_defining;
            _graphReadModel.MainMap.IsInTraceDefinitionMode = true;
            IsOpen = true;
        }

        public async void SemiautomaticMode()
        {
            var isButtonPressed = true;
            while (await MakeStepForward(isButtonPressed))
            {
                isButtonPressed = false;
            }
        }

        public async void StepBackward()
        {
            if (Steps.Count == 1) return;
            Guid backwardNodeId = Steps[Steps.Count - 2].NodeId;
            if (_readModel.Rtus.Any(r => r.NodeId == backwardNodeId))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Trace_cannot_be_terminated_by_or_pass_through_RTU_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            await JustStep(_graphReadModel.Data.Nodes.First(n => n.Id == backwardNodeId), Steps.Last().FiberVms);
        }

        public async Task<bool> StepForward()
        {
            return await MakeStepForward(true);
        }

        private async Task<bool> MakeStepForward(bool isButtonPressed)
        {
            // return a previous node among others
            var neighbours = _graphReadModel.GetNeighboursPassingThroughAdjustmentPoints(Steps.Last().NodeId);
            Guid previousNodeId = Steps.Count == 1 ? Guid.Empty : Steps[Steps.Count - 2].NodeId;

            switch (neighbours.Count)
            {
                case 1:
                    if (neighbours[0].Item1.Id != previousNodeId) return await JustStep(neighbours[0].Item1, neighbours[0].Item2);
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
                            return await JustStep(nextTuple.Item1, nextTuple.Item2);

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
            _currentHighlightedNodeVm.IsHighlighted = false;

            if (!_stepChoiceViewModel.Initialize(neighbours.Select(e => e.Item1).ToList(), previousNodeId))
                return false;
            if (_windowManager.ShowDialogWithAssignedOwner(_stepChoiceViewModel) != true)
            {
                _currentHighlightedNodeVm.IsHighlighted = true;
                return false;
            }
            var selectedNode = _stepChoiceViewModel.GetSelected();
            var selectedTuple = neighbours.First(n => n.Item1.Id == selectedNode.Id);

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(selectedNode.Id, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                _currentHighlightedNodeVm.IsHighlighted = true;
                return false;
            }

            Steps.Add(new StepModel() { NodeId = selectedNode.Id, Title = titleStr, EquipmentId = equipmentId, FiberVms = selectedTuple.Item2 });
            _graphReadModel.MainMap.SetPosition(selectedNode.Position);
            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == selectedNode.Id);
            _currentHighlightedNodeVm.IsHighlighted = true;
            SetFibersLight(selectedTuple.Item2, true);

            return true;
        }

        private async Task<bool> JustStep(NodeVm nextNode, List<FiberVm> fiberVmsToNode)
        {
            _currentHighlightedNodeVm.IsHighlighted = false;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(nextNode.Position);
            var unused = await _graphReadModel.RefreshVisiblePart();

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(nextNode.Id, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                _currentHighlightedNodeVm.IsHighlighted = true;
                _graphReadModel.MainMap.SetPositionWithoutFiringEvent(_currentHighlightedNodeVm.Position);
                var unused2 = await _graphReadModel.RefreshVisiblePart();
                return false;
            }

            Steps.Add(new StepModel() { NodeId = nextNode.Id, Title = titleStr, EquipmentId = equipmentId, FiberVms = fiberVmsToNode });
            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == nextNode.Id);
            _currentHighlightedNodeVm.IsHighlighted = true;
            SetFibersLight(fiberVmsToNode, true);

            return true;
        }

        public void CancelStep()
        {
            if (Steps.Count == 0) return;
            _currentHighlightedNodeVm.IsHighlighted = false;

            SetFibersLight(Steps.Last().FiberVms, false);
            Steps.Remove(Steps.Last());

            _currentHighlightedNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == Steps.Last().NodeId);
            if (_currentHighlightedNodeVm == null)
                return;
            _graphReadModel.MainMap.SetPosition(_currentHighlightedNodeVm.Position);
            _currentHighlightedNodeVm.IsHighlighted = true;
        }

        public async void Accept()
        {
            IsButtonsEnabled = false;
            var result = await AcceptProcedure();
            IsButtonsEnabled = true;
            if (result)
                TryClose();
        }

        private async Task<bool> AcceptProcedure()
        {
            if (!Validate()) return false;

            GetListsAugmentedWithAdjustmentPoints(out var traceNodes, out var traceEquipments);
            var traceAddViewModel = _globalScope.Resolve<TraceInfoViewModel>();
            await traceAddViewModel.Initialize(_newTraceId, traceEquipments, traceNodes, true);
            _windowManager.ShowDialogWithAssignedOwner(traceAddViewModel);

            if (!traceAddViewModel.IsSavePressed) return false;

            _currentHighlightedNodeVm.IsHighlighted = false;
            return true;
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

        public void AddNodeIntoFiber(NodeIntoFiberAdded evnt)
        {
            StepModel step;
            while ((step = Steps.FirstOrDefault(s => s.FiberVms
                .FirstOrDefault(f => f.Id == evnt.FiberId) != null)) != null)
            {
                var pos = Steps.IndexOf(step);

                if (evnt.InjectionType == EquipmentType.AdjustmentPoint)
                {
                    var neighbours = _graphReadModel.GetNeighboursPassingThroughAdjustmentPoints(Steps[pos - 1].NodeId);
                    var tuple = neighbours.First(t => t.Item1.Id == step.NodeId);
                    step.FiberVms = tuple.Item2;
                }
                else
                {
                    var newStep1 = new StepModel()
                    {
                        NodeId = evnt.Id,
                        Title = "",
                        EquipmentId = evnt.EquipmentId,
                    };
                    var neighbours = _graphReadModel.GetNeighboursPassingThroughAdjustmentPoints(Steps[pos - 1].NodeId);
                    var tuple = neighbours.First(t => t.Item1.Id == evnt.Id);
                    newStep1.FiberVms = tuple.Item2;

                    var newStep2 = new StepModel()
                    {
                        NodeId = step.NodeId,
                        Title = step.Title,
                        EquipmentId = step.EquipmentId,
                    };
                    var neighbours2 = _graphReadModel.GetNeighboursPassingThroughAdjustmentPoints(evnt.Id);
                    var tuple2 = neighbours2.First(t => t.Item1.Id == step.NodeId);
                    newStep2.FiberVms = tuple2.Item2;

                    Steps.Remove(step);
                    Steps.Insert(pos, newStep1);
                    Steps.Insert(pos + 1, newStep2);
                }
            }
        }

        public void UpdateNode(Guid nodeId)
        {
            for (int i = 1; i < Steps.Count; i++)
            {
                if (Steps[i].NodeId == nodeId)
                {
                    var step = Steps[i];
                    var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == nodeId);
                    step.Title = nodeVm.Title;

                    var equipment = _readModel.Equipments.FirstOrDefault(e => e.EquipmentId == step.EquipmentId);
                    if (equipment == null)
                    {
                        equipment = _readModel.Equipments.First(e => e.NodeId == nodeId);
                        step.EquipmentId = equipment.EquipmentId;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(equipment.Title))
                            step.Title += @" / " + equipment.Title;
                    }

                    Steps.Remove(step);
                    Steps.Insert(i, step);
                }
            }
        }

        public bool IsNodeUsed(Guid nodeId)
        {
            return Steps.Any(s => s.NodeId == nodeId);
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
            _currentHighlightedNodeVm.IsHighlighted = false;
            foreach (var fiberVms in Steps.Select(s => s.FiberVms))
            {
                SetFibersLight(fiberVms, false);
            }
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            _graphReadModel.MainMap.IsInTraceDefinitionMode = false;
            base.CanClose(callback);
        }

        private void SetFibersLight(IEnumerable<FiberVm> fiberVms, bool light)
        {
            foreach (var fiberVm in fiberVms)
            {
                fiberVm.SetLightOnOff(_newTraceId, light);
                var fiber = _readModel.Fibers.First(f => f.FiberId == fiberVm.Id);
                fiber.SetLightOnOff(_newTraceId, light);
            }
        }
    }
}
