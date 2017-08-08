using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RtuCommandDeliveredDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public MessageProcessingResult MessageProcessingResult { get; set; }
    }
}