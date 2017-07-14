using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringCharonModel
    {
        public string Title { get; set; }
        public List<MonitoringPortModel> Ports { get; set; } = new List<MonitoringPortModel>();

        public int GetCycleTime()
        {
            return Ports.Where(p => p.IsIncluded).Sum(p => p.FastBaseSpan.Seconds) +
                   Ports.Count(p => p.IsIncluded) * 2; // 2 sec for toggle port
        }
    }
    public class MonitoringTimespans
    {
        public TimeSpan PreciseMeas { get; set; } = TimeSpan.Zero;
        public TimeSpan PreciseSave { get; set; } = TimeSpan.Zero;
        public TimeSpan FastSave { get; set; } = TimeSpan.Zero;
    }
    public class MonitoringSettingsModel
    {
        public List<MonitoringCharonModel> Charons { get; set; } = new List<MonitoringCharonModel>();
        public MonitoringTimespans Timespans { get; set; } = new MonitoringTimespans();
        public bool IsMonitoringOn { get; set; }

        public string GetCycleTime()
        {
            return TimeSpan.FromSeconds(Charons.Sum(c => c.GetCycleTime())).ToString();
        }
    }
    public class MonitoringSettingsViewModel : Screen
    {
        public List<MonitoringPortModel> Ports { get; set; } // for binding
        public string CharonAddress { get; set; }
        public string CycleFullTime { get; set; }

        public MonitoringSettingsModel Model { get; set; }

        public MonitoringSettingsViewModel(MonitoringSettingsModel model)
        {
            Model = model;
            CycleFullTime = Model.GetCycleTime();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Monitoring settings";
        }
    }
}
