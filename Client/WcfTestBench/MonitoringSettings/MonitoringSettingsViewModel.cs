using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        public MonitoringSettingsModel Model { get; set; }

        public int SelectedTabIndex { get; set; }

        public MonitoringSettingsViewModel(MonitoringSettingsModel model)
        {

            Model = model;
            Model.CalculateCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Monitoring settings";
        }

        public void Apply()
        {
        }
    }
}
