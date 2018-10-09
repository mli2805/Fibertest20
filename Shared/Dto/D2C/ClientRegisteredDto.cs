using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class CurrentDatacenterSmtpParametersDto
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
    }

    [DataContract]
    public class ClientRegisteredDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public Role Role { get; set; }

        [DataMember]
        public Guid ZoneId { get; set; }

        [DataMember]
        public string ZoneTitle { get; set; }

        [DataMember]
        public Guid GraphDbVersionId { get; set; }

        [DataMember]
        public string DatacenterVersion { get; set; }

        [DataMember]
        public bool IsInGisVisibleMode { get; set; }   

        [DataMember]
        public CurrentDatacenterSmtpParametersDto Smtp { get; set; }  

        [DataMember]
        public int GsmModemComPort { get; set; }
    }
}