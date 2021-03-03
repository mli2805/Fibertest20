using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DoClientMeasurementDto
    {
        [DataMember]
        public string ConnectionId { get; set; }
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public SelectedMeasParams SelectedMeasParams { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }

        // only to show message on display
        [DataMember]
        public string OtauIp { get; set; }

        [DataMember]
        public int OtauTcpPort { get; set; }

    }
}