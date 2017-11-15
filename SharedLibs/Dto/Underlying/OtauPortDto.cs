using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    [DataContract]
    public class OtauPortDto
    {
        [DataMember]
        public int OpticalPort { get; set; }

        [DataMember]
        public bool IsPortOnMainCharon { get; set; } // if true - value of OtauIp is inner IP of otdr(otau)  ex. 192.168.88.101
        [DataMember]
        public string OtauIp { get; set; }

        [DataMember]
        public int OtauTcpPort { get; set; }
    }

    [Serializable]
    [DataContract]
    public class PortWithTraceDto
    {
        [DataMember]
        public OtauPortDto OtauPort { get; set; }

        [DataMember]
        public Guid TraceId { get; set; }
    }
}