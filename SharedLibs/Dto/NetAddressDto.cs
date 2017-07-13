using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class NetAddressDto
    {
        [DataMember]
        public string Ip4Address { get; set; } // 172.35.98.128
        [DataMember]
        public string HostName { get; set; } // domain.beltelecom.by 
        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public bool IsAddressSetAsIp { get; set; }
    }
}