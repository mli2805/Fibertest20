using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class GisExpSettingsViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly CurrentGis _currentGis;

        public bool IsBigGraphMode { get; set; }

        public List<int> ZoomList { get; } = new List<int>() {15, 16, 17, 18, 19};
        public int SelectedZoom { get; set; }


        public List<int> ShiftList { get; } = new List<int>() {16, 28, 40, 52};
        public int SelectedShift { get; set; }

        public GisExpSettingsViewModel(IniFile iniFile, CurrentGis currentGis)
        {
            _iniFile = iniFile;
            _currentGis = currentGis;

            IsBigGraphMode = currentGis.IsBigGraphMode;
            SelectedZoom = currentGis.ThresholdZoom;
            SelectedShift = (int)(currentGis.ScreenPartAsMargin * 100);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"GIS Experiment settings";
        }

        public void Save()
        {
            _currentGis.IsBigGraphMode = IsBigGraphMode;
            _iniFile.Write(IniSection.Map, IniKey.IsBigGraphMode, IsBigGraphMode);
            _currentGis.ThresholdZoom = SelectedZoom;
            _iniFile.Write(IniSection.Map, IniKey.ThresholdZoom, SelectedZoom);
            _currentGis.ScreenPartAsMargin = SelectedShift / 100.0;
            _iniFile.Write(IniSection.Map, IniKey.ScreenPartAsMargin, _currentGis.ScreenPartAsMargin);

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
