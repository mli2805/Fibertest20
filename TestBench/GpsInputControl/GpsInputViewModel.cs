using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using GMap.NET;

namespace Iit.Fibertest.TestBench
{
    public class GpsInputViewModel : PropertyChangedBase
    {
        private GpsInputMode _selectedGpsInputMode;

        public OneCoorViewModel OneCoorViewModelLatitude { get; set; }
        public OneCoorViewModel OneCoorViewModelLongitude { get; set; }

        public PointLatLng Coors { get; set; }

        public List<GpsInputMode> GpsInputModes { get; set; } =
            Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>().ToList();

        public GpsInputMode SelectedGpsInputMode
        {
            get { return _selectedGpsInputMode; }
            set
            {
                if (value == _selectedGpsInputMode) return;
                _selectedGpsInputMode = value;
                OneCoorViewModelLatitude.CurrentGpsInputMode = value;
                OneCoorViewModelLongitude.CurrentGpsInputMode = value;
            }
        }

        public void Cancel()
        {
            
        }

        public GpsInputViewModel(GpsInputMode mode, PointLatLng coors)
        {
            Coors = coors;
            _selectedGpsInputMode = mode;

            OneCoorViewModelLatitude = new OneCoorViewModel(SelectedGpsInputMode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(SelectedGpsInputMode, Coors.Lng);
        }
    }
}
