using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class CheckServerConnectionDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
    }
}