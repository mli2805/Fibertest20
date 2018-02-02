using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class NodeUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly Node _originalNode;
        private readonly PointLatLng _nodeCoors;

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
        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
            (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
             select new GpsInputModeComboItem(mode)).ToList();

        private GpsInputModeComboItem _selectedGpsInputMode;
        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get => _selectedGpsInputMode;
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                NotifyOfPropertyChange();
                Coors = _nodeCoors.ToDetailedString(_selectedGpsInputMode.Mode);
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

        public NodeUpdateViewModel(Guid nodeId, ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _readModel = readModel;
            _readModel.PropertyChanged += _readModel_PropertyChanged;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _originalNode = _readModel.Nodes.First(n => n.Id == nodeId);
            _nodeCoors = new PointLatLng() {Lat = _originalNode.Latitude, Lng = _originalNode.Longitude};
            Title = _originalNode.Title;
            SelectedGpsInputMode = GpsInputModes.First();
            Comment = _originalNode.Comment;

            TracesInNode = _readModel.Traces.Where(t => t.Nodes.Contains(nodeId)).ToList();

            EquipmentsInNode = new ObservableCollection<ItemOfEquipmentTableModel>(
                _readModel.Equipments.Where(e => e.NodeId == _originalNode.Id && e.Type != EquipmentType.EmptyNode).Select(CreateEqItem));
        }

        private void _readModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EquipmentsInNode = new ObservableCollection<ItemOfEquipmentTableModel>(
                _readModel.Equipments.Where(eq => eq.NodeId == _originalNode.Id && eq.Type != EquipmentType.EmptyNode).Select(CreateEqItem));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Node;
        }

        private ItemOfEquipmentTableModel CreateEqItem(Equipment equipment)
        {
            var tracesNames = _readModel.Traces.Where(t => t.Equipments.Contains(equipment.Id))
                .Aggregate("", (current, traceVm) => current + (traceVm.Title + @" ;  "));

            var isLastForSomeTrace = _readModel.Traces.Any(t => t.Equipments.Last() == equipment.Id);
            var isPartOfTraceWithBase = _readModel.Traces.Any(t => t.Equipments.Contains(equipment.Id) && t.HasAnyBaseRef);

            var eqItem = new ItemOfEquipmentTableModel()
            {
                Id = equipment.Id,
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
                LaunchUpdateEquipmentView(equipment.Id);
            else if (cmd is RemoveEquipment)
                RemoveEquipment((RemoveEquipment) cmd);
            else
                await AddEquipmentIntoNode((bool)cmd);
        }

        public async Task AddEquipmentIntoNode(bool isCableReserveRequested)
        {
            var cmd = VerboseTasks.BuildAddEquipmentIntoNodeCommand(_originalNode.Id, isCableReserveRequested, _readModel, _windowManager);
            if (cmd == null)
                return;
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async void AddEquipment()
        {
            await AddEquipmentIntoNode(false);
        }

        public async void AddCableReserve()
        {
            await AddEquipmentIntoNode(true);
        }

        private async void LaunchUpdateEquipmentView(Guid id)
        {
            var equipment = _readModel.Equipments.First(e => e.Id == id);
            UpdateEquipment cmd;
            if (equipment.Type == EquipmentType.CableReserve)
            {
                var cableReserveViewModel = new CableReserveInfoViewModel(equipment, _c2DWcfManager);
                _windowManager.ShowDialogWithAssignedOwner(cableReserveViewModel);
                if (cableReserveViewModel.Command == null) return;
                cmd = (UpdateEquipment)cableReserveViewModel.Command;
            }
            else
            {
                var equipmentViewModel = new EquipmentInfoViewModel(equipment, _c2DWcfManager);
                _windowManager.ShowDialogWithAssignedOwner(equipmentViewModel);
                if (equipmentViewModel.Command == null) return;
                cmd = (UpdateEquipment)equipmentViewModel.Command;
            }
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async void RemoveEquipment(RemoveEquipment cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
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
