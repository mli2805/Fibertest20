using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class IsPortMonitoringOn
    {
        [DataMember]
        public OtauPortDto Port { get; set; }
        [DataMember]
        public bool IsMonitoringOn { get; set; }
    }
}