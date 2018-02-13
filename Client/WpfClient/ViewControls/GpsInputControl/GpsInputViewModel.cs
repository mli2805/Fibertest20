using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class GpsInputViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;

        public OneCoorViewModel OneCoorViewModelLatitude { get; set; }
        public OneCoorViewModel OneCoorViewModelLongitude { get; set; }

        public PointLatLng Coors { get; set; }

        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
        (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
         select new GpsInputModeComboItem(mode)).ToList();

        private readonly GpsInputMode _modeInIniFile;
        private GpsInputModeComboItem _selectedGpsInputMode;
        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get => _selectedGpsInputMode;
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                OneCoorViewModelLatitude.CurrentGpsInputMode = value.Mode;
                OneCoorViewModelLongitude.CurrentGpsInputMode = value.Mode;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.GpsInputMode, (byte)_selectedGpsInputMode.Mode);
            }
        }

        public void DropChanges()
        {
            OneCoorViewModelLatitude.ReassignValue(Coors.Lat);
            OneCoorViewModelLongitude.ReassignValue(Coors.Lng);
        }

        public GpsInputViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;
            _modeInIniFile = (GpsInputMode)_iniFile.Read(IniSection.Miscellaneous, IniKey.GpsInputMode, 2);
            _selectedGpsInputMode = _modeInIniFile == GpsInputMode.Degrees
                ? new GpsInputModeComboItem(GpsInputMode.DegreesAndMinutes)
                : new GpsInputModeComboItem(GpsInputMode.Degrees);
        }
        public void Initialize(PointLatLng coors)
        {
            Coors = coors;

            OneCoorViewModelLatitude = new OneCoorViewModel(SelectedGpsInputMode.Mode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(SelectedGpsInputMode.Mode, Coors.Lng);
            SelectedGpsInputMode = GpsInputModes.FirstOrDefault(i=>i.Mode == _modeInIniFile);
        }


    }
}
