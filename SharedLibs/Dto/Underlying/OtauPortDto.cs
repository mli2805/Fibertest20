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
        public bool IsPortOnMainCharon { get; set; } // if true - value of OtauIp will be ignored

        [DataMember]
        public string OtauIp { get; set; }

        [DataMember]
        public int OtauTcpPort { get; set; }
    }

    [DataContract]
    public class OtauDto
    {
        [DataMember]
        public string Serial { get; set; }
        [DataMember]
        public NetAddress NetAddress { get; set; }
        [DataMember]
        public int OwnPortCount { get; set; }
    }
}