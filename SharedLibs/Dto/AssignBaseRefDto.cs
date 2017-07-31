using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class AssignBaseRefDto
    {
        [DataMember]
        public string ClientAddress { get; set; }
        [DataMember]
        public string RtuIpAddress { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }

        [DataMember]
        public List<BaseRefDto> BaseRefs { get; set; }
    }
}