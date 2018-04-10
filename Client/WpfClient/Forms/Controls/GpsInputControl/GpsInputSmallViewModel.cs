using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GpsInputSmallViewModel
    {
        private readonly CurrentGpsInputMode _currentGpsInputMode;
        public OneCoorViewModel OneCoorViewModelLatitude { get; set; }
        public OneCoorViewModel OneCoorViewModelLongitude { get; set; }

        public PointLatLng Coors { get; set; }

        public void DropChanges()
        {
            OneCoorViewModelLatitude.ReassignValue(Coors.Lat);
            OneCoorViewModelLongitude.ReassignValue(Coors.Lng);
        }

        public GpsInputSmallViewModel(CurrentGpsInputMode currentGpsInputMode)
        {
            _currentGpsInputMode = currentGpsInputMode;
            currentGpsInputMode.PropertyChanged += CurrentGpsInputMode_PropertyChanged;
        }

        private void CurrentGpsInputMode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OneCoorViewModelLatitude.CurrentGpsInputMode = ((CurrentGpsInputMode) sender).Mode;
            OneCoorViewModelLongitude.CurrentGpsInputMode = ((CurrentGpsInputMode) sender).Mode;
        }

        public void Initialize(PointLatLng coors)
        {
            Coors = coors;

            OneCoorViewModelLatitude = new OneCoorViewModel(_currentGpsInputMode.Mode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(_currentGpsInputMode.Mode, Coors.Lng);
        }
    }
}
