using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RtuChecksChannelDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsMainChannel { get; set; }
    }
}