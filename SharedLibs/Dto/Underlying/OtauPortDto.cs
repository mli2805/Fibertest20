using System;
using System.Runtime.Serialization;

namespace Dto
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
}