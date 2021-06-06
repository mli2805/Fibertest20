using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DoOutOfTurnPreciseMeasurementDto
    {
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public PortWithTraceDto PortWithTraceDto { get; set; }

    }
}