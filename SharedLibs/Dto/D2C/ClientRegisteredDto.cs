using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
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