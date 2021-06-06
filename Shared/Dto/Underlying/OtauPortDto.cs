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
        public bool IsPortOnMainCharon { get; set; }

        [DataMember] 
        public string Serial { get; set; }

        [DataMember]
        public string OtauId { get; set; } // in VeEX RTU main OTAU has its own ID
    }
}