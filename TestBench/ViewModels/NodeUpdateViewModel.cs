using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench
{
    public class EqItem : PropertyChangedBase
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public string Traces { get; set; }

        public bool IsRemoveEnabled { get; set; }

        private object _command;
        public object Command
        {
            get { return _command; }
            set
            {
                if (Equals(value, _command)) return;
                _command = value;
                NotifyOfPropertyChange();
            }
        }

        public void UpdateEquipment()
        {
            Command = new UpdateEquipment { Id = Id };
        }

        public void RemoveEquipment()
        {
            Command = new RemoveEquipment { Id = Id };
        }
    }

    public class NodeUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly GraphVm _graphVm;
        private readonly IWindowManager _windowManager;
        private readonly NodeVm _originalNode;
        public bool IsClosed { get; set; }

        public Guid NodeId { get; set; }

        private string _title;
        private string _comment;
        private bool _isButtonSaveEnabled;
        private object _command;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<EqItem> EquipmentsInNode { get; set; }
        public EqItem SelectedEquipment { get; set; }

        public List<TraceVm> TracesInNode { get; set; }

        private bool IsChanged()
        {
            return _title != _originalNode.Title
                   || _comment != _originalNode.Comment;
        }

        public bool IsButtonSaveEnabled
        {
            get { return _isButtonSaveEnabled; }
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public object Command
        {
            get { return _command; }
            set
            {
                if (Equals(value, _command)) return;
                _command = value;
                NotifyOfPropertyChange();
            }
        }

        public NodeUpdateViewModel(Guid nodeId, GraphVm graphVm, IWindowManager windowManager)
        {
            _graphVm = graphVm;
            _windowManager = windowManager;
            NodeId = nodeId;
            _originalNode = _graphVm.Nodes.First(n => n.Id == nodeId);
            Title = _originalNode.Title;
            Comment = _originalNode.Comment;

            TracesInNode = _graphVm.Traces.Where(t => t.Nodes.Contains(nodeId)).ToList();

            EquipmentsInNode = new ObservableCollection<EqItem>(
                _graphVm.Equipments.Where(e => e.Node.Id == NodeId).Select(equipmentVm => CreateEqItem(equipmentVm)));

            IsClosed = false;
        }

        private EqItem CreateEqItem(EquipmentVm equipmentVm)
        {
            var tracesNames = _graphVm.Traces.Where(t => t.Equipments.Contains(equipmentVm.Id))
                .Aggregate("", (current, traceVm) => current + (traceVm.Title + " ;  "));

            var isLastForSomeTrace = _graphVm.Traces.Any(t => t.Equipments.Last() == equipmentVm.Id);
            var isPartOfTraceWithBase = _graphVm.Traces.Any(t => t.Equipments.Contains(equipmentVm.Id) && t.HasBase);

            var eqItem = new EqItem()
            {
                Id = equipmentVm.Id,
                Type = equipmentVm.Type.ToString(),
                Title = equipmentVm.Title,
                Comment = equipmentVm.Comment,
                Traces = tracesNames,
                IsRemoveEnabled = !isLastForSomeTrace && !isPartOfTraceWithBase,
            };
            eqItem.PropertyChanged += EqItem_PropertyChanged;
            return eqItem;
        }

        private void EqItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Command")
                return;
            var cmd = ((EqItem) sender).Command;
            if (cmd is UpdateEquipment)
                LaunchUpdateEquipmentView(((UpdateEquipment)cmd).Id);
            else
                RemoveEquipment((RemoveEquipment)cmd); 
        }

        public void AddEquipment()
        {
            var addEquipmentViewModel = new EquipmentUpdateViewModel(NodeId, Guid.Empty);
            _windowManager.ShowDialog(addEquipmentViewModel);
            if (addEquipmentViewModel.Command == null)
                return;
            Command = addEquipmentViewModel.Command;

            IMapper mapper = new MapperConfiguration(
                    cfg => cfg.AddProfile<MappingCommandToVm>()).CreateMapper();
            var equipmentVm = mapper.Map<EquipmentVm>(Command);
            EquipmentsInNode.Add(CreateEqItem(equipmentVm));
        }

        private void LaunchUpdateEquipmentView(Guid id)
        {
            var equipmentVm = _graphVm.Equipments.First(e => e.Id == id);

            var updateEquipmentViewModel = new EquipmentUpdateViewModel(NodeId, id);
            IMapper mapperToViewModel = new MapperConfiguration(
                    cfg => cfg.AddProfile<MappingVmToViewModel>()).CreateMapper();
            mapperToViewModel.Map(equipmentVm, updateEquipmentViewModel);
            _windowManager.ShowDialog(updateEquipmentViewModel);

            if (updateEquipmentViewModel.Command == null)
                return;
            Command = updateEquipmentViewModel.Command;

            IMapper mapper = new MapperConfiguration(
                    cfg => cfg.AddProfile<MappingCommandToVm>()).CreateMapper();
            mapper.Map(Command, equipmentVm);

            EquipmentsInNode.Remove(EquipmentsInNode.First(e => e.Id == equipmentVm.Id));
            EquipmentsInNode.Add(CreateEqItem(equipmentVm));
        }

        public void RemoveEquipment(RemoveEquipment cmd)
        {
            Command = cmd;
            EquipmentsInNode.Remove(EquipmentsInNode.First(e => e.Id == cmd.Id));
        }

        public void Save()
        {
            Command = IsChanged() ?
                new UpdateNode
                {
                    Id = NodeId,
                    Title = _title,
                    Comment = _comment
                } 
                : null;

            CloseView();
        }

        public void Cancel()
        {
            Command = null;
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (string.IsNullOrEmpty(_title))
                            errorMessage = Resources.SID_Title_is_required;
                        if (_graphVm.Nodes.Any(n => n.Title == _title && n.Id != _originalNode.Id))
                            errorMessage = Resources.SID_There_is_a_node_with_the_same_title;
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }

    }
}
