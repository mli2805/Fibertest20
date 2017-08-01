using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class RtuConnectionCheckedDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsPingSuccessful { get; set; }

        [DataMember]
        public bool IsRtuInitialized { get; set; }

        [DataMember]
        public bool IsRtuConnectionSuccessful { get; set; }

    }
}