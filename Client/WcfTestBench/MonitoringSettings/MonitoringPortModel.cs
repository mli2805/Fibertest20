using System;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringPortModel
    {
        public int PortNumber { get; set; }
        public string TraceTitle { get; set; }
        public TimeSpan PreciseBaseSpan { get; set; } = TimeSpan.Zero;
        public TimeSpan FastBaseSpan { get; set; } = TimeSpan.Zero;
        public TimeSpan AdditionalBaseSpan { get; set; } = TimeSpan.Zero;
        public bool IsIncluded { get; set; }
        public bool IsAnyBaseAssigned => PreciseBaseSpan != TimeSpan.Zero || FastBaseSpan != TimeSpan.Zero;

        public string Duration => FastBaseSpan.TotalSeconds + " / " + PreciseBaseSpan.TotalSeconds + " sec";
    }
}
