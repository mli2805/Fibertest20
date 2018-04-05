using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

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
        private List<StepModel> _stepsWithAdjustmentPoints;

        public TraceStepByStepViewModel(ILifetimeScope globalScope, GraphReadModel graphReadModel, Model readModel, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public void Initialize(Guid rtuNodeId, string rtuTitle)
        {
            Steps = new ObservableCollection<StepModel>();
            _stepsWithAdjustmentPoints = new List<StepModel>();
            var rtuNode = _readModel.Nodes.First(n => n.NodeId == rtuNodeId);
            _graphReadModel.MainMap.Position = rtuNode.Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == rtuNodeId);
            _currentHighlightedNode.IsHighlighted = true;
            var firstStepRtu = new StepModel() { NodeId = rtuNode.NodeId, Title = rtuTitle, EquipmentId = rtuNodeId };
            Steps.Add(firstStepRtu);
            _stepsWithAdjustmentPoints.Add(firstStepRtu);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Step_by_step_trace_defining;
        }

        public void SemiautomaticMode()
        {
            while (StepForward()) { }
        }

        public void StepBackward()
        {
            if (Steps.Count == 1) return;
            Guid backwardNodeId = Steps[Steps.Count - 2].NodeId;
            JustStep(_graphReadModel.Data.Nodes.First(n => n.Id == backwardNodeId));
        }

        public bool StepForward() // public because it is button handler
        {
            var neighbours = _graphReadModel.GetNeighbours(Steps.Last().NodeId);
            Guid previousNodeId = Steps.Count == 1 ? Guid.Empty : Steps[Steps.Count - 2].NodeId;

            switch (neighbours.Count)
            {
                case 1:
                    if (neighbours[0].Id != previousNodeId)
                        return JustStep(neighbours[0]);
                    return false;
                case 2:
                    if (previousNodeId != Guid.Empty)
                        return JustStep(nextNode: neighbours[0].Id != previousNodeId ? neighbours[0] : neighbours[1]);
                    else
                        return ForkIt(neighbours, previousNodeId);
                default:
                    return ForkIt(neighbours, previousNodeId);
            }
        }

        private bool ForkIt(List<NodeVm> neighbours, Guid previousNodeId)
        {
            _currentHighlightedNode.IsHighlighted = false;

            var vm = new StepChoiceViewModel();
            vm.Initialize(neighbours, previousNodeId);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) != true)
            {
                _currentHighlightedNode.IsHighlighted = true;
                return false;
            }
            var selectedNode = vm.GetSelected();

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(selectedNode.Id, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                _currentHighlightedNode.IsHighlighted = true;
                return false;
            }

            Steps.Add(new StepModel() { NodeId = selectedNode.Id, Title = titleStr, EquipmentId = equipmentId });
            _graphReadModel.MainMap.Position = selectedNode.Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == selectedNode.Id);
            _currentHighlightedNode.IsHighlighted = true;
            return true;
        }

        private bool JustStep(NodeVm nextNode)
        {
            _currentHighlightedNode.IsHighlighted = false;

            var equipmentId = _graphReadModel.ChooseEquipmentForNode(nextNode.Id, false, out var titleStr);
            if (equipmentId == Guid.Empty)
            {
                _currentHighlightedNode.IsHighlighted = true;
                return false;
            }

            Steps.Add(new StepModel() { NodeId = nextNode.Id, Title = titleStr, EquipmentId = equipmentId });
            _graphReadModel.MainMap.Position = nextNode.Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == nextNode.Id);
            _currentHighlightedNode.IsHighlighted = true;
            return true;
        }

        public void CancelStep()
        {
            _currentHighlightedNode.IsHighlighted = false;
            Steps.Remove(Steps.Last());
            _graphReadModel.MainMap.Position = _graphReadModel.Data.Nodes.First(n => n.Id == Steps.Last().NodeId).Position;
            _currentHighlightedNode = _graphReadModel.Data.Nodes.First(n => n.Id == Steps.Last().NodeId);
            _currentHighlightedNode.IsHighlighted = true;
        }

        public void Accept()
        {
            if (_stepsWithAdjustmentPoints.Count <= 1) return;

            var equipment = _readModel.Equipments.First(e => e.EquipmentId == _stepsWithAdjustmentPoints.Last().EquipmentId);
            if (equipment.Type <= EquipmentType.EmptyNode)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
                return;
            }

            var traceEquipments = _stepsWithAdjustmentPoints.Select(s => s.EquipmentId).ToList();
            var traceNodes = _stepsWithAdjustmentPoints.Select(s => s.NodeId).ToList();
            var traceAddViewModel = _globalScope.Resolve<TraceInfoViewModel>();
            traceAddViewModel.Initialize(Guid.Empty, traceEquipments, traceNodes);
            if (_windowManager.ShowDialogWithAssignedOwner(traceAddViewModel) == true)
            {
                _currentHighlightedNode.IsHighlighted = false;
                TryClose();
            }
        }

        public void Cancel()
        {
            _currentHighlightedNode.IsHighlighted = false;
            TryClose();
        }
    }
}
