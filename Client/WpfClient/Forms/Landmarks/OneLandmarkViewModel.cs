using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class OneLandmarkViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;

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

        private EquipmentTypeComboItem _selectedEquipmentTypeItem;
        public EquipmentTypeComboItem SelectedEquipmentTypeItem
        {
            get => _selectedEquipmentTypeItem;
            set
            {
                if (Equals(value, _selectedEquipmentTypeItem) || value == null) return;
                _selectedEquipmentTypeItem = value;
                LandmarkUnderWork.EquipmentType = value.Type;
                NotifyOfPropertyChange();
            }
        }

        private Landmark _landmarkUnderWork;
        public Landmark LandmarkUnderWork
        {
            get => _landmarkUnderWork;
            set
            {
                if (value == null) return;
                _landmarkUnderWork = value;
                NotifyOfPropertyChange();
            }
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
            if (type == EquipmentType.Rtu) return new List<EquipmentTypeComboItem>
                { new EquipmentTypeComboItem(EquipmentType.Rtu) };
            if (type == EquipmentType.EmptyNode) return new List<EquipmentTypeComboItem>
                { new EquipmentTypeComboItem(EquipmentType.EmptyNode) };
            return new List<EquipmentTypeComboItem>
            {
                new EquipmentTypeComboItem(EquipmentType.Closure),
                new EquipmentTypeComboItem(EquipmentType.Cross),
                new EquipmentTypeComboItem(EquipmentType.Terminal),
                new EquipmentTypeComboItem(EquipmentType.CableReserve),
                new EquipmentTypeComboItem(EquipmentType.Other)
            };
        }

        public bool HasPrivileges { get; set; }

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

        public OneLandmarkViewModel(CurrentUser currentUser, CurrentGis currentGis,
             GpsInputSmallViewModel gpsInputSmallViewModel, IWindowManager windowManager)
        {
            HasPrivileges = currentUser.Role <= Role.Root;
            IsEditEnabled = true;
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            _windowManager = windowManager;
            GpsInputSmallViewModel = gpsInputSmallViewModel;
        }

        public void Initialize(Landmark selectedLandmark)
        {
            LandmarkUnderWork = selectedLandmark;

            GpsInputSmallViewModel.Initialize(selectedLandmark.GpsCoors);
            ComboItems = GetItems(selectedLandmark.EquipmentType);
            SelectedEquipmentTypeItem = ComboItems.First(i => i.Type == selectedLandmark.EquipmentType);
            // IsEquipmentEnabled = HasPrivileges && selectedLandmark.EquipmentType != EquipmentType.EmptyNode &&
            //                      selectedLandmark.EquipmentType != EquipmentType.Rtu;
            // IsUserInputEnabled = HasPrivileges && selectedLandmark.EquipmentType != EquipmentType.Rtu;
        }


        public Landmark GetLandmark()
        {
            var errorMessage = GpsInputSmallViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return null;
            }

            LandmarkUnderWork.GpsCoors = position;
            return LandmarkUnderWork.Clone();
        }
    }
}
