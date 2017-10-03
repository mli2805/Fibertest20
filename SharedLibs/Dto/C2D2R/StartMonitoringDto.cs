using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class StartMonitoringDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }
    }
}