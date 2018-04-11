using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class OneLandmarkViewModel : PropertyChangedBase
    {
        private readonly ReflectogramManager _reflectogramManager;
        private GpsInputSmallViewModel _gpsInputSmallViewModel;
        private Landmark _selectedLandmark;

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

        public Landmark SelectedLandmark
        {
            get => _selectedLandmark;
            set
            {
                if (Equals(value, _selectedLandmark) || value == null) return;
                _selectedLandmark = value;
                GpsInputSmallViewModel.Initialize(SelectedLandmark.GpsCoors);
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
