using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class UnRegisterClientDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
    }
}