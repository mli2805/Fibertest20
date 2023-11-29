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

    [DataContract]
    public class GsmSettingsDto
    {
        [DataMember]
        public int GsmModemPort { get; set; }
        [DataMember]
        public int GsmModemSpeed { get; set; }
        [DataMember]
        public int GsmModemTimeoutMs { get; set; }
    }
}