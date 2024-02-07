using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RtuCurrentStateDto : RequestAnswer
    {
        [DataMember]
        public InitializationResult LastInitializationResult { get; set; }
        [DataMember]
        public CurrentMonitoringStepDto CurrentStepDto { get; set; }

        [DataMember]
        public List<MonitoringResultDto> MonitoringResultDtos { get; set; }

        public RtuCurrentStateDto() { }

        public RtuCurrentStateDto(ReturnCode returnCode) : base(returnCode)
        {
        }
    }
}