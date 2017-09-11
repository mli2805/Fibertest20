using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RtuConnectionCheckedDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsRtuInitialized { get; set; } // started && initialized

        [DataMember]
        public bool IsServiceStarted { get; set; } // only started

        [DataMember]
        public bool IsPingSuccessful { get; set; } // is not started
    }
}