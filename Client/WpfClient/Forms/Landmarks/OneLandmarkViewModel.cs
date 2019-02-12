using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class OneLandmarkViewModel : PropertyChangedBase
    {
        public string TraceTitle;
        public DateTime PreciseTimestamp;
        public int SorFileId;
        public Guid RtuId;

        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TabulatorViewModel _tabulatorViewModel;

        public bool IsIncludeEquipmentEnabled
        {
            get => _isIncludeEquipmentEnabled;
            set
            {
                if (value == _isIncludeEquipmentEnabled) return;
                _isIncludeEquipmentEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsExcludeEquipmentEnabled
        {
            get => _isExcludeEquipmentEnabled;
            set
            {
                if (value == _isExcludeEquipmentEnabled) return;
                _isExcludeEquipmentEnabled = value;
                NotifyOfPropertyChange();
            }
        }


        private GpsInputSmallViewModel _gpsInputSmallViewModel;
        public GpsInputSmallViewModel GpsInputSmallViewModel
        {
            get => _gpsInputSmallViewModel;
            set
            {
                if (Equals(value, _gpsInputSmallViewModel)) return;
                _gpsInputSmallViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private Landmark _landmarkBeforeChanges;

        private EquipmentTypeComboItem _selectedEquipmentTypeItem;
        public EquipmentTypeComboItem SelectedEquipmentTypeItem
        {
            get => _selectedEquipmentTypeItem;
            set
            {
                if (Equals(value, _selectedEquipmentTypeItem) || value == null) return;
                _selectedEquipmentTypeItem = value;
                SelectedLandmark.EquipmentType = value.Type;
                NotifyOfPropertyChange();
            }
        }

        private Landmark _selectedLandmark;
        public Landmark SelectedLandmark
        {
            get => _selectedLandmark;
            set
            {
                if (value == null) return;
                _selectedLandmark = value;
                InitializeUserControl();
                NotifyOfPropertyChange();
            }
        }

        private void InitializeUserControl()
        {
            _landmarkBeforeChanges = (Landmark)_selectedLandmark.Clone();
            GpsInputSmallViewModel.Initialize(SelectedLandmark.GpsCoors);
            ComboItems = GetItems(SelectedLandmark.EquipmentType);
            SelectedEquipmentTypeItem = ComboItems.First(i => i.Type == SelectedLandmark.EquipmentType);
            IsEquipmentEnabled = HasPrivilevies && SelectedLandmark.EquipmentType != EquipmentType.EmptyNode &&
                                 SelectedLandmark.EquipmentType != EquipmentType.Rtu;
        }

        private List<EquipmentTypeComboItem> _comboItems;
        public List<EquipmentTypeComboItem> ComboItems
        {
            get => _comboItems;
            set
            {
                if (Equals(value, _comboItems)) return;
                _comboItems = value;
                NotifyOfPropertyChange();
            }
        }

        private List<EquipmentTypeComboItem> GetItems(EquipmentType type)
        {
            if (type == EquipmentType.Rtu) return new List<EquipmentTypeComboItem> { new EquipmentTypeComboItem(EquipmentType.Rtu) };
            if (type == EquipmentType.EmptyNode) return new List<EquipmentTypeComboItem> { new EquipmentTypeComboItem(EquipmentType.EmptyNode) };
            return new List<EquipmentTypeComboItem>
            {
                new EquipmentTypeComboItem(EquipmentType.Closure),
                new EquipmentTypeComboItem(EquipmentType.Cross),
                new EquipmentTypeComboItem(EquipmentType.Terminal),
                new EquipmentTypeComboItem(EquipmentType.CableReserve),
                new EquipmentTypeComboItem(EquipmentType.Other)
            };
        }

        public bool HasPrivilevies { get; set; }

        private bool _isEquipmentEnabled;
        private bool _isIncludeEquipmentEnabled;
        private bool _isExcludeEquipmentEnabled;
        private bool _isFromBaseRef;

        public bool IsEquipmentEnabled
        {
            get => _isEquipmentEnabled;
            set
            {
                if (value == _isEquipmentEnabled) return;
                _isEquipmentEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFromBaseRef
        {
            get => _isFromBaseRef;
            set
            {
                if (value == _isFromBaseRef) return;
                _isFromBaseRef = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility GisVisibility { get; set; }

        private bool _isEditEnabled;
        public bool IsEditEnabled
        {
            get => _isEditEnabled;
            set
            {
                if (value == _isEditEnabled) return;
                _isEditEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public OneLandmarkViewModel(CurrentUser currentUser, CurrentlyHiddenRtu currentlyHiddenRtu, CurrentGis currentGis,
            GpsInputSmallViewModel gpsInputSmallViewModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
            GraphReadModel graphReadModel, Model readModel,
            ReflectogramManager reflectogramManager, TabulatorViewModel tabulatorViewModel)
        {
            HasPrivilevies = currentUser.Role <= Role.Root;
            IsEditEnabled = true;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _tabulatorViewModel = tabulatorViewModel;
            GpsInputSmallViewModel = gpsInputSmallViewModel;
        }

        public async void Apply()
        {
            IsEditEnabled = false;
            _graphReadModel.ExtinguishNodes();
            var unused = await ApplyingProcess();
            IsEditEnabled = true;
        }

        private async Task<bool> ApplyingProcess()
        {
            var result = await ApplyEquipment();
            if (result != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            result = await ApplyNode();
            if (result != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            return true;
        }

        private async Task<string> ApplyEquipment()
        {
            if (_landmarkBeforeChanges.EquipmentTitle != SelectedLandmark.EquipmentTitle ||
                _landmarkBeforeChanges.EquipmentType != SelectedLandmark.EquipmentType)
            {
                var equipment = _readModel.Equipments.First(e => e.EquipmentId == SelectedLandmark.EquipmentId);
                return await _c2DWcfManager.SendCommandAsObj(
                    new UpdateEquipment
                    {
                        EquipmentId = SelectedLandmark.EquipmentId,
                        Title = SelectedLandmark.EquipmentTitle,
                        Type = SelectedLandmark.EquipmentType,
                        CableReserveLeft = equipment.CableReserveLeft,
                        CableReserveRight = equipment.CableReserveRight,
                        Comment = equipment.Comment,
                    });
            }
            return null;
        }

        private async Task<string> ApplyNode()
        {
            if (_landmarkBeforeChanges.NodeTitle != SelectedLandmark.NodeTitle ||
                _landmarkBeforeChanges.NodeComment != SelectedLandmark.NodeComment ||
                _landmarkBeforeChanges.GpsCoors != GpsInputSmallViewModel.Get())
            {
                var cmd = SelectedLandmark.EquipmentType == EquipmentType.Rtu
                    ? (object)new UpdateRtu()
                    {
                        RtuId = RtuId,
                        Title = SelectedLandmark.NodeTitle,
                        Comment = SelectedLandmark.NodeComment,
                        Position = GpsInputSmallViewModel.Get()
                    }
                    : new UpdateAndMoveNode
                    {
                        NodeId = SelectedLandmark.NodeId,
                        Title = SelectedLandmark.NodeTitle,
                        Comment = SelectedLandmark.NodeComment,
                        Position = GpsInputSmallViewModel.Get()
                    };
                return await _c2DWcfManager.SendCommandAsObj(cmd);
            }
            return null;
        }

        public void Cancel()
        {
            if (_landmarkBeforeChanges == null) return;
            if (_currentlyHiddenRtu.Collection.Contains(RtuId)) return;

            SelectedLandmark = _landmarkBeforeChanges;
            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == SelectedLandmark.NodeId);
            nodeVm.Position = GpsInputSmallViewModel.Get();
            _graphReadModel.ExtinguishNodes();
        }

        public void ShowLandmarkOnMap()
        {
            _graphReadModel.ExtinguishNodes();
            if (_currentlyHiddenRtu.Collection.Contains(RtuId))
            {
                //                _renderingManager.ShowOneTrace(Model.Trace);
                _currentlyHiddenRtu.Collection.Remove(RtuId);
                _currentlyHiddenRtu.ChangedRtu = RtuId;
            }

            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == SelectedLandmark.NodeId);
            nodeVm.Position = GpsInputSmallViewModel.Get();
            _graphReadModel.PlaceNodeIntoScreenCenter(SelectedLandmark.NodeId);
            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;
        }
        public void ShowReflectogram()
        {
            _reflectogramManager.SetTempFileName(TraceTitle, BaseRefType.Precise.ToString(), PreciseTimestamp);
            _reflectogramManager.ShowBaseReflectogramWithSelectedLandmark(SorFileId, SelectedLandmark.Number + 1);
        }


    }
}
