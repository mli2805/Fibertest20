using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class NodeUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly Bus _bus;
        private readonly Node _originalNode;
        private  GpsLocation _nodeCoors;
        public bool IsClosed { get; set; }

        private string _title;
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
        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
            (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
             select new GpsInputModeComboItem(mode)).ToList();

        private GpsInputModeComboItem _selectedGpsInputMode;
        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get { return _selectedGpsInputMode; }
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                NotifyOfPropertyChange();
                Coors = _nodeCoors.ToString(_selectedGpsInputMode.Mode);
            }
        }

        private string _coors;
        public string Coors
        {
            get { return _coors; }
            set
            {
                if (value == _coors) return;
                _coors = value;
                NotifyOfPropertyChange();
            }
        }

        private string _comment;
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

        public ObservableCollection<EqItemVm> EquipmentsInNode { get; set; }
        public EqItemVm SelectedEquipment { get; set; }

        public List<Trace> TracesInNode { get; set; }

        private bool IsChanged()
        {
            return _title != _originalNode.Title
                   || _comment != _originalNode.Comment;
        }

        private bool _isButtonSaveEnabled;
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

        public NodeUpdateViewModel(Guid nodeId, ReadModel readModel, IWindowManager windowManager, Bus bus)
        {
            _readModel = readModel;
            _windowManager = windowManager;
            _bus = bus;
            _originalNode = _readModel.Nodes.First(n => n.Id == nodeId);
            _nodeCoors = new GpsLocation(_originalNode.Latitude, _originalNode.Longitude);
            Title = _originalNode.Title;
            SelectedGpsInputMode = GpsInputModes.First();
            Comment = _originalNode.Comment;

            TracesInNode = _readModel.Traces.Where(t => t.Nodes.Contains(nodeId)).ToList();

            EquipmentsInNode = new ObservableCollection<EqItemVm>(
                _readModel.Equipments.Where(e => e.NodeId == _originalNode.Id).Select(equipmentVm => CreateEqItem(equipmentVm)));

            IsClosed = false;
        }

        private EqItemVm CreateEqItem(Equipment equipment)
        {
            var tracesNames = _readModel.Traces.Where(t => t.Equipments.Contains(equipment.Id))
                .Aggregate("", (current, traceVm) => current + (traceVm.Title + @" ;  "));

            var isLastForSomeTrace = _readModel.Traces.Any(t => t.Equipments.Last() == equipment.Id);
            var isPartOfTraceWithBase = _readModel.Traces.Any(t => t.Equipments.Contains(equipment.Id) && t.HasBase);

            var eqItem = new EqItemVm()
            {
                Id = equipment.Id,
                Type = equipment.Type.ToString(),
                Title = equipment.Title,
                Comment = equipment.Comment,
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
            var cmd = ((EqItemVm)sender).Command;
            if (cmd is UpdateEquipment)
                LaunchUpdateEquipmentView(((UpdateEquipment)cmd).Id);
            else
                RemoveEquipment((RemoveEquipment)cmd);
        }

        public void AddEquipment()
        {
            // TODO ask traces whick will use new equipment

            var addEquipmentViewModel = new EquipmentUpdateViewModel(_originalNode.Id, Guid.Empty, _bus);
            _windowManager.ShowDialog(addEquipmentViewModel);
            if (addEquipmentViewModel.Command == null)
                return;
            var cmd = addEquipmentViewModel.Command;

            _bus.SendCommand(cmd).Wait();
            // refresh equipments
            EquipmentsInNode = new ObservableCollection<EqItemVm>(
                _readModel.Equipments.Where(e => e.NodeId == _originalNode.Id).Select(equipmentVm => CreateEqItem(equipmentVm)));
        }

        private void LaunchUpdateEquipmentView(Guid id)
        {
            var equipmentVm = _readModel.Equipments.First(e => e.Id == id);

            var updateEquipmentViewModel = new EquipmentUpdateViewModel(Guid.Empty, id, _bus);
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
            _bus.SendCommand(cmd);
            // refresh equipments
            EquipmentsInNode = new ObservableCollection<EqItemVm>(
                _readModel.Equipments.Where(e => e.NodeId == _originalNode.Id).Select(equipmentVm => CreateEqItem(equipmentVm)));
        }

        public void Save()
        {
            Command = IsChanged() ?
                new UpdateNode
                {
                    Id = _originalNode.Id,
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
                        if (_readModel.Nodes.Any(n => n.Title == _title && n.Id != _originalNode.Id))
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
