using System.Collections.Generic;
using System.Linq;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringCharonModel
    {
        public string Title { get; set; }
        public List<MonitoringPortModel> Ports { get; set; } = new List<MonitoringPortModel>();

        public int GetCycleTime()
        {
            return Ports.Where(p => p.IsIncluded).Sum(p => (int)p.FastBaseSpan.TotalSeconds) +
                   Ports.Count(p => p.IsIncluded) * 2; // 2 sec for toggle port
        }
    }
}