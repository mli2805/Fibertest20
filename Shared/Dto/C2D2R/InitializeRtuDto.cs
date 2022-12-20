using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class InitializeRtuDto
    {
        [DataMember]
        public string ClientIp { get; set; }
        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public RtuMaker RtuMaker { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }
        [DataMember]
        // public string OtauId { get; set; }
        public VeexOtau MainVeexOtau { get; set; } = new VeexOtau(); // in Veex RTU it is a separate unit

        [DataMember]
        public DoubleAddress ServerAddresses { get; set; }
        [DataMember]
        public DoubleAddress RtuAddresses { get; set; }

        [DataMember]
        public bool IsFirstInitialization { get; set; }

        [DataMember]
        public bool IsSynchronizationRequired { get; set; }

        // RTU properties after previous initialization
        [DataMember]
        public string Serial { get; set; }

        [DataMember]
        public int OwnPortCount { get; set; }

         [DataMember]
        public Dictionary<int, OtauDto> Children { get; set; }
    }
}
