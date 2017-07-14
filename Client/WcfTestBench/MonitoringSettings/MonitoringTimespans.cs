using System;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringTimespans
    {
        public TimeSpan PreciseMeas { get; set; } = TimeSpan.Zero;
        public TimeSpan PreciseSave { get; set; } = TimeSpan.Zero;
        public TimeSpan FastSave { get; set; } = TimeSpan.Zero;
    }
}