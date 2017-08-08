using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class SaveMonitoringResultDto
    {
        [DataMember]
        public Guid RtuId { get; set; }
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