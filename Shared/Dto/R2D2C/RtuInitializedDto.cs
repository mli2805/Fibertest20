using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RtuInitializedDto
    {
        [DataMember]
        public RtuMaker Maker { get; set; }

        [DataMember]
        public string Mfid { get; set; }
        [DataMember]
        public string Mfsn { get; set; }
        [DataMember]
        public string Omid { get; set; }
        [DataMember]
        public string Omsn { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsInitialized { get; set; }
        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public DoubleAddress RtuAddresses { get; set; }

        [DataMember]
        public NetAddress OtdrAddress { get; set; }

        [DataMember]
        public string Serial { get; set; }
        [DataMember]
        public int OwnPortCount { get; set; }
        [DataMember]
        public int FullPortCount { get; set; }
        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public Dictionary<int, OtauDto> Children { get; set; }

        [DataMember]
        public bool IsMonitoringOn { get; set; }

        [DataMember]
        public TreeOfAcceptableMeasParams AcceptableMeasParams { get; set; }
    }
}
