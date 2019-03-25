using System;

namespace Iit.Fibertest.Dto
{
    public class CurrentDatacenterParameters
    {
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public Guid GraphDbVersionId { get; set; }
        public string DatacenterVersion { get; set; }

        public SmtpSettingsDto Smtp { get; set; }
        public int GsmModemComPort { get; set; }
    }
}