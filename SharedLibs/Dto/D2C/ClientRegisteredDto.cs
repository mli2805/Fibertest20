using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class ClientRegisteredDto
    {
        [DataMember]
        public bool IsRegistered { get; set; }

        [DataMember]
        public int ErrorCode { get; set; }
    }
}