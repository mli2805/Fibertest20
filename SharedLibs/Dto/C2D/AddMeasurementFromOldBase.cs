using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class AddMeasurementFromOldBase
    {
        [DataMember]
        public Guid TraceId { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }
        [DataMember]
        public BaseRefType BaseRefType { get; set; }
        [DataMember]
        public FiberState TraceState { get; set; }
        [DataMember]
        public DateTime MeasurementTimestamp { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}