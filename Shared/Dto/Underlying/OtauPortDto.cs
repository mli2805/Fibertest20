using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    [DataContract]
    public class OtauPortDto
    {
        [DataMember]
        public string OtauId { get; set; } // in VeEX RTU main OTAU has its own ID, for MAK it is a RTU ID

        [DataMember]
        public NetAddress NetAddress { get; set; } = new NetAddress();
      
        [DataMember]
        public int OpticalPort { get; set; }

        [DataMember] 
        public string Serial { get; set; }

        [DataMember]
        public bool IsPortOnMainCharon { get; set; }

        [DataMember]
        public int MainCharonPort { get; set; } // only for additional otau - port of main otau this otau is connected to
    }
}