using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    [Serializable]
    public class BopStateChangedDto
    {
        [DataMember]
        public Guid RtuId { get; set; }
        [DataMember]
        public string OtauIp { get; set; }
        [DataMember]
        public bool IsOk { get; set; }
            
    }
}