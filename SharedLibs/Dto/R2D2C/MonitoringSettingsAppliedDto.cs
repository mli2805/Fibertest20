using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class MonitoringSettingsAppliedDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}