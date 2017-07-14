using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        public string CycleFullTime { get; set; }

        public MonitoringSettingsModel Model { get; set; }

        public int SelectedTabIndex { get; set; }

        public MonitoringSettingsViewModel(MonitoringSettingsModel model)
        {
            Model = model;
            CycleFullTime = Model.GetCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Monitoring settings";
        }

        public void Apply()
        {
            SelectedTabIndex = 1;
        }
    }
}
