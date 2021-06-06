using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class UnRegisterClientDto
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string ConnectionId { get; set; }
        [DataMember]
        public string ClientIp { get; set; }
    }
}