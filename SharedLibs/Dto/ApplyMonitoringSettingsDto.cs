using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class ApplyMonitoringSettingsDto
    {
        [DataMember]
        public string RtuIpAddress { get; set; }

        [DataMember]
        public bool IsMonitoringOn { get; set; }

        [DataMember]
        public MonitoringTimespansDto Timespans { get; set; }

        [DataMember]
        public List<IsPortMonitoringOn> Ports { get; set; }
    }
}