using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class GetSnapshotDto
    {
        [DataMember]
        public int LastIncludedEvent { get; set; }

     
        [DataMember]
        public string ClientIp { get; set; }

    }

    [DataContract]
    public class SnapshotParamsDto
    {
        [DataMember]
        public int PortionsCount { get; set; }

     
        [DataMember]
        public int Size { get; set; }

    }
}