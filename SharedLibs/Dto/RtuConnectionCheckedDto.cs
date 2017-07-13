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
        public bool IsSuccessful { get; set; }

    }
}