using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EquipmentTypeComboItem
    {
        public EquipmentType Type { get; set; }

        public EquipmentTypeComboItem(EquipmentType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToLocalizedString();
        }
    }
    public class OneLandmarkViewModel : PropertyChangedBase
    {
        private readonly ReflectogramManager _reflectogramManager;

        public List<EquipmentTypeComboItem> ComboItems { get; set; } 
            = new List<EquipmentTypeComboItem>()
            {
                new EquipmentTypeComboItem(EquipmentType.EmptyNode),
                new EquipmentTypeComboItem(EquipmentType.CableReserve),
                new EquipmentTypeComboItem(EquipmentType.Closure),
                new EquipmentTypeComboItem(EquipmentType.Cross),
                new EquipmentTypeComboItem(EquipmentType.Other),
                new EquipmentTypeComboItem(EquipmentType.Terminal),
                new EquipmentTypeComboItem(EquipmentType.Rtu)
            };

        private EquipmentTypeComboItem _selectedEquipmentTypeItem;
        public EquipmentTypeComboItem SelectedEquipmentTypeItem
        {
            get => _selectedEquipmentTypeItem;
            set
            {
                if (Equals(value, _selectedEquipmentTypeItem)) return;
                _selectedEquipmentTypeItem = value;
                SelectedLandmark.EquipmentType = value.Type;
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

        private Landmark _selectedLandmark;
        private bool _isEquipmentEnabled;

        public Landmark SelectedLandmark
        {
            get => _selectedLandmark;
            set
            {
                if (Equals(value, _selectedLandmark) || value == null) return;
                _selectedLandmark = value;
                GpsInputSmallViewModel.Initialize(SelectedLandmark.GpsCoors);
                SelectedEquipmentTypeItem = ComboItems.First(i => i.Type == SelectedLandmark.EquipmentType);
                IsEquipmentEnabled = SelectedLandmark.EquipmentType != EquipmentType.EmptyNode &&
                                     SelectedLandmark.EquipmentType != EquipmentType.Rtu;
                NotifyOfPropertyChange();
            }
        }

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

        public OneLandmarkViewModel(GpsInputSmallViewModel gpsInputSmallViewModel, ReflectogramManager reflectogramManager)
        {
            _reflectogramManager = reflectogramManager;
            GpsInputSmallViewModel = gpsInputSmallViewModel;
        }

        public void Apply()
        {

        }

        public void Cancel()
        {

        }

        public void ShowLandmarkOnMap()
        {

        }
        public void ShowReflectogram()
        {
        }
    }
}
