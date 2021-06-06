using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    [DataContract]
    public class PortWithTraceDto
    {
        [DataMember]
        public OtauPortDto OtauPort { get; set; }

        [DataMember]
        public Guid TraceId { get; set; }
    }
}