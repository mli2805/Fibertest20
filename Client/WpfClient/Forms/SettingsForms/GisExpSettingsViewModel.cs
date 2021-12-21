using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class GisExpSettingsViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly CurrentGis _currentGis;

        public int ThresholdZoom { get; set; }
        public int ThresholdNodeCount { get; set; }

        public bool IsZoom { get; set; }
        public bool IsNodeCount { get; set; }

        public GisExpSettingsViewModel(IniFile iniFile, CurrentGis currentGis)
        {
            _iniFile = iniFile;
            _currentGis = currentGis;

            IsZoom = currentGis.GisRenderingByZoom;
            IsNodeCount = !IsZoom;
            ThresholdZoom = currentGis.ThresholdZoom;
            ThresholdNodeCount = currentGis.ThresholdNodeCount;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"GIS Experiment settings";
        }

        public void Save()
        {
            _currentGis.GisRenderingByZoom = IsZoom;
            _currentGis.ThresholdZoom = ThresholdZoom;
            _currentGis.ThresholdNodeCount = ThresholdNodeCount;
            _iniFile.Write(IniSection.Map, IniKey.GisRenderingByZoom, IsZoom);
            _iniFile.Write(IniSection.Map, IniKey.ThresholdZoom, ThresholdZoom);
            _iniFile.Write(IniSection.Map, IniKey.ThresholdNodeCount, ThresholdNodeCount);

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
