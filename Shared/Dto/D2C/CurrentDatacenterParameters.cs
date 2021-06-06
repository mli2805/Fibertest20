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
    }
}