using System;

namespace Iit.Fibertest.Dto
{
    public class CurrentDatacenterParameters
    {
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public Guid StreamIdOriginal { get; set; }
        public int SnapshotLastEvent { get; set; }
        public DateTime SnapshotLastDate { get; set; }
        public string DatacenterVersion { get; set; }

        public string WebApiDomainName { get; set; }
        public string WebApiBindingProtocol { get; set; }

        public SmtpSettingsDto Smtp { get; set; }
        public int GsmModemComPort { get; set; }
        public SnmpSettingsDto Snmp { get; set; }

        public void FillIn(ClientRegisteredDto dto)
        {
            DatacenterVersion = dto.DatacenterVersion;
            StreamIdOriginal = dto.StreamIdOriginal;
            SnapshotLastEvent = dto.SnapshotLastEvent;
            SnapshotLastDate = dto.SnapshotLastDate;
            Smtp = new SmtpSettingsDto()
            {
                SmptHost = dto.Smtp.SmptHost,
                SmptPort = dto.Smtp.SmptPort,
                MailFrom = dto.Smtp.MailFrom,
                MailFromPassword = dto.Smtp.MailFromPassword,
                SmtpTimeoutMs = dto.Smtp.SmtpTimeoutMs,
                SslEnabled = dto.Smtp.SslEnabled,
            };
            GsmModemComPort = dto.GsmModemComPort;
            Snmp = new SnmpSettingsDto()
            {
                IsSnmpOn = dto.Snmp.IsSnmpOn,
                SnmpTrapVersion = dto.Snmp.SnmpTrapVersion,
                SnmpReceiverIp = dto.Snmp.SnmpReceiverIp,
                SnmpReceiverPort = dto.Snmp.SnmpReceiverPort,
                SnmpAgentIp = dto.Snmp.SnmpAgentIp,
                SnmpCommunity = dto.Snmp.SnmpCommunity,
                EnterpriseOid = dto.Snmp.EnterpriseOid,
                SnmpEncoding = dto.Snmp.SnmpEncoding,
            };
        }
    }


}