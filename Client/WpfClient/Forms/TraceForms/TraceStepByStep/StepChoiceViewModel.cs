using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class StepChoiceViewModel : Screen
    {
        private readonly GraphReadModel _graphReadModel;
        public List<RadioButtonModel> Models { get; set; }
        private List<NodeVm> _neighbours;
        private NodeVm _selectedNode;

        public StepChoiceViewModel(GraphReadModel graphReadModel)
        {
            _graphReadModel = graphReadModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Next_step;
        }

        public bool Initialize(List<NodeVm> neighbours, Guid previousNodeId)
        {
            _neighbours = neighbours;
            Models = new List<RadioButtonModel>();
            foreach (var nodeVm in neighbours.Where(n=>n.Id != previousNodeId))
            {
                var model = new RadioButtonModel() { Id = nodeVm.Id, IsEnabled = true, Title = nodeVm.Title };
                model.PropertyChanged += Model_PropertyChanged;
                Models.Add(model);
            }

            var previous = neighbours.FirstOrDefault(n => n.Id == previousNodeId);
            if (previous != null)
            {
                var model = new RadioButtonModel() { Id = previous.Id, IsEnabled = true, Title = previous.Title + Resources.SID____previous_ };
                model.PropertyChanged += Model_PropertyChanged;
                Models.Add(model);
            }

            _selectedNode = _neighbours.FirstOrDefault();
            if (_selectedNode == null) return false;
            Models.First().IsChecked = true;
            _selectedNode.IsHighlighted = true;
            return true;
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var modelWithChanges = (RadioButtonModel)sender;
            var nodeWithChanges = _neighbours.First(n => n.Id == modelWithChanges.Id);
            nodeWithChanges.IsHighlighted = modelWithChanges.IsChecked;

            if (modelWithChanges.IsChecked)
            {
                _selectedNode = nodeWithChanges;
                _graphReadModel.MainMap.Position = _selectedNode.Position;
            }
        }

        public NodeVm GetSelected()
        {
            return _neighbours.First(n=>n.Id == Models.First(m => m.IsChecked).Id);
        }

        public void Select()
        {
            _selectedNode.IsHighlighted = false;
            TryClose(true);
        }

        public void Cancel()
        {
            _selectedNode.IsHighlighted = false;
            TryClose(false);
        }
    }
}
