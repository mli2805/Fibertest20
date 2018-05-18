using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class UnRegisterClientDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string ClientIp { get; set; }
    }
}