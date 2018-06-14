using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RtuChecksChannelDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public bool IsMainChannel { get; set; }
    }
}