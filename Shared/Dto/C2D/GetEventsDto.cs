using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class GetEventsDto
    {
        [DataMember]
        public int Revision { get; set; }

        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string ClientIp { get; set; }
    }
}