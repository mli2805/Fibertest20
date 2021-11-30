using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DoOutOfTurnPreciseMeasurementDto
    {
        [DataMember]
        public string ConnectionId { get; set; }
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public PortWithTraceDto PortWithTraceDto { get; set; }

        [DataMember]
        public bool IsOltTrapCaused { get; set; } // false means user's measurement
    }
}