﻿using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class CurrentGis : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        private GpsInputMode _gpsInputMode;

        public GpsInputMode GpsInputMode
        {
            get => _gpsInputMode;
            set
            {
                if (value == _gpsInputMode) return;
                _gpsInputMode = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.GpsInputMode, (int)_gpsInputMode);
                NotifyOfPropertyChange(nameof(GpsInputMode));
            }
        }

        private bool _isWithoutMapMode;
        public bool IsWithoutMapMode
        {
            get { return _isWithoutMapMode; }
            set
            {
                if (value == _isWithoutMapMode) return;
                _isWithoutMapMode = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsGisOn));
            }
        }

        private bool _isRootTempGisOn;
        public bool IsRootTempGisOn
        {
            get { return _isRootTempGisOn; }
            set
            {
                if (value == _isRootTempGisOn) return;
                _isRootTempGisOn = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsGisOn));
            }
        }

        public bool IsGisOn => !IsWithoutMapMode || IsRootTempGisOn;

        public CurrentGis(IniFile iniFile)
        {
            _iniFile = iniFile;
            _gpsInputMode = (GpsInputMode)iniFile.Read(IniSection.Miscellaneous, IniKey.GpsInputMode, 0);
        }
    }

}