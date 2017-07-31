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

    [DataContract]
    public class RegisterClientDto
    {
        [DataMember]
        public string ClientAddress { get; set; }
        [DataMember]
        public string ClientName { get; set; }
    }

    [DataContract]
    public class UnRegisterClientDto
    {
        [DataMember]
        public string ClientAddress { get; set; }
    }

    [DataContract]
    public class StartMonitoringDto
    {
        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public string RtuAddress { get; set; }
    }

    [DataContract]
    public class StopMonitoringDto
    {
        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public string RtuAddress { get; set; }
    }


}