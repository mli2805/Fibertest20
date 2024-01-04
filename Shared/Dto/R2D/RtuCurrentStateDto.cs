using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RtuCurrentStateDto : RequestAnswer
    {
        [DataMember]
        public Guid RtuId { get; set; }
       
        [DataMember]
        public bool IsRtuInitialized { get; set; }
        [DataMember]
        public bool IsMonitoringOn { get; set; }
        [DataMember]
        public CurrentMonitoringStepDto CurrentStepDto { get; set; }

        [DataMember]
        public List<string> MessagesJson { get; set; } // MonitoringResultDto, BopStatusChangedDto

        public RtuCurrentStateDto() {}

        public RtuCurrentStateDto(ReturnCode returnCode) : base(returnCode)
        {
        }
    }
}