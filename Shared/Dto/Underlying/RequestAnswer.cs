using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    public class RequestAnswer
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public RtuOccupationState RtuOccupationState { get; set; }

        public RequestAnswer()
        {
        }

        public RequestAnswer(ReturnCode returnCode)
        {
            ReturnCode = returnCode;
        }
    }
}
