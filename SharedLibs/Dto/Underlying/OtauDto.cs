using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class OtauDto
    {
        [DataMember]
        public string Serial { get; set; }
        [DataMember]
        public NetAddress NetAddress { get; set; }
        [DataMember]
        public int OwnPortCount { get; set; }
        [DataMember]
        public bool IsOk { get; set; }
    }
}