using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class SorBytesDto
    {
        [DataMember]
        public string ClientIp { get; set; }
        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }

        [DataMember]
        public Guid Id { get; set; }
    }
}