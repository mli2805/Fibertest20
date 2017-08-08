using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class BaseRefAssignedDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}