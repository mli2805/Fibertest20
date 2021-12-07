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

        [DataMember]
        public string ConnectionId { get; set; }

    }

    [DataContract]
    public class SnapshotParamsDto
    {
        [DataMember]
        public int PortionsCount { get; set; }


        [DataMember]
        public int Size { get; set; }

    } 
    
    [DataContract]
    public class SerializedModelDto
    {
        [DataMember]
        public int PortionsCount { get; set; }

        [DataMember]
        public int Size { get; set; }

        [DataMember]
        public int LastIncludedEvent { get; set; }


    }
}