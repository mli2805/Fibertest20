using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class InitializeRtuDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public DoubleAddress ServerAddresses { get; set; }
        [DataMember]
        public DoubleAddress RtuAddresses { get; set; }
    }
}
