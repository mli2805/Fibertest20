using Caliburn.Micro;
using GMap.NET;

namespace Iit.Fibertest.Client
{
    public class GpsInputSmallViewModel : PropertyChangedBase
    {
        private readonly CurrentGpsInputMode _currentGpsInputMode;

        private OneCoorViewModel _oneCoorViewModelLatitude;
        public OneCoorViewModel OneCoorViewModelLatitude
        {
            get => _oneCoorViewModelLatitude;
            set
            {
                if (Equals(value, _oneCoorViewModelLatitude)) return;
                _oneCoorViewModelLatitude = value;
                NotifyOfPropertyChange();
            }
        }

        private OneCoorViewModel _oneCoorViewModelLongitude;
        public OneCoorViewModel OneCoorViewModelLongitude
        {
            get => _oneCoorViewModelLongitude;
            set
            {
                if (Equals(value, _oneCoorViewModelLongitude)) return;
                _oneCoorViewModelLongitude = value;
                NotifyOfPropertyChange();
            }
        }

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

        public PointLatLng Get()
        {
            return new PointLatLng(OneCoorViewModelLatitude.StringsToValue(), OneCoorViewModelLongitude.StringsToValue());
        }
    }
}
