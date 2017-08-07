using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class StopMonitoringDto
    {
        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public string RtuAddress { get; set; }
    }
}