using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ApplyMonitoringSettingsDto
    {
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }
        [DataMember]
        public RtuMaker RtuMaker { get; set; }
        [DataMember]
        public string OtdrId { get; set; }

        [DataMember]
        public string OtauId { get; set; }

        [DataMember]
        public bool IsMonitoringOn { get; set; }

        [DataMember]
        public MonitoringTimespansDto Timespans { get; set; }

        [DataMember]
        public List<PortWithTraceDto> Ports { get; set; }
    }
}