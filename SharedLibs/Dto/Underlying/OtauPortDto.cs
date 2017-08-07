using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class OtauPortDto
    {
        [DataMember]
        public string Ip { get; set; }

        [DataMember]
        public int TcpPort { get; set; }

        [DataMember]
        public int OpticalPort { get; set; }
    }
}