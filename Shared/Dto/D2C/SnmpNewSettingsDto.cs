using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class SnmpNewSettingsDto
    {
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public string SnmpVersion { get; set; } = "v1";
        [DataMember]
        public bool UseIitOid { get; set; } = true;
        [DataMember]
        public string CustomOid { get; set; }
        [DataMember]
        public string Community { get; set; }
        [DataMember]
        public string AuthoritativeEngineId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public bool IsAuthPswSet { get; set; }
        [DataMember]
        public string AuthenticationPassword { get; set; }
        [DataMember]
        public string AuthenticationProtocol { get; set; }
        [DataMember]
        public bool IsPrivPswSet { get; set; }
        [DataMember]
        public string PrivacyPassword { get; set; }
        [DataMember]
        public string PrivacyProtocol { get; set; }
        [DataMember]
        public string TrapReceiverAddress { get; set; }
        [DataMember]
        public int TrapReceiverPort { get; set; }


        [DataMember] public string SnmpEncoding { get; set; } = "utf8";
        [DataMember] public string SnmpLanguage { get; set; } = "en-US";
    }
}