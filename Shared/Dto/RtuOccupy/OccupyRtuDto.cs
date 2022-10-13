using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class OccupyRtuDto
    {
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public RtuOccupationState State { get; set; }

    }
}
