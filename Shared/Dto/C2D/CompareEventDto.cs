using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class CompareEventDto
    {
        [DataMember]
        public int Revision { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public string ClientIp { get; set; }
    }
}