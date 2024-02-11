using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientMeasurementStartedDto : RequestAnswer
    {
        [DataMember]
        public Guid ClientMeasurementId { get; set; }

        [DataMember]
        public Guid TraceId { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }

        public ClientMeasurementStartedDto()
        {
        }

        public ClientMeasurementStartedDto(ReturnCode returnCode) : base(returnCode)
        {
        }
    }
}