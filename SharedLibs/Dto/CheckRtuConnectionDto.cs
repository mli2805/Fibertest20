using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class CheckRtuConnectionDto
    {
        [DataMember]
        public string ClientAddress { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }


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