using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GpsInputViewModel : PropertyChangedBase
    {
        private readonly CurrentGpsInputMode _currentGpsInputMode;

        public OneCoorViewModel OneCoorViewModelLatitude { get; set; }
        public OneCoorViewModel OneCoorViewModelLongitude { get; set; }

        public PointLatLng Coors { get; set; }

        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
        (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
         select new GpsInputModeComboItem(mode)).ToList();

        private readonly GpsInputMode _modeInIniFile;
        private GpsInputModeComboItem _selectedGpsInputModeComboItem;
        public GpsInputModeComboItem SelectedGpsInputModeComboItem
        {
            get => _selectedGpsInputModeComboItem;
            set
            {
                if (Equals(value, _selectedGpsInputModeComboItem)) return;
                _selectedGpsInputModeComboItem = value;
                OneCoorViewModelLatitude.CurrentGpsInputMode = value.Mode;
                OneCoorViewModelLongitude.CurrentGpsInputMode = value.Mode;
                _currentGpsInputMode.Mode = _selectedGpsInputModeComboItem.Mode;
            }
        }

        public void DropChanges()
        {
            OneCoorViewModelLatitude.ReassignValue(Coors.Lat);
            OneCoorViewModelLongitude.ReassignValue(Coors.Lng);
        }

        public GpsInputViewModel(CurrentGpsInputMode currentGpsInputMode)
        {
            _currentGpsInputMode = currentGpsInputMode;
            _modeInIniFile = currentGpsInputMode.Mode;
            _selectedGpsInputModeComboItem = _modeInIniFile == GpsInputMode.Degrees
                ? new GpsInputModeComboItem(GpsInputMode.DegreesAndMinutes)
                : new GpsInputModeComboItem(GpsInputMode.Degrees);
        }

        public void Initialize(PointLatLng coors)
        {
            Coors = coors;

            OneCoorViewModelLatitude = new OneCoorViewModel(SelectedGpsInputModeComboItem.Mode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(SelectedGpsInputModeComboItem.Mode, Coors.Lng);
            SelectedGpsInputModeComboItem = GpsInputModes.FirstOrDefault(i=>i.Mode == _modeInIniFile);
        }

        public PointLatLng Get()
        {
            return new PointLatLng(OneCoorViewModelLatitude.StringsToValue(), OneCoorViewModelLongitude.StringsToValue());
        }

    }
}
