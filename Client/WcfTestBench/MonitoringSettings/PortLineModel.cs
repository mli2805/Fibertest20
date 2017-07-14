using System;

namespace WcfTestBench.MonitoringSettings
{
    public class PortLineModel
    {
        public int PortNumber { get; set; }
        public string TraceTitle { get; set; }
        public TimeSpan PreciseBaseSpan { get; set; } = TimeSpan.Zero;
        public TimeSpan FastBaseSpan { get; set; } = TimeSpan.Zero;
        public TimeSpan AdditionalBaseSpan { get; set; } = TimeSpan.Zero;
        public bool IsMonitoringOn { get; set; }
        public bool IsAnyBaseAssigned => PreciseBaseSpan != TimeSpan.Zero || FastBaseSpan != TimeSpan.Zero;

        public string Duration => FastBaseSpan.Seconds + " / " + PreciseBaseSpan.Seconds;
    }
}
