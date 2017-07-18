using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class MonitoringSettingsAppliedDto
    {
        [DataMember]
        public string RtuIpAddress { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}