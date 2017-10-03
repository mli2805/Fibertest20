using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MonitoringStartedDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}