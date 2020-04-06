using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DetachOtauDto
    {
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public Guid OtauId { get; set; }

        [DataMember]
        public NetAddress OtauAddress { get; set; }

        [DataMember]
        public int OpticalPort { get; set; }
    }
}