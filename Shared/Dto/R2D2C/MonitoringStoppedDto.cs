using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MonitoringStoppedDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }

        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }
}