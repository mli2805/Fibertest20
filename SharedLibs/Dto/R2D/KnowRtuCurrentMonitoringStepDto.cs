using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class KnowRtuCurrentMonitoringStepDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public RtuCurrentMonitoringStep MonitoringStep { get; set; }
        [DataMember]
        public OtauPortDto OtauPort { get; set; }
        [DataMember]
        public BaseRefType BaseRefType { get; set; }
    }
}
