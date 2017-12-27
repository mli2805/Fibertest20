using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DetachOtauDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public Guid OtauId { get; set; }

        [DataMember]
        public NetAddress OtauAddresses { get; set; }

        [DataMember]
        public int OpticalPort { get; set; }
    }
}