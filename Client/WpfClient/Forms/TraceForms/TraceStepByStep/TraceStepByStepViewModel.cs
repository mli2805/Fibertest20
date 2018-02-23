using System;
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

        public void Initialize(Guid rtuId)
        {
            Steps = new ObservableCollection<StepModel>();
            var rtu = _graphReadModel.Rtus.First(r => r.Node.Id == rtuId);
            Steps.Add(new StepModel() { NodeId = rtu.Node.Id, Title = rtu.Title });
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Step_by_step_trace_defining;
        }

        public void GoToFork()
        {
        }

        public void StepBackward()
        {
        }

        public void StepForward()
        {
            var neighbours = _graphReadModel.GetNeighbours(Steps.Last().NodeId);
            Guid previousNodeId = Steps.Count == 1 ? Guid.Empty : Steps[Steps.Count - 2].NodeId;

            switch (neighbours.Count)
            {
                case 1:
                    if (neighbours[0].Id != previousNodeId)
                        JustStep(neighbours[0]);
                    break;
                case 2:
                    if (previousNodeId != Guid.Empty)
                        JustStep(nextNode: neighbours[0].Id != previousNodeId ? neighbours[0] : neighbours[1]);
                    else
                        ForkIt(neighbours, previousNodeId);
                    break;
                default:
                    ForkIt(neighbours, previousNodeId);
                    break;
            }
        }

        private void ForkIt(List<NodeVm> neighbours, Guid previousNodeId)
        {
            var vm = new StepChoiceViewModel(_graphReadModel);
            vm.Initialize(neighbours, previousNodeId);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) == true)
            {
                var selectedNode = vm.GetSelected();
                Steps.Add(new StepModel() { NodeId = selectedNode.Id, Title = selectedNode.Title });
            }
        }

        private void JustStep(NodeVm nextNode)
        {
            Steps.Add(new StepModel() { NodeId = nextNode.Id, Title = nextNode.Title });
        }

        public void CancelStep()
        {
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
