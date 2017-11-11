using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class BaseRefDto
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }

        [DataMember]
        public TimeSpan Duration { get; set; } 
    }
}