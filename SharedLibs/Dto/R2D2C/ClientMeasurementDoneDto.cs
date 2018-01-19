using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientMeasurementDoneDto
    {
        [DataMember]
        public byte[] SorBytes { get; set; }

        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }
    }
}