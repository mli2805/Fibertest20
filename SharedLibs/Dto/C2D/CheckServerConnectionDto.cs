using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class CheckServerConnectionDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
    }
}