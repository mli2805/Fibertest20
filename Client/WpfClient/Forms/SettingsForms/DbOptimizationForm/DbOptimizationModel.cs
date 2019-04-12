using System;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class DbOptimizationModel : PropertyChangedBase
    {
        #region statistics information
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

        private string _dataSize;
        public string DataSize
        {
            get { return _dataSize; }
            set
            {
                if (value == _dataSize) return;
                _dataSize = value;
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
        #endregion

        public bool IsRemoveMode { get; set; } = true;
        public  bool IsSnapshotMode { get; set; }

        public bool IsMeasurements { get; set; }
        public bool IsOpticalEvents { get; set; }
        public bool IsNetworkEvents { get; set; }

     //   public DateTime UpToLimit{ get; set; } = new DateTime(DateTime.Today.Year - 2, 12, 31);
        public DateTime UpToLimit{ get; set; } = DateTime.Today;

   //     private DateTime _selectedDate = new DateTime(DateTime.Today.Year - 2, 12, 31);
        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                if (value.Equals(_selectedDate)) return;
                _selectedDate = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }
    }

}