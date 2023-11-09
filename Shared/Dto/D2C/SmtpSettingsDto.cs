using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class SmtpSettingsDto
    {
        [DataMember]
        public string SmptHost { get; set; }

        [DataMember]
        public int SmptPort { get; set; }

        [DataMember]
        public string MailFrom { get; set; }

        [DataMember]
        public string MailFromPassword { get; set; }

        [DataMember]
        public int SmtpTimeoutMs { get; set; }

        [DataMember]
        public bool SslEnabled { get; set; }
    }
}