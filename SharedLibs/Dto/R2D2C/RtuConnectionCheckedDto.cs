using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RtuConnectionCheckedDto
    {
        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsRtuInitialized { get; set; } // started && initialized

        [DataMember]
        public bool IsRtuStarted { get; set; } // only started

        [DataMember]
        public bool IsPingSuccessful { get; set; } // is not started
    }
}