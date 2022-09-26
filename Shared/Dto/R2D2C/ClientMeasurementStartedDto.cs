using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientMeasurementStartedDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public RtuOccupationState RtuOccupationState { get; set; }

        [DataMember]
        public Guid ClientMeasurementId { get; set; }

        [DataMember]
        public Guid TraceId { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }

    }
}