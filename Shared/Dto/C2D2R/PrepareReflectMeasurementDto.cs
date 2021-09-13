using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class PrepareReflectMeasurementDto
    {
        [DataMember]
        public string ConnectionId { get; set; }
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public string OtdrId { get; set; }
        [DataMember]
        public string OtauId { get; set; }
        [DataMember]
        public int MasterPortNumber { get; set; }
        [DataMember]
        public int PortNumber { get; set; }
    }
}