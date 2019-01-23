using Caliburn.Micro;
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

        public CurrentGis(IniFile iniFile)
        {
            _iniFile = iniFile;
            _gpsInputMode = (GpsInputMode)iniFile.Read(IniSection.Miscellaneous, IniKey.GpsInputMode, 0);
        }
    }

}