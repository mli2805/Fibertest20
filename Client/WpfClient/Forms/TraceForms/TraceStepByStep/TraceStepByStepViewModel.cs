﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class StepModel
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; }
    }
    public class TraceStepByStepViewModel : Screen
    {
        private readonly GraphReadModel _graphReadModel;
        private readonly IWindowManager _windowManager;
        public ObservableCollection<StepModel> Steps { get; set; }

        public TraceStepByStepViewModel(GraphReadModel graphReadModel, IWindowManager windowManager)
        {
            _graphReadModel = graphReadModel;
            _windowManager = windowManager;
        }

        public void Initialize(Guid rtuNodeId)
        {
            Steps = new ObservableCollection<StepModel>();
            var rtu = _graphReadModel.Rtus.First(r => r.Node.Id == rtuNodeId);
            _graphReadModel.MainMap.Position = rtu.Node.Position;
            Steps.Add(new StepModel() { NodeId = rtu.Node.Id, Title = rtu.Title });
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Step_by_step_trace_defining;
        }

        public void SemiautomaticMode()
        {
            while (StepForward()){}
        }

        public void StepBackward()
        {
            if (Steps.Count == 1) return;
            Guid backwardNodeId = Steps[Steps.Count - 2].NodeId;
            JustStep(_graphReadModel.Nodes.First(n=>n.Id == backwardNodeId));
        }

        public bool StepForward()
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
            var vm = new StepChoiceViewModel();
            vm.Initialize(neighbours, previousNodeId);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) != true)
                return false;

            var selectedNode = vm.GetSelected();
            Steps.Add(new StepModel() { NodeId = selectedNode.Id, Title = selectedNode.Title });
            _graphReadModel.MainMap.Position = selectedNode.Position;
            return true;
        }

        private bool JustStep(NodeVm nextNode)
        {

            Steps.Add(new StepModel() { NodeId = nextNode.Id, Title = nextNode.Title });
            _graphReadModel.MainMap.Position = nextNode.Position;
            return true;
        }

        public void CancelStep()
        {
            Steps.Remove(Steps.Last());
            _graphReadModel.MainMap.Position = _graphReadModel.Nodes.First(n => n.Id == Steps.Last().NodeId).Position;
        }

        public void Accept()
        {
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
