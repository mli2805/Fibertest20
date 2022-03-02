﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class StepChoiceViewModel : Screen
    {
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        public List<RadioButtonModel> Models { get; set; }
        private List<Node> _neighbours;
        private Node _selectedNode;

        public StepChoiceViewModel(GraphReadModel graphReadModel, Model readModel)
        {
            _graphReadModel = graphReadModel;
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Next_step;
        }

        public async Task<bool> Initialize(List<Guid> neighbours, Guid previousNodeId)
        {
            _neighbours = neighbours.Select(id => _readModel.Nodes.First(n => n.NodeId == id)).ToList();

            Models = new List<RadioButtonModel>();
            foreach (var node in _neighbours)
            {
                var model = new RadioButtonModel()
                {
                    Id = node.NodeId,
                    IsEnabled = true,
                    Title = node.NodeId == previousNodeId ? node.Title + Resources.SID____previous_ : node.Title,
                };
                model.PropertyChanged += Model_PropertyChanged;
                if (node.NodeId != previousNodeId)
                    Models.Insert(0, model);
                else
                    Models.Add(model); // previous node should be last in Models list
            }

            _selectedNode = _neighbours.FirstOrDefault();
            if (_selectedNode == null) return false;
            Models.First().IsChecked = true;

            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _selectedNode.NodeId);
            if (nodeVm == null)
            {
                _graphReadModel.MainMap.SetPositionWithoutFiringEvent(_selectedNode.Position);
                await _graphReadModel.RefreshVisiblePart();
                nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _selectedNode.NodeId);
            }

            nodeVm.IsHighlighted = true;
            return true;
        }

        private async void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var modelWithChanges = (RadioButtonModel)sender;

            if (modelWithChanges.IsChecked)
            {
                _selectedNode = _neighbours.First(n => n.NodeId == modelWithChanges.Id);
                _graphReadModel.MainMap.SetPositionWithoutFiringEvent(_selectedNode.Position);
                await _graphReadModel.RefreshVisiblePart();
                var newChoiceNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _selectedNode.NodeId);
                newChoiceNodeVm.IsHighlighted = true;
            }
            else
            {
                var previousChoiceNodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == modelWithChanges.Id);
                previousChoiceNodeVm.IsHighlighted = false;
            }
        }

        public Node GetSelected()
        {
            return _neighbours.First(n => n.NodeId == Models.First(m => m.IsChecked).Id);
        }

        public void Select()
        {
            _selectedNode.IsHighlighted = false;
            TryClose(true);
        }

        public void Cancel()
        {
            _selectedNode.IsHighlighted = false;
            var  nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _selectedNode.NodeId);
            nodeVm.IsHighlighted = false;
            TryClose(false);
        }

        public override void CanClose(Action<bool> callback)
        {
            foreach (var radioButtonModel in Models)
            {
                radioButtonModel.PropertyChanged -= Model_PropertyChanged;
            }
            base.CanClose(callback);
        }

    }
}
