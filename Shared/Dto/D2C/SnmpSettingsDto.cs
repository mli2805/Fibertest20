using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class SnmpSettingsDto
    {
        [DataMember]
        public bool IsSnmpOn { get; set; }
        [DataMember]
        public string SnmpTrapVersion { get; set; }
        [DataMember]
        public string SnmpReceiverIp { get; set; }
        [DataMember]
        public int SnmpReceiverPort { get; set; }
        [DataMember]
        public string SnmpAgentIp { get; set; }
        [DataMember]
        public string SnmpCommunity { get; set; }
        [DataMember]
        public string EnterpriseOid { get; set; }
        [DataMember]
        public string SnmpEncoding { get; set; }
    }
}