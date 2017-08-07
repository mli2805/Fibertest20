using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RegisterClientDto
    {
        [DataMember]
        public string ClientAddress { get; set; }
        [DataMember]
        public string ClientName { get; set; }
    }
}