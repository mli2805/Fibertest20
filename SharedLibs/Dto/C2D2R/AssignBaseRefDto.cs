using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class AssignBaseRefDto
    {
        [DataMember]
        public Guid ClientId { get; set; }
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }

        [DataMember]
        public List<BaseRefDto> BaseRefs { get; set; }
    }
}