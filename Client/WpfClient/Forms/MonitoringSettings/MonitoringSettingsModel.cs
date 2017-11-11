using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsModel : PropertyChangedBase
    {
        public Guid RtuId { get; set; }
        public List<MonitoringCharonModel> Charons { get; set; } = new List<MonitoringCharonModel>();
        public MonitoringFrequenciesModel Frequencies { get; set; } = new MonitoringFrequenciesModel();
        public bool IsMonitoringOn { get; set; }


        public void CalculateCycleTime()
        {
            foreach (var charon in Charons)
            {
                charon.SubscribeOnPortsChanges();
                charon.PropertyChanged += Charon_PropertyChanged;
            }
            CycleTime = TimeSpan.FromSeconds(Charons.Sum(c => c.CycleTime)).ToString();
        }

        private void Charon_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CycleTime")
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

    }
}