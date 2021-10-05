using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class VeexTestCreatedDto
    {
        [DataMember]
        public Guid TestId { get; set; }
        [DataMember]
        public Guid TraceId { get; set; }
        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public bool IsOnBop { get; set; }
        [DataMember]
        public string OtauId { get; set; }

        [DataMember]
        public DateTime CreationTimestamp { get; set; }

    }
}