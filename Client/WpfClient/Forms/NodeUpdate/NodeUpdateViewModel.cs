﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class NodeUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly CurrentGis _currentGis;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly AddEquipmentIntoNodeBuilder _addEquipmentIntoNodeBuilder;
        private Node _originalNode;
        private PointLatLng _nodeCoors;

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }
        public List<GpsInputModeComboItem> GpsInputModeComboItems { get; set; } =
            (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
             select new GpsInputModeComboItem(mode)).ToList();

        private GpsInputModeComboItem _selectedGpsInputModeComboItem;
        public GpsInputModeComboItem SelectedGpsInputModeComboItem
        {
            get => _selectedGpsInputModeComboItem;
            set
            {
                if (Equals(value, _selectedGpsInputModeComboItem)) return;
                _selectedGpsInputModeComboItem = value;
                NotifyOfPropertyChange();
                Coors = _nodeCoors.ToDetailedString(_selectedGpsInputModeComboItem.Mode);
                _currentGis.GpsInputMode = _selectedGpsInputModeComboItem.Mode;
            }
        }

        private string _coors;
        public string Coors
        {
            get => _coors;
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
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<ItemOfEquipmentTableModel> _equipmentsInNode;
        public ObservableCollection<ItemOfEquipmentTableModel> EquipmentsInNode
        {
            get => _equipmentsInNode;
            set
            {
                if (Equals(value, _equipmentsInNode)) return;
                _equipmentsInNode = value;
                NotifyOfPropertyChange();
            }
        }

        public Trace SelectedTrace { get; set; }
        public List<Trace> TracesInNode { get; set; }

        private bool IsChanged()
        {
            return _title != _originalNode.Title
                   || _comment != _originalNode.Comment;
        }

        private bool _isButtonSaveEnabled;
        public bool IsButtonSaveEnabled
        {
            get => _isButtonSaveEnabled;
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

        public bool IsEditEnabled { get; set; }

        public Visibility GisVisibility { get; set; }

        public NodeUpdateViewModel(ILifetimeScope globalScope, Model readModel, GraphReadModel graphReadModel,
            IWindowManager windowManager, EventArrivalNotifier eventArrivalNotifier,
            IWcfServiceDesktopC2D c2DWcfManager, CurrentGis currentGis,
            CurrentUser currentUser, CurrentlyHiddenRtu currentlyHiddenRtu,
            AddEquipmentIntoNodeBuilder addEquipmentIntoNodeBuilder)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _windowManager = windowManager;
            eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
            _c2DWcfManager = c2DWcfManager;
            _currentGis = currentGis;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _addEquipmentIntoNodeBuilder = addEquipmentIntoNodeBuilder;
        }

        public void Initialize(Guid nodeId)
        {
            _originalNode = _readModel.Nodes.First(n => n.NodeId == nodeId);
            _nodeCoors = _originalNode.Position;
            Title = _originalNode.Title;
            GisVisibility = _currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            _selectedGpsInputModeComboItem = GpsInputModeComboItems.First(i => i.Mode == _currentGis.GpsInputMode);
            Coors = _nodeCoors.ToDetailedString(_selectedGpsInputModeComboItem.Mode);
            Comment = _originalNode.Comment;

            TracesInNode = _readModel.Traces.Where(t => t.NodeIds.Contains(nodeId)).ToList();
            SelectedTrace = TracesInNode.FirstOrDefault();

            EquipmentsInNode = new ObservableCollection<ItemOfEquipmentTableModel>(
                _readModel.Equipments.Where(e => e.NodeId == _originalNode.NodeId && e.Type != EquipmentType.EmptyNode).Select(CreateEqItem));
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EquipmentsInNode = new ObservableCollection<ItemOfEquipmentTableModel>(
                _readModel.Equipments.Where(eq => eq.NodeId == _originalNode.NodeId && eq.Type != EquipmentType.EmptyNode).Select(CreateEqItem));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Node;
        }

        private ItemOfEquipmentTableModel CreateEqItem(Equipment equipment)
        {
            var tracesNames = _readModel.Traces.Where(t => t.EquipmentIds.Contains(equipment.EquipmentId))
                .Aggregate("", (current, traceVm) => current + (traceVm.Title + @" ;  "));

            var isLastForSomeTrace = _readModel.Traces.Any(t => t.EquipmentIds.Last() == equipment.EquipmentId);
            var isPartOfTraceWithBase = _readModel.Traces.Any(t => t.EquipmentIds.Contains(equipment.EquipmentId) && t.HasAnyBaseRef);

            var eqItem = new ItemOfEquipmentTableModel()
            {
                Id = equipment.EquipmentId,
                NodeId = _originalNode.NodeId,
                Type = equipment.Type.ToLocalizedString(),
                Title = equipment.Title,
                CableReserveLeft = equipment.CableReserveLeft.ToString(),
                CableReserveRight = equipment.CableReserveRight.ToString(),
                Comment = equipment.Comment,
                Traces = tracesNames,
                IsRemoveEnabled = !isLastForSomeTrace && !isPartOfTraceWithBase,
            };
            eqItem.PropertyChanged += EqItem_PropertyChanged;
            return eqItem;
        }

        private async void EqItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var cmd = ((ItemOfEquipmentTableModel)sender).Command;
            if (cmd is UpdateEquipment equipment)
                LaunchUpdateEquipmentView(equipment.EquipmentId);
            else if (cmd is RemoveEquipment)
                await RemoveEquipment((RemoveEquipment)cmd);
            else
                await AddEquipmentIntoNode();
        }

        public async Task AddEquipmentIntoNode()
        {
            var cmd = _addEquipmentIntoNodeBuilder.BuildCommand(_originalNode.NodeId);
            if (cmd == null)
                return;
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async void AddEquipment()
        {
            await AddEquipmentIntoNode();
        }

        public void ShowTrace()
        {
            if (SelectedTrace == null) return;

            if (_currentlyHiddenRtu.Collection.Contains(SelectedTrace.RtuId))
            {
                _currentlyHiddenRtu.Collection.Remove(SelectedTrace.RtuId);
                _currentlyHiddenRtu.ChangedRtu = SelectedTrace.RtuId;
            }
            _graphReadModel.HighlightTrace(SelectedTrace.NodeIds[0], SelectedTrace.FiberIds);
        }

        private async void LaunchUpdateEquipmentView(Guid id)
        {
            if (!_readModel.EquipmentCanBeChanged(id, _windowManager)) return;

            var equipment = _readModel.Equipments.First(e => e.EquipmentId == id);

            var equipmentViewModel = _globalScope.Resolve<EquipmentInfoViewModel>();
            equipmentViewModel.InitializeForUpdate(equipment);
            _windowManager.ShowDialogWithAssignedOwner(equipmentViewModel);
            if (equipmentViewModel.Command == null) return;
            var cmd = (UpdateEquipment)equipmentViewModel.Command;

            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task RemoveEquipment(RemoveEquipment cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async void Save()
        {
            if (IsChanged())
            {
                var cmd = new UpdateNode
                {
                    NodeId = _originalNode.NodeId,
                    Title = _title?.Trim(),
                    Comment = _comment?.Trim()
                };
                await _c2DWcfManager.SendCommandAsObj(cmd);
            }

            TryClose();
        }

        public void Cancel()
        {
            Command = null;
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
