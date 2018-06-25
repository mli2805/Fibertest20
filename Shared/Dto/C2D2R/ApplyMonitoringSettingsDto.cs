using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ApplyMonitoringSettingsDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsMonitoringOn { get; set; }

        [DataMember]
        public MonitoringTimespansDto Timespans { get; set; }

        [DataMember]
        public List<PortWithTraceDto> Ports { get; set; }
    }
}