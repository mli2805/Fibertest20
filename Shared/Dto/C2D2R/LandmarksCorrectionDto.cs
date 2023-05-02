using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class LandmarksCorrectionDto
    {
        [DataMember]
        public string ClientIp { get; set; }
        [DataMember]
        public string ConnectionId { get; set; }
        
        [DataMember]
        public Guid BatchId { get; set; }
        [DataMember]
        public List<string> Corrections { get; set; } = new List<string>();
    }
}