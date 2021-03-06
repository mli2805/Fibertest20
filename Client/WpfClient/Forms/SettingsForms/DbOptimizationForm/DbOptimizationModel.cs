﻿using System;
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
            get => _dataSize;
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


        #region remove sor
        public DateTime UpToLimit{ get; set; } 

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (value.Equals(_selectedDate)) return;
                _selectedDate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(UpToLimit));
            }
        }
        #endregion

        #region make snapshot
        public DateTime FromLimit2{ get; set; } 
        public DateTime UpToLimit2{ get; set; } 

        private DateTime _selectedDate2;
        public DateTime SelectedDate2
        {
            get { return _selectedDate2; }
            set
            {
                if (value.Equals(_selectedDate2)) return;
                _selectedDate2 = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(FromLimit2));
                NotifyOfPropertyChange(nameof(UpToLimit2));
            }
        }
        #endregion

        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }
    }

}