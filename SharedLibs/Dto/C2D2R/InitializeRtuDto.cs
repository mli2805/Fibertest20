using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class InitializeRtuDto
    {
        [DataMember]
        public string ClientAddress { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public DoubleAddressWithLastConnectionCheck ServerAddresses { get; set; }
        [DataMember]
        public DoubleAddressWithLastConnectionCheck RtuAddresses { get; set; }
    }
}
