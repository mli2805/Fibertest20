using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RtuInitializedDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsInitialized { get; set; }

        [DataMember]
        public string Serial { get; set; }

        [DataMember]
        public DoubleAddress PcDoubleAddress { get; set; }

        [DataMember]
        public NetAddress OtdrAddress { get; set; }

        [DataMember]
        public int OwnPortCount { get; set; }

        [DataMember]
        public int FullPortCount { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public Dictionary<int, OtauDto> Children { get; set; }

    }
}
