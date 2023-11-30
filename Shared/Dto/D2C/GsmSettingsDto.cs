using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class GsmSettingsDto
    {
        [DataMember]
        public int GsmModemPort { get; set; }
        [DataMember]
        public int GsmModemSpeed { get; set; }
        [DataMember]
        public int GsmModemTimeoutMs { get; set; }
    }
}