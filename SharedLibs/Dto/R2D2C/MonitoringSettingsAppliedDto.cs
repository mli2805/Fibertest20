using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MonitoringSettingsAppliedDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }
    }
}