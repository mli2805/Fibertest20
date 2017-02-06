using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using GMap.NET;

namespace Iit.Fibertest.TestBench
{
    public class GpsInputViewModel : PropertyChangedBase
    {
        private GpsInputModeComboItem _selectedGpsInputMode;

        public OneCoorViewModel OneCoorViewModelLatitude { get; set; }
        public OneCoorViewModel OneCoorViewModelLongitude { get; set; }

        public PointLatLng Coors { get; set; }

        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
        (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>().ToList()
            select new GpsInputModeComboItem(mode)).ToList();

        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get { return _selectedGpsInputMode; }
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                OneCoorViewModelLatitude.CurrentGpsInputMode = value.Mode;
                OneCoorViewModelLongitude.CurrentGpsInputMode = value.Mode;
            }
        }

        public void Cancel()
        {
            OneCoorViewModelLatitude.Value = Coors.Lat;
            OneCoorViewModelLongitude.Value = Coors.Lng;
        }

        public GpsInputViewModel(GpsInputMode mode, PointLatLng coors)
        {
            Coors = coors;
            _selectedGpsInputMode = GpsInputModes.First(item => item.Mode == mode);

            OneCoorViewModelLatitude = new OneCoorViewModel(SelectedGpsInputMode.Mode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(SelectedGpsInputMode.Mode, Coors.Lng);
        }
    }
}
