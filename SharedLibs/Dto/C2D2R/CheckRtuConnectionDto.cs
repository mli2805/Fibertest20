using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class CheckRtuConnectionDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }


        [DataMember]
        public NetAddress NetAddress { get; set; }
    }
}