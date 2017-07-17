using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringCharonModel : PropertyChangedBase
    {
        public string Title { get; set; }

        private bool _groupenCheck;
        public bool GroupenCheck
        {
            get { return _groupenCheck; }
            set
            {
                _groupenCheck = value;
                foreach (var port in Ports.Where(p=>p.IsAnyBaseAssigned))
                {
                    port.IsIncluded = _groupenCheck;
                }
            }
        }

        public List<MonitoringPortModel> Ports { get; set; } = new List<MonitoringPortModel>();


        public void F()
        {
            foreach (var port in Ports)
            {
                port.PropertyChanged += Port_PropertyChanged;
            }
        }

        private void Port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsIncluded")
                NotifyOfPropertyChange(nameof(CycleTime));
        }

        public int CycleTime => 
            Ports.Where(p => p.IsIncluded).Sum(p => (int)p.FastBaseSpan.TotalSeconds) +
                   Ports.Count(p => p.IsIncluded) * 2; // 2 sec for toggle port
        
    }
}