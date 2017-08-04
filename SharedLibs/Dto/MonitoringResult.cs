using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class MonitoringResult
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public byte[] SorData { get; set; }
    }
}