using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RtuCurrentStateDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public InitializationResult LastInitializationResult { get; set; }
        [DataMember]
        public CurrentMonitoringStepDto CurrentStepDto { get; set; }

        public RtuCurrentStateDto() { }

        public RtuCurrentStateDto(ReturnCode returnCode)
        {
            ReturnCode = returnCode;
        }
    }
}