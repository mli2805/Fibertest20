using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class HeartbeatDto
    {
        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public string ClientIp { get; set; }
    }
}