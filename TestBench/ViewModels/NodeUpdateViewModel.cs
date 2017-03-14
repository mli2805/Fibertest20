﻿using System;
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

        public ObservableCollection<EqItemVm> EquipmentsInNode
        {
            get { return _equipmentsInNode; }
            set
            {
                if (Equals(value, _equipmentsInNode)) return;
                _equipmentsInNode = value;
                NotifyOfPropertyChange();
            }
        }

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
        private ObservableCollection<EqItemVm> _equipmentsInNode;

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

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Node;
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
            // TODO ask traces which will use new equipment

            var addEquipmentViewModel = new EquipmentInfoViewModel(_originalNode.Id, _bus);
            _windowManager.ShowDialog(addEquipmentViewModel);
            if (addEquipmentViewModel.Command == null)
                return;
            var cmd = (AddEquipmentIntoNode)addEquipmentViewModel.Command;
            _bus.SendCommand(cmd).Wait();
            // refresh equipments
            EquipmentsInNode.Add(new EqItemVm() {Id = cmd.Id, Title = cmd.Title, Type = cmd.Type.ToLocalizedString()});
        }

        private void LaunchUpdateEquipmentView(Guid id)
        {
            var equipment = _readModel.Equipments.First(e => e.Id == id);

            var equipmentViewModel = new EquipmentInfoViewModel(equipment, _bus);
            IMapper mapperDomainModelToViewModel = new MapperConfiguration(
                    cfg => cfg.AddProfile<MappingDomainModelToViewModel>()).CreateMapper();
            mapperDomainModelToViewModel.Map(equipment, equipmentViewModel);
            _windowManager.ShowDialog(equipmentViewModel);

            if (equipmentViewModel.Command == null)
                return;
            // refresh equipments
            var cmd = (UpdateEquipment)equipmentViewModel.Command;
            var item = EquipmentsInNode.First(i => i.Id == id);
            item.Title = cmd.Title;
            item.Type = cmd.Type.ToLocalizedString();
        }

        public void RemoveEquipment(RemoveEquipment cmd)
        {
            _bus.SendCommand(cmd);
            // refresh equipments
            EquipmentsInNode.Remove(EquipmentsInNode.First(item => item.Id == cmd.Id));
        }

        public void Save()
        {
            Command = IsChanged() ?
                new UpdateNode
                {
                    Id = _originalNode.Id,
                    Title = _title?.Trim(),
                    Comment = _comment?.Trim()
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
                        if (string.IsNullOrEmpty(_title?.Trim()))
                            errorMessage = Resources.SID_Title_is_required;
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }

    }
}
