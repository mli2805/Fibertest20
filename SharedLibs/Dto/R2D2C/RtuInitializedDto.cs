using System;
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
        public NetAddress OtdrAddress { get; set; }

        [DataMember]
        public int OwnPortCount { get; set; }

        [DataMember]
        public int FullPortCount { get; set; }

    }
}
