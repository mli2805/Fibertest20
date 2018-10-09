using System;

namespace Iit.Fibertest.Client
{
    public class CurrentDatacenterSmtpParameters
    {
        public string SmptHost { get; set; }
        public int SmptPort { get; set; }
        public string MailFrom { get; set; }
        public string MailFromPassword { get; set; }
        public int SmtpTimeoutMs { get; set; }
    }

    public class CurrentDatacenterParameters
    {
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public Guid GraphDbVersionId { get; set; }
        public string DatacenterVersion { get; set; }
        public bool IsInGisVisibleMode { get; set; } = true;
        public CurrentDatacenterSmtpParameters Smtp { get; set; }
        public int GsmModemComPort { get; set; }

    }
}