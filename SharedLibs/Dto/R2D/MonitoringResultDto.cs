using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    [DataContract]
    public class MonitoringResultDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public OtauPortDto OtauPort { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public FiberState TraceState { get; set; }

        [DataMember]
        public byte[] SorData { get; set; }
    }
}