using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class CurrentGpsInputMode
    {
        private readonly IniFile _iniFile;
        private GpsInputMode _mode;

        public GpsInputMode Mode
        {
            get => _mode;
            set
            {
                if (value == _mode) return;
                _mode = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.GpsInputMode, (int)_mode);
            }
        }

        public CurrentGpsInputMode(IniFile iniFile)
        {
            _iniFile = iniFile;
            _mode = (GpsInputMode)iniFile.Read(IniSection.Miscellaneous, IniKey.GpsInputMode, 0);
        }
    }
}