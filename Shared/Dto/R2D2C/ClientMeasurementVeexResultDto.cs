using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientMeasurementVeexResultDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string VeexMeasurementStatus { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}