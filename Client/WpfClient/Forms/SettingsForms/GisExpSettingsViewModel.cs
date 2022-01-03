using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class GisExpSettingsViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly CurrentGis _currentGis;
       
        private bool _isBigGraphMode;

        public bool IsBigGraphMode
        {
            get => _isBigGraphMode;
            set
            {
                if (value == _isBigGraphMode) return;
                _isBigGraphMode = value;
                SelectedZoom = _isBigGraphMode ? 16 : 12;
                ZoomList = _isBigGraphMode ? Enumerable.Range(10, 11).ToList() : Enumerable.Range(6, 11).ToList();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ZoomList));
                NotifyOfPropertyChange(nameof(SelectedZoom));
            }
        }

        private List<int> _zoomList = Enumerable.Range(7, 13).ToList();
        public List<int> ZoomList
        {
            get => _zoomList;
            private set
            {
                if (Equals(value, _zoomList)) return;
                _zoomList = value;
                NotifyOfPropertyChange();
            }
        }

        private int _selectedZoom;
        public int SelectedZoom
        {
            get => _selectedZoom;
            set
            {
                if (value == _selectedZoom) return;
                _selectedZoom = value;
                NotifyOfPropertyChange();
            }
        }


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
