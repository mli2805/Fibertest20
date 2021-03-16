using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientRegisteredDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public Role Role { get; set; }

        [DataMember]
        public Guid ZoneId { get; set; }

        [DataMember]
        public string ZoneTitle { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public Guid StreamIdOriginal { get; set; }

        [DataMember]
        public int SnapshotLastEvent { get; set; }

        [DataMember]
        public DateTime SnapshotLastDate { get; set; }

        [DataMember]
        public string DatacenterVersion { get; set; }

        [DataMember]
        public bool IsWithoutMapMode { get; set; }

        [DataMember]
        public SmtpSettingsDto Smtp { get; set; }

        [DataMember]
        public int GsmModemComPort { get; set; }

        [DataMember]
        public SnmpSettingsDto Snmp { get; set; }
    }
}