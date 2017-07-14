using System;
using System.Collections.Generic;
using System.Linq;

namespace WcfTestBench.MonitoringSettings
{
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
}