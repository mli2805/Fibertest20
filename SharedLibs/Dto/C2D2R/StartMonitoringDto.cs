using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class StartMonitoringDto
    {
        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }
    }
}