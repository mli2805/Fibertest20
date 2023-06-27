using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    [DataContract]
    public class MonitoringResultDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public PortWithTraceDto PortWithTrace { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public FiberState TraceState { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}