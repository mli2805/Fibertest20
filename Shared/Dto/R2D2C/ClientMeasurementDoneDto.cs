using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientMeasurementDoneDto
    {
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}