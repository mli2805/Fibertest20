using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientMeasurementDoneDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}