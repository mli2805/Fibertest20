using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringSettingsModel : PropertyChangedBase
    {
        public List<MonitoringCharonModel> Charons { get; set; } = new List<MonitoringCharonModel>();
        public MonitoringTimespans Timespans { get; set; } = new MonitoringTimespans();
        public bool IsMonitoringOn { get; set; }

        public void F()
        {
            foreach (var charon in Charons)
            {
                charon.F();
                charon.PropertyChanged += Charon_PropertyChanged;
            }
            CycleTime = TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();
        }

        private void Charon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CycleTime")
                //                NotifyOfPropertyChange(CycleTime);
                CycleTime = TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();
        }

        private string _cycleTime;
        public string CycleTime
        {
            get { return _cycleTime; }
            set
            {
                if (value == _cycleTime) return;
                _cycleTime = value;
                NotifyOfPropertyChange();
            }
        }


//        public string CycleTime => TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();
    }
}