using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class DbOptimizationModel : PropertyChangedBase
    {
        private int _measurementsNotEvents;

        public int MeasurementsNotEvents
        {
            get => _measurementsNotEvents;
            set
            {
                if (value == _measurementsNotEvents) return;
                _measurementsNotEvents = value;
                NotifyOfPropertyChange();
            }
        }

        private int _opticalEvents;
        public int OpticalEvents
        {
            get => _opticalEvents;
            set
            {
                if (value == _opticalEvents) return;
                _opticalEvents = value;
                NotifyOfPropertyChange();
            }
        }

        private int _networkEvents;
        public int NetworkEvents
        {
            get => _networkEvents;
            set
            {
                if (value == _networkEvents) return;
                _networkEvents = value;
                NotifyOfPropertyChange();
            }
        }

        private string _driveSize;
        public string DriveSize
        {
            get => _driveSize;
            set
            {
                if (value.Equals(_driveSize)) return;
                _driveSize = value;
                NotifyOfPropertyChange();
            }
        }


        private string _availableFreeSpace;
        public string AvailableFreeSpace
        {
            get => _availableFreeSpace;
            set
            {
                if (value == _availableFreeSpace) return;
                _availableFreeSpace = value;
                NotifyOfPropertyChange();
            }
        }

        private string _freeSpaceThreshold;
        public string FreeSpaceThreshold
        {
            get => _freeSpaceThreshold;
            set
            {
                if (value == _freeSpaceThreshold) return;
                _freeSpaceThreshold = value;
                NotifyOfPropertyChange();
            }
        }
    }

}