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
}