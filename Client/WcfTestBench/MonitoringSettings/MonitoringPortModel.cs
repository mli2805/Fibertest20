using System;
using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringPortModel : PropertyChangedBase
    {
        private bool _isIncluded;
        public int PortNumber { get; set; }
        public string TraceTitle { get; set; }
        public TimeSpan PreciseBaseSpan { get; set; } = TimeSpan.Zero;
        public TimeSpan FastBaseSpan { get; set; } = TimeSpan.Zero;
        public TimeSpan AdditionalBaseSpan { get; set; } = TimeSpan.Zero;

        public bool IsIncluded
        {
            get { return _isIncluded; }
            set
            {
                if (value == _isIncluded) return;
                _isIncluded = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsAnyBaseAssigned => PreciseBaseSpan != TimeSpan.Zero || FastBaseSpan != TimeSpan.Zero;

        public string Duration => FastBaseSpan.TotalSeconds + " / " + PreciseBaseSpan.TotalSeconds + " sec";
    }
}
