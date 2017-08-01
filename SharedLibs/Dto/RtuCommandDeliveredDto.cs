using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RtuCommandDeliveredDto
    {
        [DataMember]
        public string RtuAddress { get; set; }

        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public MessageProcessingResult MessageProcessingResult { get; set; }
    }
}