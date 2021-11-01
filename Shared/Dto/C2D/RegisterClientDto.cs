using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RegisterClientDto
    {
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public DoubleAddress Addresses { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string MachineKey { get; set; }
        [DataMember]
        public string SecurityAdminPassword { get; set; } // Hashed

        [DataMember]
        public bool IsUnderSuperClient { get; set; }

        [DataMember]
        public bool IsWebClient { get; set; }

    }
}