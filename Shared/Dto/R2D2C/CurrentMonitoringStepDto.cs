using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class CurrentMonitoringStepDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public MonitoringCurrentStep Step { get; set; }
        [DataMember]
        public PortWithTraceDto PortWithTraceDto { get; set; }
        [DataMember]
        public BaseRefType BaseRefType { get; set; }
    }
}
