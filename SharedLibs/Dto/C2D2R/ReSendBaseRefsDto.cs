using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ReSendBaseRefsDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public Guid TraceId { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; } // could be null if trace isn't attached to port yet
    }
}