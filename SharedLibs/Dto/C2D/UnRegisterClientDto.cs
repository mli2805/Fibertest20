using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class UnRegisterClientDto
    {
        [DataMember]
        public string ClientAddress { get; set; }
    }
}